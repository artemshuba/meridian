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
    public class LocalArtistsRepository
    {
        private bool _refreshed;

        public LocalArtistsRepository()
        {

        }

        public async Task<List<AudioArtist>> Get()
        {
            if (!_refreshed)
            {
                //check for updates on first time
                _refreshed = true;
                Refresh().ContinueWith(t =>
                {
                    ArtistsRepositoryUpdated(t.Result);
                });
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
                var musicFiles = FilesHelper.GetMusicFiles();

                double totalCount = musicFiles.Count;

                var artists = new Dictionary<string, AudioArtist>();

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
                    string artist = string.Empty;
                    if (!string.IsNullOrWhiteSpace(audioFile.Tag.FirstPerformer))
                        artist = audioFile.Tag.FirstPerformer;
                    else if (!string.IsNullOrWhiteSpace(audioFile.Tag.FirstAlbumArtist))
                        artist = audioFile.Tag.FirstAlbumArtist;

                    if (!string.IsNullOrWhiteSpace(artist))
                    {
                        track.ArtistId = Md5Helper.Md5(StringHelper.ToUtf8(artist).Trim().ToLower());
                        track.Artist = StringHelper.ToUtf8(artist).Trim();
                        if (!artists.ContainsKey(track.ArtistId))
                            artists.Add(track.ArtistId, new AudioArtist() { Id = track.ArtistId, Title = track.Artist });
                    }

                    audioFile.Dispose();

                    await Task.Delay(50);
                }

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

            if (deleted.Count > 0 || changed.Count > 0 || added.Count > 0)
                Messenger.Default.Send(new LocalRepositoryUpdatedMessage() { RepositoryType = typeof(AudioArtist) });

            LoggingService.Log(string.Format("Local artists database updated. Deleted: {0}, Changed: {1}, Added: {2}", deleted.Count, changed.Count, added.Count));
        }
    }
}
