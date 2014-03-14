using System.IO;
using System.Threading.Tasks;

namespace Neptune.Storage
{
    /// <summary>
    /// Service for working with files and folders
    /// </summary>
    public interface IFileStorage
    {
        /// <summary>
        /// Opens file for read/write. If file is not exists it will be created.
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns><see cref="Stream"/></returns>
        Task<Stream> OpenFile(string path);

        /// <summary>
        /// Reads all text from the file. If file is not exists returns null.
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns>File content or null</returns>
        Task<string> GetText(string path);

        /// <summary>
        /// Writes text to the file. If file is not exists creates it.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="text">Text</param>
        Task WriteText(string path, string text);

        /// <summary>
        /// Checks if file exists
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns>True if file exists</returns>
        Task<bool> FileExists(string path);

        /// <summary>
        /// Deletes file. If file is not exists throws exception.
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns></returns>
        Task DeleteFile(string path);


        /// <summary>
        /// Checks if folder exists
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns>True if folder exists</returns>
        Task<bool> FolderExists(string path);

        /// <summary>
        /// Creates folder and all subfolders in specified path.
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns></returns>
        Task CreateFolder(string path);

        /// <summary>
        /// Deletes folder and all subfolders and files in specified path. Throws exception if folder is not exists.
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns></returns>
        Task DeleteFolder(string path);

        bool UserRoaming { get; set; }
    }
}
