using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Meridian.Helpers;
using Meridian.Model;
using Meridian.Services.Music.Repositories;

namespace Meridian.Services.Music
{
    public class LocalMusicService
    {
        private CancellationTokenSource _scanCancellationToken = new CancellationTokenSource();
        private readonly string _libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        private readonly LocalArtistsRepository _artistsRepository;
        private readonly LocalAlbumsRepository _albumsRepository;

        public LocalMusicService()
        {
            _artistsRepository = new LocalArtistsRepository(_libraryPath);
            _albumsRepository = new LocalAlbumsRepository(_libraryPath);
        }

        public async Task ScanMusic(IProgress<double> progress)
        {
            LoggingService.Log("Music scan started");

            try
            {
                int count = 0;

                var tracks = new List<LocalAudio>();

                await Task.Run(async () =>
                {
                    var musicFiles = Directory.EnumerateFiles(_libraryPath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
                                    || s.EndsWith(".wma", StringComparison.OrdinalIgnoreCase)).ToList();

                    double totalCount = musicFiles.Count;

                    var albums = new Dictionary<string, AudioAlbum>();
                    var artists = new Dictionary<string, AudioArtist>();

                    foreach (var filePath in musicFiles)
                    {
                        using (var audioFile = TagLib.File.Create(filePath))
                        {
                            var track = new LocalAudio();
                            track.Id = Md5Helper.Md5(filePath);
                            if (!string.IsNullOrEmpty(audioFile.Tag.Title))
                                track.Title = StringHelper.ToUtf8(audioFile.Tag.Title);
                            else
                                track.Title = Path.GetFileNameWithoutExtension(filePath);
                            track.Artist = StringHelper.ToUtf8(audioFile.Tag.FirstPerformer);
                            track.Duration = audioFile.Properties.Duration;
                            track.Source = filePath;

                            if (!string.IsNullOrWhiteSpace(audioFile.Tag.Album))
                            {
                                track.AlbumId = Md5Helper.Md5(audioFile.Tag.FirstAlbumArtist != null ? audioFile.Tag.FirstAlbumArtist.ToLower() + "_" + audioFile.Tag.Album : audioFile.Tag.Album);
                                track.Album = StringHelper.ToUtf8(audioFile.Tag.Album);
                                if (!albums.ContainsKey(track.AlbumId))
                                    albums.Add(track.AlbumId, new AudioAlbum() { Id = track.AlbumId, Artist = StringHelper.ToUtf8(audioFile.Tag.FirstAlbumArtist), Title = StringHelper.ToUtf8(audioFile.Tag.Album), Year = (int)audioFile.Tag.Year});
                                else
                                {
                                    if (string.IsNullOrEmpty(albums[track.AlbumId].CoverPath) && audioFile.Tag.Pictures != null && audioFile.Tag.Pictures.Length > 0)
                                    {
                                        albums[track.AlbumId].CoverPath = filePath;
                                    }
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(audioFile.Tag.FirstPerformer))
                            {
                                track.ArtistId = Md5Helper.Md5(audioFile.Tag.FirstPerformer.ToLower());
                                track.Artist = StringHelper.ToUtf8(audioFile.Tag.FirstPerformer);
                                if (!artists.ContainsKey(track.ArtistId))
                                    artists.Add(track.ArtistId, new AudioArtist() { Id = track.ArtistId, Title = StringHelper.ToUtf8(audioFile.Tag.FirstPerformer) });
                            }
             
                            tracks.Add(track);

                            count++;

                            progress.Report(count / totalCount * 100);
                        }
                    }

                    await ServiceLocator.DataBaseService.SaveItems(tracks);
                    await ServiceLocator.DataBaseService.SaveItems(albums.Values);
                    await ServiceLocator.DataBaseService.SaveItems(artists.Values);

                    LoggingService.Log("Music scan finished. Found " + count + " tracks; " + albums.Count + " albums; " + artists.Count + " artists");

                    tracks.Clear();
                    albums.Clear();
                    artists.Clear();

                }, _scanCancellationToken.Token);
            }
            catch (Exception ex)
            {
                LoggingService.Log("Music scan error. " + ex);
            }
        }

        public void ScanMusicCancel()
        {
            _scanCancellationToken.Cancel();

            _scanCancellationToken = new CancellationTokenSource();
        }

        public async Task<List<LocalAudio>> GetTracks()
        {
            return await ServiceLocator.DataBaseService.GetItems<LocalAudio>();
        }

        public async Task<List<LocalAudio>> GetAlbumTracks(string albumId)
        {
            return await ServiceLocator.DataBaseService.GetLocalAlbumTracks(albumId);
        }

        public async Task<List<AudioAlbum>> GetAlbums()
        {
            return await _albumsRepository.Get();
        }

        public async Task<List<AudioArtist>> GetArtists()
        {
            return await _artistsRepository.Get();
        }

        public async Task<List<LocalAudio>> SearchTracks(string query)
        {
            //not good, but sqlite doesn't support case insensitive queries for unicode
            var tracks = await ServiceLocator.DataBaseService.GetItems<LocalAudio>();

            return await Task.Run(() =>
            {
                var result = tracks.Where(a => a.Title != null && a.Title.StartsWith(query, StringComparison.OrdinalIgnoreCase)).ToList();
                result.AddRange(tracks.Where(a => a.Artist != null && a.Artist.StartsWith(query, StringComparison.OrdinalIgnoreCase)).ToList());
                result.AddRange(tracks.Where(a => a.Album != null && a.Album.StartsWith(query, StringComparison.OrdinalIgnoreCase)).ToList());

                tracks.Clear();
                return result;
            });
        }
    }
}
