using Jupiter.Utils.Extensions;
using Meridian.Interfaces;
using Meridian.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkLib.Core.Attachments;
using VkLib.Core.Audio;
using Meridian.Utils.Extensions;
using VkLib.Core.Groups;
using VkLib.Error;
using GalaSoft.MvvmLight.Messaging;
using Meridian.Utils.Messaging;

namespace Meridian.Services.VK
{
    public class VkTracksService
    {
        private readonly VkLib.Vk _vk;
        //private readonly SQLiteAsyncConnection _db;

        public VkTracksService(VkLib.Vk vk/*, SQLiteAsyncConnection db*/)
        {
            _vk = vk;
            //_db = db;
        }

        public async Task<List<IAudio>> GetTracks(long userId = 0, long albumId = 0, int count = 0, string accessKey = null)
        {
            List<IAudio> result = null;

            try
            {
                var response = await _vk.Audio.Get(ownerId: userId, albumId: albumId, count: count, accessKey: accessKey);

                if (response?.Items.IsNullOrEmpty() == false)
                {
                    var tracks = ProcessTracks(response.Items);

                    //update db
                    //await _db.InsertAllAsync(tracks);

                    result = tracks;
                }
            }
            catch (VkInvalidTokenException)
            {
                Messenger.Default.Send(new MessageUserAuthChanged { IsLoggedIn = false });
            }
            catch (VkFloodControlException)
            {
                Messenger.Default.Send(new MessageUserAuthChanged { IsLoggedIn = false });
            }

            //if (result == null)
            //    result = (await _db.Table<AudioVk>().ToListAsync()).OfType<IAudio>().ToList();

            return result;
        }

        /// <summary>
        /// Return playlists of specified user
        /// </summary>
        /// <param name="userId">User id</param>
        /// <param name="tracks">List of tracks to calculate number of tracks in each playlist</param>
        public async Task<(List<IPlaylist> Playlists, int TotalCount)> GetPlaylists(long userId = 0, int count = 0, int offset = 0, IList<IAudio> tracks = null)
        {
            (List<IPlaylist> playlists, int totalCount) result = (null, 0);

            try
            {
                var response = await _vk.Audio.GetPlaylists(ownerId: userId != 0 ? userId : _vk.AccessToken.UserId, count: count, offset: offset);

                if (response?.Items.IsNullOrEmpty() == false)
                {
                    var playlists = ProcessPlaylists(response.Items);

                    result = (playlists, response.TotalCount);
                }
            }
            catch (VkInvalidTokenException)
            {
                Messenger.Default.Send(new MessageUserAuthChanged { IsLoggedIn = false });
            }

            return result;
        }

        public async Task<bool> FollowPlaylist(PlaylistVk playlist)
        {
            try
            {
                var response = await _vk.Audio.FollowPlaylist(ownerId: long.Parse(playlist.OwnerId), playlistId: long.Parse(playlist.Id), accessKey: playlist.AccessKey);
                return response != 0;
            }
            catch (VkInvalidTokenException)
            {
                Messenger.Default.Send(new MessageUserAuthChanged { IsLoggedIn = false });
            }

            return false;
        }

        public async Task<bool> DeletePlaylist(PlaylistVk playlist)
        {
            try
            {
                return await _vk.Audio.DeletePlaylist(ownerId: long.Parse(playlist.OwnerId), playlistId: long.Parse(playlist.Id));
            }
            catch (VkInvalidTokenException)
            {
                Messenger.Default.Send(new MessageUserAuthChanged { IsLoggedIn = false });
            }

            return false;
        }

        public async Task<(List<AudioPost> Posts, string NextFrom)> GetNews(int count = 50, string nextFrom = null)
        {
            (List<AudioPost> Posts, string NextFrom) result = (null, null);

            try
            {
                var response = await _vk.News.Get(filters: "post", count: count, startFrom: nextFrom);

                if (response?.Items.IsNullOrEmpty() == false)
                {
                    var audioIds = new List<string>();
                    var posts = new List<AudioPost>();
                    var postAudioMatches = new Dictionary<string, IList<string>>();

                    foreach (var newsEntry in response.Items)
                    {
                        var attachments = newsEntry.Attachments;

                        //if there are no attachments of this post, but there is repost history, take attachments from last repost
                        if (attachments.IsNullOrEmpty() && !newsEntry.CopyHistory.IsNullOrEmpty())
                            attachments = newsEntry.CopyHistory.Last().Attachments;

                        if (attachments.IsNullOrEmpty())
                            continue;

                        var ids = attachments.Where(a => a is VkAudioAttachment).Select(a => $"{a.OwnerId}_{a.Id}").ToList();
                        audioIds.AddRange(ids);

                        var post = new AudioPost();
                        post.Id = newsEntry.Id.ToString();
                        post.Text = newsEntry.Text;
                        post.PostUri = new Uri($"http://vk.com/wall{newsEntry.SourceId}_{post.Id}");
                        post.AuthorUri = new Uri(string.Format("https://vk.com/{0}{1}", newsEntry.Author is VkGroup ? "club" : "id", newsEntry.Author.Id));

                        var imageUrl = newsEntry.Attachments?.OfType<VkPhotoAttachment>().FirstOrDefault()?.SourceMax;
                        if (!string.IsNullOrEmpty(imageUrl))
                            post.ImageUri = new Uri(imageUrl);

                        post.Author = newsEntry.Author;
                        post.Date = newsEntry.Date;
                        posts.Add(post);

                        postAudioMatches.Add(post.Id, ids);
                    }

                    if (audioIds.Count == 0)
                        return result;

                    var tracks = new List<IAudio>();

                    var audioIdsChunks = audioIds.Split(100); //split ids by chunks of 100 ids (api restriction)
                    foreach (var audioIdsChunk in audioIdsChunks)
                    {
                        var resultAudios = await _vk.Audio.GetById(audioIdsChunk.ToList());
                        if (!resultAudios.IsNullOrEmpty())
                            tracks.AddRange(ProcessTracks(resultAudios));
                    }

                    foreach (var post in posts)
                    {
                        post.Tracks = tracks.Where(t => postAudioMatches[post.Id].Contains($"{t.OwnerId}_{t.Id}")).ToList();
                    }

                    result.Posts = posts.Where(p => p.Tracks.Count > 0).ToList();
                    result.NextFrom = response.NextFrom;
                }
            }
            catch (VkInvalidTokenException)
            {
                Messenger.Default.Send(new MessageUserAuthChanged { IsLoggedIn = false });
            }

            return result;
        }

        public async Task<(List<AudioPost> Posts, int TotalCount)> GetWallPosts(int count = 300, int offset = 0, long ownerId = 0)
        {
            (List<AudioPost> Posts, int TotalCount) result = (null, 0);

            try
            {
                var response = await _vk.Wall.Get(ownerId: ownerId, filter: "all", count: count, offset: offset);

                if (response?.Items.IsNullOrEmpty() == false)
                {
                    var audioIds = new List<string>();
                    var posts = new List<AudioPost>();
                    var postAudioMatches = new Dictionary<string, IList<string>>();

                    foreach (var newsEntry in response.Items)
                    {
                        var attachments = newsEntry.Attachments;

                        //if there are no attachments of this post, but there is repost history, take attachments from last repost
                        if (attachments.IsNullOrEmpty() && !newsEntry.CopyHistory.IsNullOrEmpty())
                            attachments = newsEntry.CopyHistory.Last().Attachments;

                        if (attachments.IsNullOrEmpty())
                            continue;

                        var ids = attachments.Where(a => a is VkAudioAttachment).Select(a => $"{a.OwnerId}_{a.Id}").ToList();
                        audioIds.AddRange(ids);

                        var post = new AudioPost();
                        post.Id = newsEntry.Id.ToString();
                        post.Text = newsEntry.Text;
                        post.PostUri = new Uri($"http://vk.com/wall{newsEntry.SourceId}_{post.Id}");
                        post.AuthorUri = new Uri(string.Format("https://vk.com/{0}{1}", newsEntry.Author is VkGroup ? "club" : "id", newsEntry.Author.Id));

                        var imageUrl = newsEntry.Attachments?.OfType<VkPhotoAttachment>().FirstOrDefault()?.SourceMax;
                        if (!string.IsNullOrEmpty(imageUrl))
                            post.ImageUri = new Uri(imageUrl);

                        post.Author = newsEntry.Author;
                        post.Date = newsEntry.Date;
                        posts.Add(post);

                        postAudioMatches.Add(post.Id, ids);
                    }

                    if (audioIds.Count == 0)
                        return result;

                    var tracks = new List<IAudio>();

                    var audioIdsChunks = audioIds.Split(100); //split ids by chunks of 100 ids (api restriction)
                    foreach (var audioIdsChunk in audioIdsChunks)
                    {
                        var resultAudios = await _vk.Audio.GetById(audioIdsChunk.ToList());
                        if (!resultAudios.IsNullOrEmpty())
                            tracks.AddRange(ProcessTracks(resultAudios));
                    }

                    foreach (var post in posts)
                    {
                        post.Tracks = tracks.Where(t => postAudioMatches[post.Id].Contains($"{t.OwnerId}_{t.Id}")).ToList();
                    }

                    result.Posts = posts.Where(p => p.Tracks.Count > 0).ToList();
                    response.TotalCount = response.TotalCount;
                }
            }
            catch (VkInvalidTokenException)
            {
                Messenger.Default.Send(new MessageUserAuthChanged { IsLoggedIn = false });
            }

            return result;
        }

        /// <summary>
        /// Reorder tracks
        /// </summary>
        /// <param name="trackId">Track id</param>
        /// <param name="beforeTrackId">Next track id</param>
        /// <param name="afterTrackId">Previous track id</param>
        public async Task<bool> ReorderTracks(IAudio track, string beforeTrackId, string afterTrackId)
        {
            return await _vk.Audio.Reorder(long.Parse(track.Id), long.Parse(afterTrackId), long.Parse(beforeTrackId));
        }

        /// <summary>
        /// Get popular tracks
        /// </summary>
        public async Task<List<IAudio>> GetPopularTracks(bool foreignOnly = false, int count = 0, int offset = 0, int genreId = 0)
        {
            List<IAudio> result = null;

            try
            {
                var response = await _vk.Audio.GetPopular(onlyEng: foreignOnly, count: count, offset: offset, genreId: genreId);

                if (response?.Items.IsNullOrEmpty() == false)
                {
                    var tracks = ProcessTracks(response.Items);

                    result = tracks;
                }
            }
            catch (VkInvalidTokenException)
            {
                Messenger.Default.Send(new MessageUserAuthChanged { IsLoggedIn = false });
            }


            return result;
        }

        /// <summary>
        /// Search tracks
        /// </summary>
        public async Task<(List<IAudio> Tracks, int TotalCount)> SearchTracks(string query, int count = 0, int offset = 0)
        {
            (List<IAudio> Tracks, int TotalCount) result = (null, 0);

            try
            {
                var response = await _vk.Audio.Search(query, count, offset);

                if (response?.Items.IsNullOrEmpty() == false)
                {
                    var tracks = ProcessTracks(response.Items);

                    result = (tracks, response.TotalCount);
                }
            }
            catch (VkInvalidTokenException)
            {
                Messenger.Default.Send(new MessageUserAuthChanged { IsLoggedIn = false });
            }

            return result;
        }

        /// <summary>
        /// Get list of genres
        /// </summary>
        /// <returns></returns>
        public List<VkGenre> GetGenres()
        {
            return _vk.Audio.GetGenres();
        }

        /// <summary>
        /// Add track to my music
        /// </summary>
        public async Task<bool> AddTrack(AudioVk track)
        {
            var newId = await _vk.Audio.Add(long.Parse(track.Id), long.Parse(track.OwnerId));

            if (newId > 0)
            {
                track.Id = newId.ToString();
                track.OwnerId = _vk.AccessToken?.UserId.ToString();
                track.IsAddedByCurrentUser = true;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove track from my music
        /// </summary>
        /// <param name="audio"></param>
        /// <returns></returns>
        public async Task<bool> RemoveTrack(AudioVk track)
        {
            var result = await _vk.Audio.Delete(long.Parse(track.Id), long.Parse(track.OwnerId));
            if (result)
            {
                track.IsAddedByCurrentUser = false;
            }

            return result;
        }

        /// <summary>
        /// Get track lyrics
        /// </summary>
        public async Task<string> GetTrackLyrics(long lyricsId)
        {
            return await _vk.Audio.GetLyrics(lyricsId);
        }

        /// <summary>
        /// Edit track info
        /// </summary>
        /// <returns>Returns lyrics id or 0</returns>
        public async Task<long> EditTrack(AudioVk track, string title, string artist, string lyrics = null)
        {
            return await _vk.Audio.Edit(ownerId: long.Parse(track.OwnerId), audioId: long.Parse(track.Id), artist: artist, title: title, text: lyrics);
        }

        /// <summary>
        /// Edit playlist
        /// </summary>
        /// <returns>True if success, otherwise false</returns>
        public async Task<bool> EditPlaylist(PlaylistVk playlist, string title)
        {
            return await _vk.Audio.EditAlbum(playlist.Id, title);
        }

        /// <summary>
        /// Add new playlist
        /// </summary>
        public async Task<long> AddPlaylist(string title)
        {
            return await _vk.Audio.AddAlbum(title);
        }

        /// <summary>
        /// Add tracks to playlist
        /// </summary>
        public async Task<bool> AddTracksToPlaylist(List<AudioVk> tracks, string playlistId)
        {
            return await _vk.Audio.MoveToAlbum(long.Parse(playlistId), tracks.Select(t => long.Parse(t.Id)).ToList());
        }

        #region Recommendations

        public async Task<List<CatalogBlock>> GetPersonalRecommendations()
        {
            var blocks = await _vk.Audio.GetCatalog();

            if (blocks != null)
                return blocks.Select(b => new CatalogBlock(b, tracks: b.Audios != null ? ProcessTracks(b.Audios) : null, 
                                                              playlists: b.Playlists != null ? ProcessPlaylists(b.Playlists) : null,
                                                              extendedPlaylists: b.ExtendedPlaylists != null ? 
                                                                                 ProcessPlaylists(b.ExtendedPlaylists.Select(p => p.Playlist).ToList()) : null)).ToList();

            return null;
        }

        #endregion

        private List<IAudio> ProcessTracks(List<VkAudio> tracks)
        {
            var result = new List<IAudio>();
            foreach (var vkTrack in tracks)
            {
                var track = new AudioVk(vkTrack);
                track.IsAddedByCurrentUser = track.OwnerId == _vk.AccessToken?.UserId.ToString();
                result.Add(track);
            }

            return result;
        }

        private List<IPlaylist> ProcessPlaylists(List<VkPlaylist> playlists)
        {
            return playlists.Select(a => new PlaylistVk(a)
            {
                IsAddedByCurrentUser = a.OwnerId == _vk.AccessToken?.UserId
            }).OfType<IPlaylist>().ToList();
        }
    }
}