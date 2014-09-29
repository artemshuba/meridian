using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Meridian.Helpers;
using Meridian.Model;

namespace Meridian.Services.Music.Repositories
{
    public class LocalAlbumsRepository
    {
        private readonly string _libraryPath;
        private bool _refreshed;

        public LocalAlbumsRepository(string libraryPath)
        {
            _libraryPath = libraryPath;
        }

        public async Task<List<AudioAlbum>> Get()
        {
            if (!_refreshed)
            {
                //check for updates on first time
                _refreshed = true;
                var changes = await Refresh();
                AlbumsRepositoryUpdated(changes);
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
                var musicFiles = Directory.EnumerateFiles(_libraryPath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
                                || s.EndsWith(".wma", StringComparison.OrdinalIgnoreCase)).ToList();

                double totalCount = musicFiles.Count;

                var albums = new Dictionary<string, AudioAlbum>();

                foreach (var filePath in musicFiles)
                {
                    using (var audioFile = TagLib.File.Create(filePath))
                    {
                        var track = new LocalAudio();

                        if (!string.IsNullOrWhiteSpace(audioFile.Tag.Album))
                        {
                            track.AlbumId = Md5Helper.Md5(audioFile.Tag.FirstAlbumArtist != null ? StringHelper.ToUtf8(audioFile.Tag.FirstAlbumArtist).Trim().ToLower() + "_" + StringHelper.ToUtf8(audioFile.Tag.Album).Trim() : StringHelper.ToUtf8(audioFile.Tag.Album).Trim());
                            track.Album = StringHelper.ToUtf8(audioFile.Tag.Album).Trim();
                            if (!albums.ContainsKey(track.AlbumId))
                                albums.Add(track.AlbumId, new AudioAlbum() { Id = track.AlbumId, Artist = audioFile.Tag.FirstAlbumArtist != null ? StringHelper.ToUtf8(audioFile.Tag.FirstAlbumArtist).Trim() : null, Title = StringHelper.ToUtf8(audioFile.Tag.Album).Trim(), Year = (int)audioFile.Tag.Year});
                            else
                            {
                                if (string.IsNullOrEmpty(albums[track.AlbumId].CoverPath) && audioFile.Tag.Pictures != null && audioFile.Tag.Pictures.Length > 0)
                                {
                                    albums[track.AlbumId].CoverPath = filePath;
                                }
                            }
                        }
                    }
                }

                await ServiceLocator.DataBaseService.SaveItems(albums.Values);

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

            LoggingService.Log(string.Format("Local albums database updated. Deleted: {0}, Changed: {1}, Added: {2}", deleted.Count, changed.Count, added.Count));
        }
    }
}
