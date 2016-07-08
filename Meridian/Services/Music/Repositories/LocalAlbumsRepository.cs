using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using Meridian.Helpers;
using Meridian.Model;
using Meridian.ViewModel.Messages;

namespace Meridian.Services.Music.Repositories
{
    public class LocalAlbumsRepository
    {
        private bool _refreshed;

        public LocalAlbumsRepository()
        {

        }

        public async Task<List<AudioAlbum>> Get()
        {
            if (!_refreshed)
            {
                //check for updates on first time
                _refreshed = true;
                Refresh().ContinueWith(t =>
                {
                    AlbumsRepositoryUpdated(t.Result);
                });
            }

            return await ServiceLocator.DataBaseService.GetItems<AudioAlbum>();
        }

        public async Task<Tuple<List<AudioAlbum>, List<AudioAlbum>, List<AudioAlbum>>> Refresh()
        {
            var localAlbums = await GetFromLibrary(); //list of local albums from file system

            var cachedData = await ServiceLocator.DataBaseService.GetItems<AudioAlbum>(); //list of cached albums from database

            var deleted = new List<AudioAlbum>();
            var changed = new List<AudioAlbum>();
            var added = new List<AudioAlbum>();

            //check albums changes
            foreach (var cachedAlbum in cachedData)
            {
                var associatedAlbum = localAlbums.FirstOrDefault(t => t.Id == cachedAlbum.Id);
                if (associatedAlbum == null) //if album is cached but doesn't exists in file system it supposed to be deleted
                {
                    //TODO remove album mappings

                    deleted.Add(cachedAlbum);
                    continue;
                }

                //check properties changes
                if (UpdateAlbum(cachedAlbum, associatedAlbum))
                {
                    //properties was changed, add album to list of changed albums
                    changed.Add(cachedAlbum);
                }
            }

            //looking for new albums
            foreach (var localAlbum in localAlbums)
            {
                if (cachedData.Any(t => t.Id == localAlbum.Id))
                    continue;

                added.Add(localAlbum);
            }

            return new Tuple<List<AudioAlbum>, List<AudioAlbum>, List<AudioAlbum>>(deleted, changed, added);
        }

        private Task<List<AudioAlbum>> GetFromLibrary()
        {
            return Task.Run(async () =>
            {
                var musicFiles = FilesHelper.GetMusicFiles();

                double totalCount = musicFiles.Count;

                var albums = new Dictionary<string, AudioAlbum>();

                foreach (var filePath in musicFiles)
                {
                    TagLib.File audioFile = null;

                    try
                    {
                        audioFile = TagLib.File.Create(filePath);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Log(ex);
                        continue;
                    }

                    var track = new LocalAudio();

                    if (!string.IsNullOrWhiteSpace(audioFile.Tag.Album))
                    {
                        string artist = string.Empty;
                        if (!string.IsNullOrEmpty(audioFile.Tag.FirstPerformer))
                            artist = StringHelper.ToUtf8(audioFile.Tag.FirstPerformer);
                        else if (!string.IsNullOrEmpty(audioFile.Tag.FirstAlbumArtist))
                            artist = StringHelper.ToUtf8(audioFile.Tag.FirstAlbumArtist);

                        track.AlbumId = Md5Helper.Md5(artist.Trim().ToLower() + "_" + StringHelper.ToUtf8(audioFile.Tag.Album).Trim());
                        track.Album = StringHelper.ToUtf8(audioFile.Tag.Album).Trim();
                        if (!albums.ContainsKey(track.AlbumId))
                            albums.Add(track.AlbumId, new AudioAlbum()
                            {
                                Id = track.AlbumId,
                                Artist = artist.Trim(),
                                Title = StringHelper.ToUtf8(audioFile.Tag.Album).Trim(),
                                Year = (int)audioFile.Tag.Year,
                                ArtistId = Md5Helper.Md5(artist.Trim().ToLower())
                            });
                        else
                        {
                            if (string.IsNullOrEmpty(albums[track.AlbumId].CoverPath) && audioFile.Tag.Pictures != null && audioFile.Tag.Pictures.Length > 0)
                            {
                                albums[track.AlbumId].CoverPath = filePath;
                            }
                        }
                    }
                    audioFile.Dispose();

                    await Task.Delay(50);
                }

                LoggingService.Log("Music scan finished. Found " + albums.Count + " albums");

                return albums.Values.ToList();
            });
        }

        private bool UpdateAlbum(AudioAlbum cachedAlbum, AudioAlbum updatedAlbum)
        {
            bool changed = false;

            if (cachedAlbum.Title != updatedAlbum.Title)
            {
                cachedAlbum.Title = updatedAlbum.Title;
                changed = true;
            }

            if (cachedAlbum.Artist != updatedAlbum.Artist)
            {
                cachedAlbum.Artist = updatedAlbum.Artist;
                changed = true;
            }

            if (cachedAlbum.ArtistId != updatedAlbum.ArtistId)
            {
                cachedAlbum.ArtistId = updatedAlbum.ArtistId;
                changed = true;
            }

            if (cachedAlbum.CoverPath != updatedAlbum.CoverPath)
            {
                cachedAlbum.CoverPath = updatedAlbum.CoverPath;
                changed = true;
            }

            if (cachedAlbum.Year != updatedAlbum.Year)
            {
                cachedAlbum.Year = updatedAlbum.Year;
                changed = true;
            }

            return changed;
        }

        private async void AlbumsRepositoryUpdated(Tuple<List<AudioAlbum>, List<AudioAlbum>, List<AudioAlbum>> result)
        {
            var deleted = result.Item1;
            var changed = result.Item2;
            var added = result.Item3;

            await ServiceLocator.DataBaseService.DeleteItems(deleted);

            await ServiceLocator.DataBaseService.UpdateItems(changed);

            await ServiceLocator.DataBaseService.SaveItems(added);

            if (deleted.Count > 0 || changed.Count > 0 || added.Count > 0)
                Messenger.Default.Send(new LocalRepositoryUpdatedMessage() { RepositoryType = typeof(AudioAlbum) });

            LoggingService.Log(string.Format("Local albums database updated. Deleted: {0}, Changed: {1}, Added: {2}", deleted.Count, changed.Count, added.Count));
        }
    }
}
