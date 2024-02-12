using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Sarif;

using SarifWeb.Models;
using SarifWeb.Services;
using SarifWeb.Utilities;

namespace SarifWeb.Controllers
{
    /// <summary>
    /// This class receives requests to validate SARIF files, and returns JSON that
    /// describes the results of the validation. The bulk of the JSON will be a SARIF
    /// log file, but we introduce a wrapper class <see cref="ValidationResponse">
    /// to accommodate additional information about the request itself.
    ///
    /// This class delegates most of the work to the <see cref="ValidationService" />,
    /// which is unit testable.
    /// </summary>
    [Route("[controller]")]
    public class ValidationController : Controller
    {
        private readonly ValidationService _validationService;
        private readonly ValidationUiService _validationUiService;

        public ValidationController()
        {
            // CONSIDER: Use the Unity DI container to inject dependencies, rather than
            // instantiating them by hand.
            IFileSystem fileSystem = FileSystem.Instance;

            _validationUiService = new ValidationUiService(fileSystem);
            _validationService = new ValidationService(
                HostingHelper.PostedFilesDirectory,
                HostingHelper.PolicyFilesDirectory,
                fileSystem);
        }

        /// <summary>
        /// Present the drag-and-drop UI for uploading files to be validated.
        /// </summary>
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        public ValidationResponse Post([FromBody] ValidationRequest validationRequest)
        {
            return _validationService.Validate(validationRequest);
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
        /// This method is invoked from Javascript by the jQuery filedrop API. See
        /// Views/Validation/Index.cshtml. For this reason, we must return the
        /// JSON serialization of the <see cref="ValidationResponse"/> object,
        /// rather than returning the object itself.
        /// </remarks>
        [HttpPost("ValidateFiles")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
        public IActionResult ValidateFiles(IEnumerable<IFormFile> postedFiles, List<RuleKind> ruleKinds)
        {
            string postedFilesDirectory = HostingHelper.PostedFilesDirectory;

            // The ValidationUi Index view enforces a limit of one file at a time.
            IFormFile postedFile = postedFiles.FirstOrDefault();

            ValidationResponse response = _validationUiService.ValidateFile(postedFile, postedFilesDirectory, ruleKinds);
            return Json(response);
        }

        [HttpPost("ValidateJson")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
        public IActionResult ValidateJson([FromBody] string json, List<RuleKind> ruleKinds)
        {
            string postedFilesDirectory = HostingHelper.PostedFilesDirectory;

            ValidationResponse response = _validationUiService.ValidateJson(json, postedFilesDirectory, ruleKinds);
            return Json(response);
        }
    }
}
