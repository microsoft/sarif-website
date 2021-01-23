using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace SarifWeb.Utilities
{
    internal static class HostingHelper
    {
        private static IWebHostEnvironment _hostingEnvironment;

        public static void Initialize(IWebHostEnvironment hostEnvironment)
        {
            _hostingEnvironment = hostEnvironment;
        }

        public static string PostedFilesDirectory => Path.Combine(_hostingEnvironment.ContentRootPath, "UploadedFiles");

        public static string PolicyFilesDirectory => Path.Combine(_hostingEnvironment.ContentRootPath, "policies");
    }
}