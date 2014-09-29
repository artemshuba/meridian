using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LastFmLib;
using LastFmLib.Core.Album;
using LastFmLib.Core.Artist;
using Meridian.Controls;
using Meridian.Domain;
using Meridian.Extensions;
using Meridian.Helpers;
using Meridian.Model;
using Meridian.Resources.Localization;
using Meridian.View.Flyouts;
using Meridian.ViewModel;
using VkLib;
using VkLib.Core.Attachments;
using VkLib.Core.Audio;
using VkLib.Core.Friends;
using VkLib.Core.Groups;
using VkLib.Core.Users;
using VkLib.Error;
using Xbox.Music;
using DateTimeConverter = Meridian.Helpers.DateTimeConverter;
using VkAudio = Meridian.Model.VkAudio;

namespace Meridian.Services
{
    public static class DataService
    {
        private static readonly Vkontakte _vkontakte;
        private static readonly LastFm _lastFm;
        private static readonly MusicClient _xboxMusic = new MusicClient("Meridian", "u6QLSdNTIS9lrjk306Q1EdsAsHHM3fIk+FYgNTRZrhs=");

        static DataService()
        {
            _vkontakte = ViewModelLocator.Vkontakte;
            _lastFm = ViewModelLocator.LastFm;
        }

        public static async Task<VkProfile> GetUserInfo()
        {
            try
            {
                var info = await _vkontakte.Users.Get(_vkontakte.AccessToken.UserId, "photo");
                return info;
            }
            catch (VkInvalidTokenException)
            {
                Settings.Instance.AccessToken = null;
                Settings.Instance.Save();

                AccountManager.LogOutVk();
            }

            return null;
        }

        public static async Task<ItemsResponse<VkAudioAlbum>> GetUserAlbums(int count = 0, int offset = 0, long ownerId = 0)
        {
            try
            {
                var response = await _vkontakte.Audio.GetAlbums(ownerId, count, offset);
                if (response.Items != null)
                {
                    return new ItemsResponse<VkAudioAlbum>(response.Items, response.TotalCount);
                }
            }
            catch (VkInvalidTokenException)
            {
                Settings.Instance.AccessToken = null;
                Settings.Instance.Save();

                AccountManager.LogOutVk();
            }

            return ItemsResponse<VkAudioAlbum>.Empty;
        }

        public static async Task<ItemsResponse<VkAudio>> GetUserTracks(int count = 0, int offset = 0, long albumId = 0, long ownerId = 0)
        {
            try
            {
                var response = await _vkontakte.Audio.Get(ownerId, albumId, count, offset);
                if (response.Items != null)
                {
                    return new ItemsResponse<VkAudio>(response.Items.Select(i => i.ToAudio()).ToList(), response.TotalCount);
                }
            }
            catch (VkInvalidTokenException)
            {
                Settings.Instance.AccessToken = null;
                Settings.Instance.Save();

                AccountManager.LogOutVk();
            }


            return ItemsResponse<VkAudio>.Empty;
        }

        public static async Task<ItemsResponse<VkAudio>> GetPopularTracks(int count = 0, int offset = 0)
        {
            var response = await _vkontakte.Audio.GetPopular(count: count, offset: offset);
            if (response.Items != null)
            {
                return new ItemsResponse<VkAudio>(response.Items.Select(i => i.ToAudio()).ToList(), response.TotalCount);
            }

            return ItemsResponse<VkAudio>.Empty;
        }

        public static async Task<ItemsResponse<VkProfile>> GetFriends(int count = 0, int offset = 0, long userId = 0, string fields = null)
        {
            var response = await _vkontakte.Friends.Get(userId, fields, null, count, offset, FriendsOrder.ByRating);
            if (response.Items != null)
            {
                return new ItemsResponse<VkProfile>(response.Items, response.TotalCount);
            }

            return ItemsResponse<VkProfile>.Empty;
        }

        public static async Task<ItemsResponse<VkProfile>> GetSubscriptions(int count = 0, int offset = 0, string fields = null)
        {
            var response = await _vkontakte.Subscriptions.Get();
            if (response.Items != null && response.Items.Count > 0)
            {
                var users = await _vkontakte.Users.Get(response.Items.Select(s => s.Id.ToString()), fields);
                if (users.Items != null)
                {
                    return new ItemsResponse<VkProfile>(users.Items, users.TotalCount);
                }
            }

            return ItemsResponse<VkProfile>.Empty;
        }

        public static async Task<ItemsResponse<VkGroup>> GetSocieties(int count = 0, int offset = 0, long userId = 0, string fields = null)
        {
            var response = await _vkontakte.Groups.Get(userId, fields, null, count, offset);
            if (response.Items != null)
            {
                return new ItemsResponse<VkGroup>(response.Items, response.TotalCount);
            }

            return ItemsResponse<VkGroup>.Empty;
        }

        public static async Task<List<VkAudio>> GetRecommendations(int count = 0, int offset = 0)
        {
            var vkAudios = await _vkontakte.Audio.GetRecommendations(count: count, offset: offset);
            if (vkAudios.Items != null)
            {
                var result = (from a in vkAudios.Items
                              select a.ToAudio()).ToList();

                return result;
            }

            return null;
        }

        public static async Task<Uri> GetArtistImage(string artist, bool big)
        {
            if (artist.Contains(", ") || artist.Contains(" feat ", StringComparison.OrdinalIgnoreCase) || artist.Contains(" ft. ", StringComparison.OrdinalIgnoreCase))
            {
                //if there are more then 1 artist, get first
                var artists = artist.Split(new[] {", ", " feat. ", " feat ", " ft. "}, StringSplitOptions.RemoveEmptyEntries);
                if (artists.Length > 0)
                    artist = artists.First();
            }

            if (big)
            {
                try
                {
                    var results = await _xboxMusic.Find(artist);
                    if (results != null && results.Artists != null)
                    {
                        var resultArtist = results.Artists.Items.FirstOrDefault();
                        if (resultArtist != null && !string.IsNullOrEmpty(resultArtist.ImageUrl))
                        {
                            var imageUrl = resultArtist.ImageUrl;

                            var httpClient = new HttpClient();
                            var response = await httpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);
                            if (response.IsSuccessStatusCode)
                                return new Uri(resultArtist.ImageUrl);
                        }
                    }
                }
                catch (Exception)
                {
                    Debug.WriteLine("Xbox Music: Artist " + artist + " not found.");
                }
            }

            var info = await _lastFm.Artist.GetInfo(null, artist);
            if (info == null || string.IsNullOrEmpty(big ? info.ImageMega : info.ImageExtraLarge))
                return null;

            return new Uri(big ? info.ImageMega : info.ImageExtraLarge);
        }

        public static async Task<Uri> GetTrackImage(string artist, string title)
        {
            var info = await _lastFm.Track.GetInfo(title, artist);
            if (info == null || info.ImageExtraLarge == null || string.IsNullOrEmpty(info.ImageExtraLarge))
                return null;

            return new Uri(info.ImageExtraLarge);
        }

        public static async Task<List<Audio>> GetTagTopTracks(string tag, int count = 50)
        {
            var tracks = await _lastFm.Tag.GetTopTracks(tag, count);
            if (tracks != null)
            {
                return (from track in tracks
                        select new Audio()
                        {
                            Title = track.Title,
                            Artist = track.Artist,
                            Duration = TimeSpan.FromSeconds(track.Duration)
                        }).ToList();
            }

            return null;
        }

        public static async Task<VkAudio> GetAudioByArtistAndTitle(string artist, string title)
        {
            var audios = await SearchAudio(artist + " - " + title, 10, 0);
            if (audios != null && audios.Count > 0)
            {
                var audio = audios.FirstOrDefault(x => (String.Equals(x.Title, title, StringComparison.OrdinalIgnoreCase) && String.Equals(x.Artist, artist, StringComparison.OrdinalIgnoreCase)));
                if (audio == null)
                    audio = audios.FirstOrDefault(x => String.Equals(x.Title, title, StringComparison.OrdinalIgnoreCase));
                if (audio == null)
                {
                    audio = audios.First();
                }

                return audio;
            }
            else
            {
                bool searchAgain = false;
                if (artist.Contains("(") && artist.Contains(")"))
                {
                    artist = artist.Substring(0, artist.IndexOf("(")) + artist.Substring(artist.LastIndexOf(")") + 1);
                    searchAgain = true;
                }

                if (title.Contains("(") && title.Contains(")"))
                {
                    title = title.Substring(0, title.IndexOf("(")) + title.Substring(title.LastIndexOf(")") + 1);
                    searchAgain = true;
                }

                if (searchAgain)
                    return await GetAudioByArtistAndTitle(artist, title);
            }

            return null;
        }

        public static async Task<List<VkAudio>> SearchAudio(string query, int count = 0, int offset = 0)
        {
            var vkAudios = await _vkontakte.Audio.Search(query, count, offset, VkAudioSortType.DateAdded, false, false);
            if (vkAudios.Items != null)
            {
                var result = (from a in vkAudios.Items
                              select a.ToAudio()).ToList();

                return result;
            }

            return null;
        }

        public static async Task<List<LastFmAlbum>> SearchAlbums(string query)
        {
            var albums = await _lastFm.Album.Search(query);

            return albums;
        }

        public static async Task<List<LastFmArtist>> SearchArtists(string query)
        {
            var artists = await _lastFm.Artist.Search(query);

            return artists;
        }

        public static async Task<LastFmAlbum> GetAlbumInfo(string id, string name, string artist, bool loadTracks = true, bool autoCorrent = true)
        {
            var album = await _lastFm.Album.GetInfo(id, name, artist, autoCorrent);
            return album;
        }

        public static async Task<LastFmArtist> GetArtistInfo(string id, string name)
        {
            var artist = await _lastFm.Artist.GetInfo(id, name);

            return artist;
        }

        public static async Task<List<LastFmAlbum>> GetArtistAlbums(string id, string artist, int count = 0)
        {
            var albums = await _lastFm.Artist.GetTopAlbums(id, artist, count);

            return albums;
        }

        public static async Task<List<VkAudio>> GetArtistTopTracks(string id, string artist, int count = 0)
        {
            var tracks = await _lastFm.Artist.GetTopTracks(id, artist, count);
            if (tracks != null)
            {
                return (from track in tracks
                        select track.ToAudio()).ToList();
            }

            return null;
        }

        public static async Task<List<Audio>> GetNewsAudio(int count, int offset, CancellationToken token)
        {
            try
            {
                var vkNews = await _vkontakte.News.Get(null, "post", count, offset);
                if (vkNews.Items != null)
                {
                    var audioIds = new List<string>();

                    foreach (var vkNewsEntry in vkNews.Items)
                    {
                        var attachments = vkNewsEntry.Attachments;
                        if ((vkNewsEntry.Attachments == null || vkNewsEntry.Attachments.Count == 0) && (vkNewsEntry.CopyHistory != null && vkNewsEntry.CopyHistory.Count > 0))
                        {
                            attachments = vkNewsEntry.CopyHistory.Last().Attachments;
                        }

                        if (attachments != null)
                        {
                            var audioEntries = (from a in attachments
                                                where a is VkAudioAttachment
                                                select a.OwnerId + "_" + a.Id).ToList();
                            if (audioEntries.Any())
                                audioIds.AddRange(audioEntries);
                        }
                    }

                    if (audioIds.Count == 0)
                        return null;

                    var vkAudios = new List<VkLib.Core.Audio.VkAudio>();
                    if (audioIds.Count >= 100)
                    {
                        //если аудиозаписей больше 100, разбиваем на несколько запросов по 100 аудиозаписей
                        int i = 0, j = 99;
                        while (i + j < audioIds.Count)
                        {
                            if (token.IsCancellationRequested)
                                return null;

                            vkAudios.AddRange(await _vkontakte.Audio.GetById(audioIds.GetRange(i, j)));

                            i += 100;
                            if (j >= audioIds.Count)
                                j = audioIds.Count;
                        }
                    }
                    else
                    {
                        if (token.IsCancellationRequested)
                        {
                            Debug.WriteLine("News audio cancelled");
                            return null;
                        }

                        var a = await _vkontakte.Audio.GetById(audioIds);
                        if (a != null)
                            vkAudios.AddRange(a);
                    }

                    var audios = from a in vkAudios select a.ToAudio();

                    var result = new List<Audio>();

                    foreach (var vkNewsEntry in vkNews.Items)
                    {
                        var attachments = vkNewsEntry.Attachments;
                        if ((vkNewsEntry.Attachments == null || vkNewsEntry.Attachments.Count == 0) && (vkNewsEntry.CopyHistory != null && vkNewsEntry.CopyHistory.Count > 0))
                        {
                            attachments = vkNewsEntry.CopyHistory.Last().Attachments;
                        }

                        if (attachments == null)
                            continue;

                        var audioAttachments = attachments.Where(a => a is VkAudioAttachment).ToList();
                        if (!audioAttachments.Any())
                            continue;

                        var tracks = audioAttachments.Select(a => audios.FirstOrDefault(audio => audio.Id == a.Id.ToString())).Where(a => a != null).ToList();
                        result.AddRange(tracks);
                        //var post = new AudioPost();
                        //post.Id = vkNewsEntry.Id;
                        //post.Text = vkNewsEntry.Text;
                        //if (!string.IsNullOrEmpty(post.Text))
                        //{
                        //    var regex = new Regex(@"\[.*?\]", RegexOptions.Singleline);
                        //    var matches = regex.Matches(post.Text);

                        //    foreach (Match match in matches)
                        //    {
                        //        if (!match.Value.Contains("|"))
                        //            continue;

                        //        var title = match.Value.Substring(match.Value.IndexOf("|") + 1,
                        //                                          match.Value.Length - match.Value.IndexOf("|") - 2);
                        //        post.Text = post.Text.Replace(match.Value, title);
                        //    }
                        //}

                        //post.Audios = audioAttachments.Select(a => audios.FirstOrDefault(audio => audio.Id == a.Id)).ToList();
                        //post.Author = new VkProfile()
                        //{
                        //    FirstName = vkNewsEntry.Author.Name,
                        //};
                        //post.Date = vkNewsEntry.Date;
                        //result.Add(post);
                    }

                    return result;
                }
            }
            catch (VkAccessDeniedException ex)
            {
                LoggingService.Log(ex);
            }

            return null;
        }

        public static async Task<List<Audio>> GetWallAudio(int count, int offset, long userId = 0, CancellationToken token = default(CancellationToken))
        {
            try
            {
                var vkWallResult = await _vkontakte.Wall.Get(userId, "all", count, offset);
                if (token != CancellationToken.None && token.IsCancellationRequested)
                    return null;

                if (vkWallResult.TotalCount < offset)
                    return null;

                var vkWallPosts = vkWallResult.Items;

                if (vkWallPosts != null)
                {
                    var audioIds = new List<string>();

                    foreach (var vkwallPost in vkWallPosts)
                    {
                        var attachments = vkwallPost.Attachments;
                        if ((vkwallPost.Attachments == null || vkwallPost.Attachments.Count == 0) && (vkwallPost.CopyHistory != null && vkwallPost.CopyHistory.Count > 0))
                        {
                            attachments = vkwallPost.CopyHistory.Last().Attachments;
                        }

                        if (attachments != null)
                        {
                            var audioEntries = (from a in attachments
                                                where a is VkAudioAttachment
                                                select a.OwnerId + "_" + a.Id).ToList();
                            if (audioEntries.Any())
                                audioIds.AddRange(audioEntries);
                        }
                    }

                    var vkAudios = new List<VkLib.Core.Audio.VkAudio>();
                    if (audioIds.Count >= 100)
                    {
                        //если аудиозаписей больше 100, разбиваем на несколько запросов по 100 аудиозаписей
                        int i = 0, j = 99;
                        while (i + j < audioIds.Count)
                        {
                            if (token != CancellationToken.None && token.IsCancellationRequested)
                                return null;

                            vkAudios.AddRange(await _vkontakte.Audio.GetById(audioIds.GetRange(i, j)));

                            i += 100;
                            if (j >= audioIds.Count)
                                j = audioIds.Count;
                        }
                    }
                    else if (audioIds.Count > 0)
                    {
                        if (token != CancellationToken.None && token.IsCancellationRequested)
                        {
                            Debug.WriteLine("News audio cancelled");
                            return null;
                        }

                        var a = await _vkontakte.Audio.GetById(audioIds);
                        if (a != null)
                            vkAudios.AddRange(a);
                    }

                    var audios = from a in vkAudios select a.ToAudio();

                    var result = new List<Audio>();

                    foreach (var vkWallPost in vkWallPosts)
                    {
                        var attachments = vkWallPost.Attachments;
                        if ((vkWallPost.Attachments == null || vkWallPost.Attachments.Count == 0) && (vkWallPost.CopyHistory != null && vkWallPost.CopyHistory.Count > 0))
                        {
                            attachments = vkWallPost.CopyHistory.Last().Attachments;
                        }

                        if (attachments == null)
                            continue;

                        var audioAttachments = attachments.Where(a => a is VkAudioAttachment).ToList();
                        if (!audioAttachments.Any())
                            continue;

                        var tracks = audioAttachments.Select(a => audios.FirstOrDefault(audio => audio.Id == a.Id.ToString())).Where(a => a != null).ToList();
                        result.AddRange(tracks);
                        //var post = new AudioPost();
                        //post.Id = vkWallPost.Id.ToString();
                        //post.Text = vkWallPost.Text;
                        //if (!string.IsNullOrEmpty(post.Text))
                        //{
                        //    var regex = new Regex(@"\[.*?\]", RegexOptions.Singleline);
                        //    var matches = regex.Matches(post.Text);

                        //    foreach (Match match in matches)
                        //    {
                        //        if (!match.Value.Contains("|"))
                        //            continue;

                        //        var title = match.Value.Substring(match.Value.IndexOf("|") + 1,
                        //                                          match.Value.Length - match.Value.IndexOf("|") - 2);
                        //        post.Text = post.Text.Replace(match.Value, title);
                        //    }
                        //}

                        //post.Image = vkWallPost.Attachments.Select(a =>
                        //{
                        //    if (a is VkPhotoAttachment)
                        //        return new Uri(((VkPhotoAttachment)a).SourceBig);
                        //    return null;
                        //}).FirstOrDefault();
                        //post.Audios = audioAttachments.Select(a => audios.FirstOrDefault(audio => audio.Id == a.Id.ToString())).ToList();
                        //post.Author = new UserProfile()
                        //{
                        //    FirstName = vkWallPost.Author.Name,
                        //    PhotoUri = vkWallPost.Author.Photo.ToUri()
                        //};
                        //post.Date = vkWallPost.Date;
                        //result.Add(post);
                    }

                    return result;
                }
            }
            catch (VkAccessDeniedException ex)
            {
                LoggingService.Log(ex);
            }

            return null;
        }

        public static async Task<List<Audio>> GetFavoritesAudio(int count, int offset, long userId = 0, CancellationToken token = default(CancellationToken))
        {
            try
            {
                var vkWallResult = await _vkontakte.Favorites.GetPosts(count, offset);
                if (token != CancellationToken.None && token.IsCancellationRequested)
                    return null;

                if (vkWallResult.TotalCount < offset)
                    return null;

                var vkWallPosts = vkWallResult.Items;

                if (vkWallPosts != null)
                {
                    var audioIds = new List<string>();

                    foreach (var vkwallPost in vkWallPosts)
                    {
                        if (vkwallPost.Attachments != null)
                        {
                            var audioEntries = (from a in vkwallPost.Attachments
                                                where a is VkAudioAttachment
                                                select a.OwnerId + "_" + a.Id).ToList();
                            if (audioEntries.Any())
                                audioIds.AddRange(audioEntries);
                        }
                    }

                    var vkAudios = new List<VkLib.Core.Audio.VkAudio>();
                    if (audioIds.Count >= 100)
                    {
                        //если аудиозаписей больше 100, разбиваем на несколько запросов по 100 аудиозаписей
                        int i = 0, j = 99;
                        while (i + j < audioIds.Count)
                        {
                            if (token != CancellationToken.None && token.IsCancellationRequested)
                                return null;

                            vkAudios.AddRange(await _vkontakte.Audio.GetById(audioIds.GetRange(i, j)));

                            i += 100;
                            if (j >= audioIds.Count)
                                j = audioIds.Count;
                        }
                    }
                    else if (audioIds.Count > 0)
                    {
                        if (token != CancellationToken.None && token.IsCancellationRequested)
                        {
                            Debug.WriteLine("Favorites audio cancelled");
                            return null;
                        }

                        var a = await _vkontakte.Audio.GetById(audioIds);
                        if (a != null)
                            vkAudios.AddRange(a);
                    }

                    var audios = from a in vkAudios select a.ToAudio();

                    var result = new List<Audio>();

                    foreach (var vkWallPost in vkWallPosts)
                    {
                        if (vkWallPost.Attachments == null)
                            continue;

                        var audioAttachments = vkWallPost.Attachments.Where(a => a is VkAudioAttachment).ToList();
                        if (!audioAttachments.Any())
                            continue;

                        var tracks = audioAttachments.Select(a => audios.FirstOrDefault(audio => audio.Id == a.Id.ToString())).Where(a => a != null).ToList();
                        result.AddRange(tracks);
                        //var post = new AudioPost();
                        //post.Id = vkWallPost.Id.ToString();
                        //post.Text = vkWallPost.Text;
                        //if (!string.IsNullOrEmpty(post.Text))
                        //{
                        //    var regex = new Regex(@"\[.*?\]", RegexOptions.Singleline);
                        //    var matches = regex.Matches(post.Text);

                        //    foreach (Match match in matches)
                        //    {
                        //        if (!match.Value.Contains("|"))
                        //            continue;

                        //        var title = match.Value.Substring(match.Value.IndexOf("|") + 1,
                        //                                          match.Value.Length - match.Value.IndexOf("|") - 2);
                        //        post.Text = post.Text.Replace(match.Value, title);
                        //    }
                        //}

                        //post.Image = vkWallPost.Attachments.Select(a =>
                        //{
                        //    if (a is VkPhotoAttachment)
                        //        return new Uri(((VkPhotoAttachment)a).SourceBig);
                        //    return null;
                        //}).FirstOrDefault();
                        //post.Audios = audioAttachments.Select(a => audios.FirstOrDefault(audio => audio.Id == a.Id.ToString())).ToList();
                        //post.Author = new UserProfile()
                        //{
                        //    FirstName = vkWallPost.Author.Name,
                        //    PhotoUri = vkWallPost.Author.Photo.ToUri()
                        //};
                        //post.Date = vkWallPost.Date;
                        //result.Add(post);
                    }

                    return result;
                }
            }
            catch (VkAccessDeniedException ex)
            {
                LoggingService.Log(ex);
            }

            return null;
        }

        public static async Task<bool> SetMusicStatus(VkAudio audio, List<long> targetIds = null)
        {
            var result = await _vkontakte.Audio.SetBroadcast(long.Parse(audio.Id), audio.OwnerId, targetIds);

            return result != null;
        }

        public static async Task<bool> AddAudio(VkAudio audio, string captchaSid = null, string captchaKey = null)
        {
            if (string.IsNullOrEmpty(audio.Source))
            {
                var vkAudio = await GetAudioByArtistAndTitle(audio.Artist, audio.Title);
                if (vkAudio != null)
                {
                    audio.Id = vkAudio.Id;
                    audio.Artist = vkAudio.Artist;
                    audio.Title = vkAudio.Title;
                    audio.Source = vkAudio.Source;
                    audio.OwnerId = vkAudio.OwnerId;
                    audio.AlbumId = vkAudio.AlbumId;
                    audio.LyricsId = vkAudio.LyricsId;
                }
            }

            var newId = await _vkontakte.Audio.Add(long.Parse(audio.Id), audio.OwnerId, captchaSid: captchaSid, captchaKey: captchaKey);
            if (newId > 0)
            {
                audio.Id = newId.ToString();
                audio.OwnerId = _vkontakte.AccessToken.UserId;
                audio.IsAddedByCurrentUser = true;
                return true;
            }
            return false;
        }

        public static async Task<bool> RemoveAudio(VkAudio audio)
        {
            var result = await _vkontakte.Audio.Delete(long.Parse(audio.Id), audio.OwnerId);
            if (result)
            {
                audio.IsAddedByCurrentUser = false;
            }

            return result;
        }

        public static async Task<string> GetLyrics(string lyricsId)
        {
            var result = await _vkontakte.Audio.GetLyrics(long.Parse(lyricsId));
            return result;
        }

        public static async Task<string> EditAudio(string audioId, string ownerId, string title, string artist, string lyrics = null)
        {
            var lyricsId = await _vkontakte.Audio.Edit(long.Parse(ownerId), long.Parse(audioId), artist, title, lyrics);
            return lyricsId.ToString();
        }

        public static async Task<bool> UpdateNowPlaying(Audio audio)
        {
            await _lastFm.Track.UpdateNowPlaying(audio.Artist, audio.Title, null, (int)audio.Duration.TotalSeconds);
            return true;
        }

        public static async Task<bool> Scrobble(Audio audio)
        {
            var time = (int)DateTimeConverter.ToUnixTime(DateTime.Now);

            await _lastFm.Track.Scrobble(audio.Artist, audio.Title, time.ToString(), null, (int)audio.Duration.TotalSeconds);
            return true;
        }

        public static async Task<List<Audio>> GetAdvancedRecommendations(int count = 100, CancellationToken token = default(CancellationToken))
        {
            //берем первые 300 треков пользователя
            var allTracks = await GetUserTracks(300);
            var targetArtists = new List<string>();

            if (allTracks.Items != null)
            {
                //перемешиваем и вытаскиваем 5 исполнителей, о которых знает Echonest
                allTracks.Items.Shuffle();

                int checksCount = 0;

                foreach (var audio in allTracks.Items)
                {
                    if (token.IsCancellationRequested)
                        break;

                    if (targetArtists.Contains(audio.Artist))
                        continue;

                    if (checksCount > 19)
                        break;

                    checksCount++;

                    var echoArtists = await ViewModelLocator.Echonest.Artist.Search(audio.Artist);

                    if (token.IsCancellationRequested)
                        break;

                    if (echoArtists == null || echoArtists.Count == 0)
                        continue;

                    var artist = echoArtists.First().Name;
                    targetArtists.Add(artist);


                    if (targetArtists.Count == 5)
                        break;
                }

                if (token.IsCancellationRequested)
                    return null;

                var recommendedTracks = await ViewModelLocator.Echonest.Playlist.Basic(artists: targetArtists, count: 100);
                if (recommendedTracks != null)
                {
                    var results = (from track in recommendedTracks
                                   select new Audio()
                                   {
                                       Title = track.Title,
                                       Artist = track.ArtistName
                                   }).ToList();

                    return results;
                }
            }

            return null;
        }

        public static Task<List<AudioArtist>> GetArtistsFromTracks(IEnumerable<Audio> tracks, CancellationToken token)
        {
            return Task.Run(() =>
            {
                var artists = new Dictionary<string, AudioArtist>();

                foreach (var track in tracks)
                {
                    if (token.IsCancellationRequested)
                        break;

                    //parse artist

                    var artistName = track.Artist;
                    var artistId = Md5Helper.Md5(artistName.ToLowerInvariant()); //artistid = md5(artistName)
                    if (!artists.ContainsKey(artistId))
                    {
                        var artist = new AudioArtist();
                        artist.Id = artistId;
                        artist.Title = artistName;
                        artists.Add(artistId, artist);
                    }

                    artists[artistId].Tracks.Add(track);
                }

                return artists.Values.OrderBy(a => a.Title).ToList();
            });
        }

        public static async Task CopyAlbum(string title, long albumId, long ownerId)
        {
            var newAlbumId = await ViewModelLocator.Vkontakte.Audio.AddAlbum(title);

            var audio = await GetUserTracks(albumId: albumId, ownerId: ownerId);
            if (audio.Items != null && audio.Items.Count > 0)
            {

                NotificationService.NotifyProgressStarted(MainResources.NotificationSaving);
                int progressStep = (int)(100.0f / audio.Items.Count);


                bool captchaNeeded = false;
                string captchaImg = string.Empty;
                string captchaSid = string.Empty;
                string captchaKey = string.Empty;

                var audioIds = new List<long>();

                int count = 0;
                audio.Items.Reverse();
                for (int i = 0; i < audio.Items.Count; i++)
                {
                    var track = audio.Items[i];

                    if (count > 1)
                    {
                        count = 0;
                        await Task.Delay(1000); //не больше 2-х запросов в секунду
                    }

                    try
                    {
                        var newAudioId = await ViewModelLocator.Vkontakte.Audio.Add(long.Parse(track.Id), track.OwnerId, captchaSid: captchaSid, captchaKey: captchaKey);
                        audioIds.Add(newAudioId);

                        captchaNeeded = false;
                        captchaKey = null;
                        captchaSid = null;
                    }
                    catch (VkCaptchaNeededException ex)
                    {
                        captchaNeeded = true;
                        captchaImg = ex.CaptchaImg;
                        captchaSid = ex.CaptchaSid;
                    }

                    if (captchaNeeded)
                    {
                        var flyout = new FlyoutControl();
                        flyout.FlyoutContent = new CaptchaRequestView(captchaSid, captchaImg);
                        var result = await flyout.ShowAsync();
                        if (!string.IsNullOrEmpty((string)result))
                        {
                            captchaKey = (string)result;
                            i = i - 1;
                            continue;
                        }
                        else
                        {
                            NotificationService.NotifyProgressFinished();
                            return;
                        }
                    }

                    count++;

                    NotificationService.NotifyProgressChanged(progressStep);
                }

                await ViewModelLocator.Vkontakte.Audio.MoveToAlbum(newAlbumId, audioIds);

                NotificationService.NotifyProgressFinished(MainResources.NotificationSaved);
            }
        }
    }
}
