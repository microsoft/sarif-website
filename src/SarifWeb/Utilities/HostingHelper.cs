using System.Web.Hosting;

namespace SarifWeb.Utilities
{
    internal static class HostingHelper
    {
        public static string PostedFilesDirectory =>
            HostingEnvironment.MapPath("~/UploadedFiles");

        public static string PolicyFilesDirectory =>
            HostingEnvironment.MapPath("~/bin/policies");
    }
}