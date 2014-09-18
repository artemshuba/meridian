using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Meridian.Helpers;
using Meridian.Model;

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

                    foreach (var filePath in musicFiles)
                    {
                        using (var audioFile = TagLib.File.Create(filePath))
                        {
                            var track = new LocalAudio();
                            track.Id = Md5Helper.Md5(filePath);
                            track.Title = ToUtf8(audioFile.Tag.Title);
                            track.Artist = ToUtf8(audioFile.Tag.FirstPerformer);
                            track.Duration = audioFile.Properties.Duration;
                            track.Url = filePath;

                            tracks.Add(track);

                            count++;

                            progress.Report(count / totalCount * 100);
                        }
                    }

                    await ServiceLocator.DataBaseService.SaveItems(tracks);
                }, _scanCancellationToken.Token);

                LoggingService.Log("Music scan finished. Found " + count + " tracks");
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

        public async Task<List<LocalAudio>> GetLocalTracks()
        {
            return await ServiceLocator.DataBaseService.GetItems<LocalAudio>();
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
