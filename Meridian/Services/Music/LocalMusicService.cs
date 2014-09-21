using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Meridian.Helpers;
using Meridian.Model;
using Meridian.Extensions;
using Xbox.Music;

namespace Meridian.Services.Music
{
    public class LocalMusicService
    {
        private CancellationTokenSource _scanCancellationToken = new CancellationTokenSource();

        public async Task ScanMusic(IProgress<double> progress)
        {
            var libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

            LoggingService.Log("Music scan started");

            try
            {
                int count = 0;

                var tracks = new List<LocalAudio>();

                await Task.Run(async () =>
                {
                    var musicFiles = Directory.EnumerateFiles(libraryPath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
                                    || s.EndsWith(".wma", StringComparison.OrdinalIgnoreCase)).ToList();

                    double totalCount = musicFiles.Count;

                    var albums = new Dictionary<string, AudioAlbum>();

                    foreach (var filePath in musicFiles)
                    {
                        using (var audioFile = TagLib.File.Create(filePath))
                        {
                            var track = new LocalAudio();
                            track.Id = Md5Helper.Md5(filePath);
                            track.Title = ToUtf8(audioFile.Tag.Title);
                            track.Artist = ToUtf8(audioFile.Tag.FirstPerformer);
                            track.Duration = audioFile.Properties.Duration;
                            track.Source = filePath;

                            if (!string.IsNullOrWhiteSpace(audioFile.Tag.Album))
                            {
                                track.AlbumId = Md5Helper.Md5(audioFile.Tag.FirstAlbumArtist + "_" + audioFile.Tag.Album);
                                track.Album = ToUtf8(audioFile.Tag.Album);
                                if (!albums.ContainsKey(track.AlbumId))
                                    albums.Add(track.AlbumId, new AudioAlbum() { Id = track.AlbumId, Artist = ToUtf8(audioFile.Tag.FirstAlbumArtist), Title = ToUtf8(audioFile.Tag.Album) });
                                else
                                {
                                    if (string.IsNullOrEmpty(albums[track.AlbumId].CoverPath) && audioFile.Tag.Pictures != null && audioFile.Tag.Pictures.Length > 0)
                                    {
                                        albums[track.AlbumId].CoverPath = filePath;
                                    }
                                }
                            }

                            tracks.Add(track);

                            count++;

                            progress.Report(count / totalCount * 100);
                        }
                    }

                    await ServiceLocator.DataBaseService.SaveItems(tracks);
                    await ServiceLocator.DataBaseService.SaveItems(albums.Values);

                    LoggingService.Log("Music scan finished. Found " + count + " tracks; " + albums.Count + " albums");
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

        public async Task<List<AudioAlbum>> GetAlbums()
        {
            return await ServiceLocator.DataBaseService.GetItems<AudioAlbum>();
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
                return result;
            });
        }

        private static string ToUtf8(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return new string(input.ToCharArray().
                Select(x => ((x + 848) >= 'А' && (x + 848) <= 'ё') ? (char)(x + 848) : x).
                ToArray());
        }

    }
}
