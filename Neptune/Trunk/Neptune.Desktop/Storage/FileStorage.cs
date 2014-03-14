using System.IO;
using System.Threading.Tasks;
using Neptune.Extensions;
using Neptune.Storage;

namespace Neptune.Desktop.Storage
{
    public class FileStorageService : IFileStorage
    {
        private static FileStorageService _intance;

        public static IFileStorage Instance
        {
            get
            {
                if (_intance == null)
                    _intance = new FileStorageService();
                return _intance;
            }
        }

        public async Task<Stream> OpenFile(string path)
        {
            return await Task.Run(() => File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite));
        }

        public Task<string> GetText(string path)
        {
            return Task.Run(() =>
            {
                if (File.Exists(path))
                    return File.ReadAllText(path);

                return null;
            });
        }

        public async Task WriteText(string path, string text)
        {
            using (var stream = await OpenFile(path))
            {
                stream.SetLength(0);
                stream.WriteText(text);
            }
        }

        public Task<bool> FileExists(string path)
        {
            return Task.Run(() => File.Exists(path));
        }

        public Task DeleteFile(string path)
        {
            return Task.Run(() =>
            {
                if (!File.Exists(path))
                    throw new FileNotFoundException();
                File.Delete(path);
            });
        }

        public Task<bool> FolderExists(string path)
        {
            return Task.Run(() =>
            {
                return Directory.Exists(path);
            });
        }

        public Task CreateFolder(string path)
        {
            return Task.Run(() =>
            {
                Directory.CreateDirectory(path);
            });
        }

        public Task DeleteFolder(string path)
        {
            return Task.Run(() => Directory.Delete(path, true));
        }

        public bool UserRoaming { get; set; }
    }
}
