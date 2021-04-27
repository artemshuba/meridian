using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Jupiter.Utils.Extensions;

namespace Jupiter.Utils.Helpers
{
    public static class FileStorageHelper
    {
        public static bool IsFileExists(string path, IStorageFolder rootFolder = null)
        {
            var localFolder = rootFolder ?? ApplicationData.Current.LocalFolder;
            var fullPath = Path.Combine(localFolder.Path, path);
            return File.Exists(fullPath);
        }

        public static bool IsFolderExists(string path, IStorageFolder rootFolder = null)
        {
            var localFolder = rootFolder ?? ApplicationData.Current.LocalFolder;
            var fullPath = Path.Combine(localFolder.Path, path);
            return Directory.Exists(fullPath);
        }

        public static async Task<StorageFolder> CreateFolder(string path, IStorageFolder rootFolder = null, CreationCollisionOption options = CreationCollisionOption.FailIfExists)
        {
            var localFolder = rootFolder ?? ApplicationData.Current.LocalFolder;
            return await localFolder.CreateFolderAsync(path, options);
        }

        public static async Task WriteText(string path, string text, IStorageFolder rootFolder = null)
        {
            var folder = rootFolder ?? ApplicationData.Current.LocalFolder;

            var file = await folder.CreateFileAsync(path, CreationCollisionOption.ReplaceExisting);

            using (var stream = await file.OpenStreamForWriteAsync())
            {
                stream.SetLength(0);
                stream.WriteText(text);
            }
        }

        public static async Task AppendText(string path, string text, IStorageFolder rootFolder = null)
        {
            var folder = rootFolder ?? ApplicationData.Current.LocalFolder;

            var file = await folder.CreateFileAsync(path, CreationCollisionOption.OpenIfExists);

            using (var stream = await file.OpenStreamForWriteAsync())
            {
                stream.Seek(stream.Length, SeekOrigin.Begin);
                stream.WriteText(text);
            }
        }

        public static async Task<string> ReadText(string path, IStorageFolder rootFolder = null)
        {
            var folder = rootFolder ?? ApplicationData.Current.LocalFolder;
            var file = await folder.TryGetItemAsync(path) as StorageFile;

            if (file == null)
                return null;

            using (var stream = await file.OpenStreamForReadAsync())
            {
                return stream.ReadText();
            }
        }

        public static async Task<Stream> OpenFileRead(string path, IStorageFolder rootFolder = null)
        {
            var folder = rootFolder ?? ApplicationData.Current.LocalFolder;
            var file = await folder.GetFileAsync(path);

            return (await file.OpenReadAsync()).AsStreamForRead();
        }

        public static async Task<Stream> OpenFileWrite(string path, IStorageFolder rootFolder = null)
        {
            var folder = rootFolder ?? ApplicationData.Current.LocalFolder;
            var file = await folder.CreateFileAsync(path, CreationCollisionOption.ReplaceExisting);

            return (await file.OpenAsync(FileAccessMode.ReadWrite)).AsStreamForWrite();
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

        public static async Task DeleteFile(string path, IStorageFolder rootFolder = null)
        {
            var folder = rootFolder ?? ApplicationData.Current.LocalFolder;
            var file = await folder.GetItemAsync(path);
            await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }

        public static async Task DeleteFolder(string path, IStorageFolder rootFolder = null)
        {
            var folder = rootFolder ?? ApplicationData.Current.LocalFolder;
            var f = await folder.GetItemAsync(path);
            await f.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }

        public static async Task<DateTimeOffset> GetLastModifiedDate(string path, IStorageFolder rootFolder = null)
        {
            var folder = rootFolder ?? ApplicationData.Current.LocalFolder;
            var file = await folder.GetFileAsync(path);

            return (await file.GetBasicPropertiesAsync()).DateModified;
        }
    }
}
