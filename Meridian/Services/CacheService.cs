using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Neptune.Desktop.Storage;

namespace Meridian.Services
{
    public static class CacheService
    {
        private const string CachePath = "Cache";

        public static async Task<ImageSource> GetCachedImage(string path)
        {
            var filePath = Path.Combine(CachePath, path);

            if (await FileStorage.FileExists(filePath))
            {
                using (var stream = await FileStorage.OpenFile(filePath))
                {

                    var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = ms;
                    bi.EndInit();
                    return bi;
                }
            }

            return null;
        }

        public static async Task<ImageSource> CacheImage(string url, string path)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            var savePath = Path.Combine(CachePath, path);
            var parentDir = Path.GetDirectoryName(savePath);

            if (!await FileStorage.FolderExists(parentDir))
                await FileStorage.CreateFolder(parentDir);

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                using (var stream = await new HttpClient().GetStreamAsync(url))
                {
                    using (var ms = new MemoryStream())
                    {
                        await stream.CopyToAsync(ms);
                        ms.Seek(0, SeekOrigin.Begin);
                        await SaveStream(ms, savePath);
                    }
                }

                return await GetCachedImage(path);
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }


            return null;
        }

        public static string GetSafeFileName(string input)
        {
            var fileName = input;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '-');
            }

            return fileName;
        }

        private static async Task SaveStream(Stream stream, string fileName)
        {
            using (var fileStream = File.OpenWrite(fileName))
            {
                var buffer = new byte[1024];

                while (stream.Read(buffer, 0, buffer.Length) > 0)
                {
                    fileStream.Write(buffer, 0, buffer.Length);
                }

                fileStream.Flush();
            }
        }
    }
}
