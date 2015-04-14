using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meridian.Services;
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

                    var files = GetDirectoryFiles(folder.Path, "*.*", SearchOption.AllDirectories)
                        .Where(s => s.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
                                        || s.EndsWith(".wma", StringComparison.OrdinalIgnoreCase)).ToList();
                    //var files = Directory.EnumerateFiles(folder.Path, "*.*", SearchOption.AllDirectories)
                    //        .Where(s => s.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
                    //                    || s.EndsWith(".wma", StringComparison.OrdinalIgnoreCase)).ToList();

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


        //http://stackoverflow.com/questions/5098011/directory-enumeratefiles-unauthorizedaccessexception
        /// <summary>
        /// A safe way to get all the files in a directory and sub directory without crashing on UnauthorizedException
        /// </summary>
        /// <param name="rootPath">Starting directory</param>
        /// <param name="patternMatch">Filename pattern match</param>
        /// <param name="searchOption">Search subdirectories or only top level directory for files</param>
        /// <returns>List of files</returns>
        public static IEnumerable<string> GetDirectoryFiles(string rootPath, string patternMatch, SearchOption searchOption)
        {
            IEnumerable<string> foundFiles = Enumerable.Empty<string>(); // Start with an empty container

            if (searchOption == SearchOption.AllDirectories)
            {
                try
                {
                    IEnumerable<string> subDirs = Directory.EnumerateDirectories(rootPath);
                    foreach (string dir in subDirs)
                    {
                        foundFiles = foundFiles.Concat(GetDirectoryFiles(dir, patternMatch, searchOption)); // Add files in subdirectories recursively to the list
                    }
                }
                catch (UnauthorizedAccessException) { } // Incase we have an access error - we don't want to mask the rest
            }

            try
            {
                foundFiles = foundFiles.Concat(Directory.EnumerateFiles(rootPath, patternMatch)); // Add files from the current directory to the list
            }
            catch (UnauthorizedAccessException) { } // Incase we have an access error - we don't want to mask the rest

            return foundFiles; // This is it finally
        }
    }
}
