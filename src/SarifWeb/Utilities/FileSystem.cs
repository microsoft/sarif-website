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
            int retry = 5;

            while (true)
            {
                try
                {
                    File.Delete(path);
                    return;
                }
                catch (IOException ex)
                {
                    retry--;
                    if (retry < 0) throw ex;
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }
    }
}