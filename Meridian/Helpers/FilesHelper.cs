using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Shell;

namespace Meridian.Helpers
{
    public static class FilesHelper
    {
        public static List<string> GetMusicFiles()
        {
            var musicFiles = new List<string>();

            if (ShellLibrary.IsPlatformSupported)
            {
                var library = ShellLibrary.Load("Music", true);
                foreach (var folder in library)
                {
                    if (!Directory.Exists(folder.Path))
                        continue;

                    var files = Directory.EnumerateFiles(folder.Path, "*.*", SearchOption.AllDirectories)
                            .Where(s => s.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
                                        || s.EndsWith(".wma", StringComparison.OrdinalIgnoreCase)).ToList();

                    musicFiles.AddRange(files);
                }
            }
            else
            {
                musicFiles =
                    Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "*.*", SearchOption.AllDirectories)
                        .Where(s => s.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
                                    || s.EndsWith(".wma", StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return musicFiles;
        }
    }
}
