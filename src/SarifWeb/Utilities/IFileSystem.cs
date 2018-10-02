namespace SarifWeb.Utilities
{
    /// <summary>
    /// Mockable interface around the file system.
    /// </summary>
    public interface IFileSystem
    {
        void DeleteFile(string path);
        bool FileExists(string path);
        string ReadAllText(string path);
        void WriteAllText(string path, string contents);
    }
}