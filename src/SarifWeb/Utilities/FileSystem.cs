using System.IO;

namespace SarifWeb.Utilities
{
    /// <summary>
    /// Product implementation of the mockable <see cref="IFileSystem"/> interface.
    /// </summary>
    public class FileSystem : IFileSystem
    {
        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }
    }
}