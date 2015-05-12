using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using Meridian.Helpers;
using Meridian.Model;
using Meridian.ViewModel.Messages;

namespace Meridian.Services.Music.Repositories
{
    public class LocalTracksRepository
    {
        private bool _refreshed;

        public async Task<List<LocalAudio>> GetTracks()
        {
            if (!_refreshed)
            {
                //check for updates on first time
                _refreshed = true;
                Refresh().ContinueWith(t =>
                {
                    TracksRepositoryUpdated(t.Result);
                });
            }

            return await ServiceLocator.DataBaseService.GetItems<LocalAudio>();
        }

        public async Task<Tuple<List<LocalAudio>, List<LocalAudio>, List<LocalAudio>>> Refresh()
        {
            var localTracks = await GetFromLibrary(); //list of local tracks from file system

            var cachedData = await ServiceLocator.DataBaseService.GetItems<LocalAudio>(); //list of cached tracks from database

            var deleted = new List<LocalAudio>();
            var changed = new List<LocalAudio>();
            var added = new List<LocalAudio>();

            //check tracks changes
            foreach (var cachedTrack in cachedData)
            {
                var associatedTrack = localTracks.FirstOrDefault(t => t.Id == cachedTrack.Id);
                if (associatedTrack == null) //if track is cached but doesn't exists in file system it supposed to be deleted
                {
                    //TODO remove track mappings

                    deleted.Add(cachedTrack);
                    continue;
                }

                //check properties changes
                if (UpdateTrack(cachedTrack, associatedTrack))
                {
                    //properties was changed, add track to list of changed tracks
                    changed.Add(cachedTrack);
                }
            }

            //looking for new tracks
            foreach (var localTrack in localTracks)
            {
                if (cachedData.Any(t => t.Id == localTrack.Id))
                    continue;

                added.Add(localTrack);
            }

            return new Tuple<List<LocalAudio>, List<LocalAudio>, List<LocalAudio>>(deleted, changed, added);
        }

        private Task<List<LocalAudio>> GetFromLibrary()
        {
            return Task.Run(async () =>
            {
                var musicFiles = FilesHelper.GetMusicFiles();

                double totalCount = musicFiles.Count;

                var tracks = new List<LocalAudio>();

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
                    track.Id = Md5Helper.Md5(filePath);
                    if (!string.IsNullOrEmpty(audioFile.Tag.Title))
                        track.Title = StringHelper.ToUtf8(audioFile.Tag.Title);
                    else
                        track.Title = Path.GetFileNameWithoutExtension(filePath);

                    var artist = audioFile.Tag.FirstPerformer;
                    if (string.IsNullOrEmpty(artist))
                        artist = audioFile.Tag.FirstAlbumArtist;

                    track.Artist = StringHelper.ToUtf8(artist);

                    if (!string.IsNullOrEmpty(track.Artist))
                    {
                        track.Artist = track.Artist.Trim();
                        track.ArtistId = Md5Helper.Md5(track.Artist.Trim().ToLower());
                    }
                    else
                        track.Artist = string.Empty;

                    track.Duration = audioFile.Properties.Duration;
                    track.Source = filePath;

                    if (!string.IsNullOrWhiteSpace(audioFile.Tag.Album))
                    {
                        track.AlbumId = Md5Helper.Md5(track.Artist.Trim().ToLower() + "_" + StringHelper.ToUtf8(audioFile.Tag.Album).Trim());
                        track.Album = StringHelper.ToUtf8(audioFile.Tag.Album).Trim();
                    }

                    tracks.Add(track);
                    audioFile.Dispose();

                    await Task.Delay(50);
                }

                LoggingService.Log("Music scan finished. Found " + tracks.Count + " tracks");

                return tracks;
            });
        }

        private bool UpdateTrack(LocalAudio cachedTrack, LocalAudio updatedTrack)
        {
            bool changed = false;

            if (cachedTrack.Title != updatedTrack.Title)
            {
                cachedTrack.Title = updatedTrack.Title;
                changed = true;
            }

            if (cachedTrack.Artist != updatedTrack.Artist)
            {
                cachedTrack.Artist = updatedTrack.Artist;
                changed = true;
            }

            if (cachedTrack.Album != updatedTrack.Album)
            {
                cachedTrack.Album = updatedTrack.Album;
                changed = true;
            }

            if (cachedTrack.AlbumId != updatedTrack.AlbumId)
            {
                cachedTrack.AlbumId = updatedTrack.AlbumId;
                changed = true;
            }

            if (cachedTrack.ArtistId != updatedTrack.ArtistId)
            {
                cachedTrack.ArtistId = updatedTrack.ArtistId;
                changed = true;
            }

            if (cachedTrack.Duration != updatedTrack.Duration)
            {
                cachedTrack.Duration = updatedTrack.Duration;
                changed = true;
            }

            return changed;
        }

        private async void TracksRepositoryUpdated(Tuple<List<LocalAudio>, List<LocalAudio>, List<LocalAudio>> result)
        {
            var deleted = result.Item1;
            var changed = result.Item2;
            var added = result.Item3;

            await ServiceLocator.DataBaseService.DeleteItems(deleted);

            await ServiceLocator.DataBaseService.UpdateItems(changed);

            await ServiceLocator.DataBaseService.SaveItems(added);

            if (deleted.Count > 0 || changed.Count > 0 || added.Count > 0)
                Messenger.Default.Send(new LocalRepositoryUpdatedMessage() { RepositoryType = typeof(LocalAudio) });

            LoggingService.Log(string.Format("Local tracks database updated. Deleted: {0}, Changed: {1}, Added: {2}", deleted.Count, changed.Count, added.Count));
        }
    }
}
