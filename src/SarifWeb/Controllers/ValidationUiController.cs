using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using SarifWeb.Models;
using SarifWeb.Services;
using SarifWeb.Utilities;
using FromBody = System.Web.Http.FromBodyAttribute;

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
        private readonly IFileSystem _fileSystem;
        private readonly ValidationUiService _validationUiService;

        public ValidationUiController()
        {
            // CONSIDER: Use the Unity DI container to inject dependencies, rather than
            // instantiating them by hand.
            _fileSystem = new FileSystem();
            IHttpClientProxy httpClientProxy = new HttpClientProxy();

            _validationUiService = new ValidationUiService(_fileSystem, httpClientProxy);
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
        /// An serialized object that describes the results of the validation.
        /// </returns>
        /// <remarks>
        /// This method is invoked from Javascript by the JQuery filedrop API. See
        /// Views/ValidationUi/Index.cshtml. For this reason, we must return the
        /// JSON serialization of the <see cref="ValidationResponse"/> object,
        /// rather than returning the object itself.
        /// </remarks>
        [HttpPost]
        public async Task<string> ValidateFilesAsync(IEnumerable<HttpPostedFileBase> postedFiles)
        {
            // The ValidationUi Index view enforces a limit of one file at a time.
            HttpPostedFileBase postedFile = postedFiles.FirstOrDefault();

            string postedFileName = postedFile.FileName;
            string savedFileName = Guid.NewGuid() + Path.GetExtension(postedFileName);
            string savedFilePath = Path.Combine(HostingHelper.PostedFilesDirectory, savedFileName);
            postedFile.SaveAs(savedFilePath);

            ValidationResponse response = await GetFileValidationResponse(savedFilePath);
            return JsonConvert.SerializeObject(response);
        }

        [HttpPost]
        public async Task<string> ValidateJsonAsync([FromBody] string json)
        {
            string fileName = $"{Guid.NewGuid()}.sarif";
            string filePath = Path.Combine(HostingHelper.PostedFilesDirectory, fileName);
            _fileSystem.WriteAllText(filePath, json);

            ValidationResponse response = await GetFileValidationResponse(filePath);
            return JsonConvert.SerializeObject(response);
        }

        private async Task<ValidationResponse> GetFileValidationResponse(string filePath)
        {
            // Extract information from the parts of the Controller object that are hard to mock.
            HttpRequestBase request = ControllerContext.RequestContext.HttpContext.Request;
            string baseAddress = string.Format(
                CultureInfo.InvariantCulture,
                "{0}://{1}{2}",
                request.Url.Scheme, request.Url.Authority, Url.Content("~"));

            return await _validationUiService.ValidateFileAsync(filePath, request, baseAddress);
        }
    }
}