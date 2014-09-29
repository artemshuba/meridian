using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Threading;
using Meridian.Model;
using Meridian.Services;

namespace Meridian.Helpers
{
    public static class ArtistImageHelper
    {
        private const int MAX_PARALLEL_ITEMS = 3;

        private static readonly Dictionary<string, List<AudioArtist>> _requestQueue = new Dictionary<string, List<AudioArtist>>();
        private static readonly object _syncRoot = new object();
        private static bool _isQueueWorking;

        /// <summary>
        /// Ставит трек в очередь на получение обложки. Когда очередь подойдет, треку будет присвоена обложка.
        /// </summary>
        /// <param name="target"></param>
        public static void RequestCover(AudioArtist target)
        {
            //ставим трек в очередь и сразу возвращаем null
            EnqueueCoverRequest(target);
        }

        /// <summary>
        /// Получить обложку
        /// </summary>
        /// <param name="target"></param>
        public static Task<ImageSource> GetCover(AudioArtist target)
        {
            return GetImage(target.Title);
        }

        private static void EnqueueCoverRequest(AudioArtist target)
        {
            lock (_syncRoot)
            {
                if (_requestQueue.ContainsKey(target.Id))
                {
                    if (_requestQueue[target.Id].All(t => t != target))
                        _requestQueue[target.Id].Add(target);
                }
                else
                {
                    _requestQueue.Add(target.Id, new List<AudioArtist>() { target });
                }
            }

            if (!_isQueueWorking)
                Task.Run(() => StartQueueProcessing()); //вся обработка очереди производится в 1 потоке
        }

        private static async void StartQueueProcessing()
        {
            _isQueueWorking = true;

            while (_requestQueue.Count > 0)
            {
                List<KeyValuePair<string, List<AudioArtist>>> queueItems;

                lock (_syncRoot)
                {
                    queueItems = _requestQueue.Take(MAX_PARALLEL_ITEMS).ToList(); //берем несколько элементов очереди
                }

                var runningTasks = new List<Task<KeyValuePair<string, ImageSource>>>();

                foreach (var queuItem in queueItems)
                {
                    var artist = queuItem.Value.FirstOrDefault();
                    if (artist == null)
                        continue;
                    runningTasks.Add(ProcessQueueItem(queuItem.Key, artist.Title));
                }

                var covers = await Task.WhenAll(runningTasks);

                runningTasks.Clear();


                foreach (var keyValuePair in covers)
                {
                    if (queueItems.All(i => i.Key != keyValuePair.Key))
                        continue;

                    var q = queueItems.First(i => i.Key == keyValuePair.Key); //находим соответствующий элемент
                    for (var i = 0; i < q.Value.Count; i++) //передаем в каждый callback полученную обложку
                    {
                        //await Task.Delay(20);

                        var target = q.Value[i];
                        if (target != null)
                        {
                            KeyValuePair<string, ImageSource> pair = keyValuePair;
                            DispatcherHelper.RunAsync(() =>
                            {
                                target.Image = pair.Value;
                            });
                        }
                    }

                    lock (_syncRoot)
                    {
                        _requestQueue.Remove(q.Key); //удаляем их из очереди
                    }
                }

                await Task.Delay(100);
            }

            _isQueueWorking = false;
        }

        private static async Task<KeyValuePair<string, ImageSource>> ProcessQueueItem(string albumId, string artist)
        {
            ImageSource source = await GetImage(artist);

            return new KeyValuePair<string, ImageSource>(albumId, source);
        }

        private static async Task<ImageSource> GetImage(string artist)
        {
            try
            {
                ImageSource cachedImage = await GetImageAsync(artist);
                return cachedImage;
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

            return null;
        }

        private static Task<ImageSource> GetImageAsync(string artist)
        {
            var tcs = new TaskCompletionSource<ImageSource>();

            DispatcherHelper.RunAsync(async () =>
            {
                var cachedImage = await CacheService.GetCachedImage("artists/" + CacheService.GetSafeFileName(artist + "_big.jpg"));
                if (cachedImage != null)
                {
                    tcs.SetResult(cachedImage);
                    return;
                }

                var imageUri = await DataService.GetArtistImage(artist, true);
                if (imageUri != null)
                {
                    cachedImage = await CacheService.CacheImage(imageUri.OriginalString,
                                "artists/" + CacheService.GetSafeFileName(artist + "_big.jpg"));

                    tcs.SetResult(cachedImage);
                }

                tcs.SetResult(null);
            });

            return tcs.Task;
        }
    }
}
