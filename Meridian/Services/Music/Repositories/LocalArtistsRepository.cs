using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Meridian.Helpers;
using Meridian.Model;

namespace Meridian.Services.Music.Repositories
{
    public class LocalArtistsRepository
    {
        private readonly string _libraryPath;
        private bool _refreshed;

        public LocalArtistsRepository(string libraryPath)
        {
            _libraryPath = libraryPath;
        }

        public async Task<List<AudioArtist>> Get()
        {
            if (!_refreshed)
            {
                //check for updates on first time
                _refreshed = true;
                var changes = await Refresh();
                ArtistsRepositoryUpdated(changes);
            }

            return await ServiceLocator.DataBaseService.GetItems<AudioArtist>();
        }

        public async Task<Tuple<List<AudioArtist>, List<AudioArtist>, List<AudioArtist>>> Refresh()
        {
            var localArtists = await GetFromLibrary(); //list of local artists from file system

            var cachedData = await ServiceLocator.DataBaseService.GetItems<AudioArtist>(); //list of cached artists from database

            var deleted = new List<AudioArtist>();
            var changed = new List<AudioArtist>();
            var added = new List<AudioArtist>();

            //check artists changes
            foreach (var cachedArtist in cachedData)
            {
                var associatedArtist = localArtists.FirstOrDefault(t => t.Id == cachedArtist.Id);
                if (associatedArtist == null) //if artist is cached but doesn't exists in file system it supposed to be deleted
                {
                    deleted.Add(cachedArtist);
                    continue;
                }

                //check properties changes
                if (UpdateArtist(cachedArtist, associatedArtist))
                {
                    //properties was changed, add artist to list of changed artists
                    changed.Add(cachedArtist);
                }
            }

            //looking for new artists
            foreach (var localArtist in localArtists)
            {
                if (cachedData.Any(t => t.Id == localArtist.Id))
                    continue;

                added.Add(localArtist);
            }

            return new Tuple<List<AudioArtist>, List<AudioArtist>, List<AudioArtist>>(deleted, changed, added);
        }

        private Task<List<AudioArtist>> GetFromLibrary()
        {
            return Task.Run(async () =>
            {
                var musicFiles = Directory.EnumerateFiles(_libraryPath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
                                || s.EndsWith(".wma", StringComparison.OrdinalIgnoreCase)).ToList();

                double totalCount = musicFiles.Count;

                var artists = new Dictionary<string, AudioArtist>();

                foreach (var filePath in musicFiles)
                {
                    using (var audioFile = TagLib.File.Create(filePath))
                    {
                        var track = new LocalAudio();

                        if (!string.IsNullOrWhiteSpace(audioFile.Tag.FirstPerformer))
                        {
                            track.ArtistId = Md5Helper.Md5(audioFile.Tag.FirstPerformer.ToLower());
                            track.Artist = StringHelper.ToUtf8(audioFile.Tag.FirstPerformer);
                            if (!artists.ContainsKey(track.ArtistId))
                                artists.Add(track.ArtistId, new AudioArtist() { Id = track.ArtistId, Title = StringHelper.ToUtf8(audioFile.Tag.FirstPerformer) });
                        }
                    }
                }

                await ServiceLocator.DataBaseService.SaveItems(artists.Values);

                LoggingService.Log("Music scan finished. Found " + artists.Count + " artists");

                return artists.Values.ToList();
            });
        }

        private bool UpdateArtist(AudioArtist cachedArtist, AudioArtist updatedArtist)
        {
            bool changed = false;

            if (!cachedArtist.Title.Equals(updatedArtist.Title, StringComparison.OrdinalIgnoreCase))
            {
                cachedArtist.Title = updatedArtist.Title;
                changed = true;
            }

            return changed;
        }

        private async void ArtistsRepositoryUpdated(Tuple<List<AudioArtist>, List<AudioArtist>, List<AudioArtist>> result)
        {
            var deleted = result.Item1;
            var changed = result.Item2;
            var added = result.Item3;

            await ServiceLocator.DataBaseService.DeleteItems(deleted);

            await ServiceLocator.DataBaseService.UpdateItems(changed);

            await ServiceLocator.DataBaseService.SaveItems(added);

            LoggingService.Log(string.Format("Local artists database updated. Deleted: {0}, Changed: {1}, Added: {2}", deleted.Count, changed.Count, added.Count));
        }
    }
}
