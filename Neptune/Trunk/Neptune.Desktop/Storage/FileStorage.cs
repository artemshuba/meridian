using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Neptune.Extensions;

namespace Neptune.Desktop.Storage
{
    public static class FileStorage
    {
        private static readonly Dictionary<string, SemaphoreSlim> _semaphores = new Dictionary<string, SemaphoreSlim>();

        public static async Task<Stream> OpenFile(string path)
        {
            return await Task.Run(() => File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite));
        }

        public static Task<string> GetText(string path)
        {
            return Task.Run(() =>
            {
                if (File.Exists(path))
                    return File.ReadAllText(path);

                return null;
            });
        }

        public static async Task WriteText(string path, string text)
        {
            var semaphore = GetSemaphore(path);
            await semaphore.WaitAsync();

            using (var stream = await OpenFile(path))
            {
                stream.SetLength(0);
                stream.WriteText(text);
            }

            _semaphores.Remove(path);
            semaphore.Release();
        }

        public static Task<bool> FileExists(string path)
        {
            return Task.Run(() => File.Exists(path));
        }

        public static Task DeleteFile(string path)
        {
            return Task.Run(() =>
            {
                if (!File.Exists(path))
                    throw new FileNotFoundException();
                File.Delete(path);
            });
        }

        public static DateTime GetFileUpdateTime(string path)
        {
            return File.GetLastWriteTime(path);
        }

        public static Task<bool> FolderExists(string path)
        {
            return Task.Run(() =>
            {
                return Directory.Exists(path);
            });
        }

        public static Task CreateFolder(string path)
        {
            return Task.Run(() =>
            {
                Directory.CreateDirectory(path);
            });
        }

        public static Task DeleteFolder(string path)
        {
            return Task.Run(() => Directory.Delete(path, true));
        }

        private static SemaphoreSlim GetSemaphore(string fileName)
        {
            if (_semaphores.ContainsKey(fileName))
                return _semaphores[fileName];

            var semaphore = new SemaphoreSlim(1);
            _semaphores[fileName] = semaphore;
            return semaphore;
        }
    }
}
