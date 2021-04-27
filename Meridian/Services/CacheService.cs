using Jupiter.Utils.Helpers;
using Meridian.Model;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Meridian.Services
{
    public class CacheService
    {
        public const string CachePath = "Cache";
        public const string ImageCachePath = CachePath + "\\Image";

        public async Task<CachedImage> CacheImageFromUri(Uri sourceUri, string key, int optimalImageWidth = 0)
        {
            bool cached = false;

            try
            {
                using (var stream = await new HttpClient().GetStreamAsync(sourceUri))
                {
                    using (var ms = new MemoryStream())
                    {
                        await stream.CopyToAsync(ms);
                        await ms.FlushAsync();

                        ms.Seek(0, SeekOrigin.Begin);

                        await SaveImageStream(key, ms);

                        cached = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to cache image");
            }

            if (cached)
                return await GetCachedImage(key, optimalImageWidth);
            else
                return null;
        }

        public async Task<CachedImage> GetCachedImage(string key, int optimalImageWidth = 0)
        {
            var stream = await GetCachedImageStream(key);
            if (stream == null)
                return null;

            try
            {
                var bi = new BitmapImage();
                if (optimalImageWidth != 0)
                {
                    bi.DecodePixelType = DecodePixelType.Logical;
                    bi.DecodePixelWidth = optimalImageWidth;
                }
                await bi.SetSourceAsync(stream.AsRandomAccessStream());
                return new CachedImage { Key = key, Source = bi };
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to get cached image");
            }

            return null;
        }

        public async Task<Stream> GetCachedImageStream(string key)
        {
            if (!IsFileCached(key, ImageCachePath))
                return null;

            try
            {
                var fileName = GetFileNameForKey(key);
                var filePath = Path.Combine(ImageCachePath, fileName);

                if (!FileStorageHelper.IsFileExists(filePath))
                    return null;

                var ms = new MemoryStream();

                using (var fileStream = await FileStorageHelper.OpenFileRead(filePath))
                {
                    await fileStream.CopyToAsync(ms);
                }

                await ms.FlushAsync();

                ms.Seek(0, SeekOrigin.Begin);

                return ms;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to get cached image stream");
            }

            return null;
        }

        public Uri GetCachedImageUri(string key, string path = null)
        {
            var fileName = GetFileNameForKey(key);
            return new Uri($"ms-appdata:///Local/{ImageCachePath}/{fileName}");
        }

        public Task SaveImageStream(string key, Stream stream)
        {
            return SaveStream(key, stream, ImageCachePath);
        }

        public async Task SaveStream(string key, Stream stream, string path = null)
        {
            string fileName = GetFileNameForKey(key);
            string filePath = Path.Combine(path ?? string.Empty, fileName);

            using (var fileStream = await FileStorageHelper.OpenFileWrite(filePath))
            {
                await stream.CopyToAsync(fileStream);

                await fileStream.FlushAsync();
            }
        }

        public bool IsImageCached(string key)
        {
            return IsFileCached(key, ImageCachePath);
        }

        public bool IsFileCached(string key, string path = null)
        {
            string fileName = GetFileNameForKey(key);
            string filePath = Path.Combine(path ?? string.Empty, fileName);

            return FileStorageHelper.IsFileExists(filePath);
        }

        private string GetFileNameForKey(string key)
        {
            //generate md5 for key
            var alg = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var buff = CryptographicBuffer.ConvertStringToBinary(key, BinaryStringEncoding.Utf8);
            var hashed = alg.HashData(buff);
            return CryptographicBuffer.EncodeToHexString(hashed) + ".jpg";
        }
    }
}