using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Threading;
using Meridian.Model;

namespace Meridian.Helpers
{
    public static class AlbumCoversHelper
    {
        private const int MAX_PARALLEL_ITEMS = 3;

        private static readonly Dictionary<string, List<AudioAlbum>> _requestQueue = new Dictionary<string, List<AudioAlbum>>();
        private static readonly object _syncRoot = new object();
        private static bool _isQueueWorking;

        /// <summary>
        /// Ставит трек в очередь на получение обложки. Когда очередь подойдет, треку будет присвоена обложка.
        /// </summary>
        /// <param name="target"></param>
        public static void RequestCover(AudioAlbum target)
        {
            //ставим трек в очередь и сразу возвращаем null
            EnqueueCoverRequest(target);
        }

        /// <summary>
        /// Получить обложку
        /// </summary>
        /// <param name="target"></param>
        public static Task<ImageSource> GetCover(AudioAlbum target)
        {
            return GetImage(target.CoverPath);
        }

        private static void EnqueueCoverRequest(AudioAlbum target)
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
                    _requestQueue.Add(target.Id, new List<AudioAlbum>() { target });
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
                List<KeyValuePair<string, List<AudioAlbum>>> queueItems;

                lock (_syncRoot)
                {
                    queueItems = _requestQueue.Take(MAX_PARALLEL_ITEMS).ToList(); //берем несколько элементов очереди
                }

                var runningTasks = new List<Task<KeyValuePair<string, ImageSource>>>();

                foreach (var queuItem in queueItems)
                {
                    var album = queuItem.Value.FirstOrDefault();
                    if (album == null)
                        continue;
                    runningTasks.Add(ProcessQueueItem(queuItem.Key, album.CoverPath));
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

                                target.Cover = pair.Value;
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

        private static async Task<KeyValuePair<string, ImageSource>> ProcessQueueItem(string albumId, string coverPath)
        {
            ImageSource source = null;
            try
            {
                source = await GetImage(coverPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return new KeyValuePair<string, ImageSource>(albumId, source);
        }

        private static async Task<ImageSource> GetImage(string coverPath)
        {
            using (var audioFile = TagLib.File.Create(coverPath))
            {
                var image = audioFile.Tag.Pictures.FirstOrDefault();
                if (image != null)
                {
                    var ms = new MemoryStream();
                    await ms.WriteAsync(image.Data.Data, 0, image.Data.Data.Length);
                    ms.Seek(0, SeekOrigin.Begin);

                    BitmapImage bi = null;

                    //await DispatcherHelper.RunAsync(() =>
                    //{
                        bi = new BitmapImage();
                    //});

                    try
                    {
                        bi.BeginInit();
                        bi.StreamSource = ms;
                        bi.EndInit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        return null;
                    }
                    bi.Freeze();
                    return bi;
                }
            }

            return null;
        }
    }
}
