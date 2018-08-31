using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SarifWeb.Utilities;
using SarifWeb.Services;
using SarifWeb.Models;

namespace SarifWeb.Controllers
{
    /// <summary>
    /// This class receives requests from the Web UI to validate SARIF files. It delegates
    /// most of the work to the ValidationUiService, which is unit testable. This class
    /// has only a thin layer of code to extract information from the hard-to-test portions
    /// of the Controller object and pass them on to the service.
    /// </summary>
    public class ValidationUiController : Controller
    {
        private readonly ValidationUiService _validationUiService;

        public ValidationUiController()
        {
            // CONSIDER: Use the Unity DI container to inject dependencies, rather than
            // instantiating them by hand.
            IFileSystem fileSystem = new FileSystem();
            IHttpClientProxy httpClientProxy = new HttpClientProxy();

            _validationUiService = new ValidationUiService(fileSystem, httpClientProxy);
        }

        /// <summary>
        /// Present the drag-and-drop UI for uploading files to be validated.
        /// </summary>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Send an uploaded file to the ValidationUiService, which will post it to the
        /// Validation Web API controller, which performs the actual validation.
        /// </summary>
        /// <param name="postedFiles">
        /// The files that the user dropped onto the drag-and-drop UI.
        /// </param>
        /// <returns>
        /// An object that describes the results of the validation.
        /// </returns>
        /// <remarks>
        /// This method is invoked from Javascript by the JQuery filedrop API. See
        /// Views/ValidationUi/Index.cshtml.
        /// </remarks>
        [HttpPost]
        public async Task<ValidationResponse> ValidateFilesAsync(IEnumerable<HttpPostedFileBase> postedFiles)
        {
            // Extract information from the parts of the Controller object that are hard to mock.
            HttpRequestBase request = ControllerContext.RequestContext.HttpContext.Request;
            string postedFilesDirectory = HostingHelper.PostedFilesDirectory;
            string baseAddress = string.Format(
                CultureInfo.InvariantCulture,
                "{0}://{1}{2}",
                request.Url.Scheme, request.Url.Authority, Url.Content("~"));

            // The JQuery filedrop API allows you to drop multiple files, but at least for
            // now, for simplicity, the ValidationUi Index view enforces a limit of one file
            // at a time (it does this by setting maxfiles to 1 in the handler for the filedrop event).
            HttpPostedFileBase postedFile = postedFiles.FirstOrDefault();

            return await _validationUiService.ValidateFileAsync(postedFile, request, postedFilesDirectory, baseAddress);
        }
    }
}