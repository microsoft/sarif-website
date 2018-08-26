using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SarifWeb.Utilities;
using SarifWeb.Services;
using SarifWeb.Models;
using Newtonsoft.Json;

namespace SarifWeb.Controllers
{
    public class ValidationUiController : Controller
    {
        private readonly ValidationUiService _validationUiService;

        public ValidationUiController()
        {
            IFileSystem fileSystem = new FileSystem();
            IHttpClientProxy httpClientProxy = new HttpClientProxy();

            _validationUiService = new ValidationUiService(fileSystem, httpClientProxy);
        }

        // GET: Validation
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ValidationResponseModel> ValidateFilesAsync(IEnumerable<HttpPostedFileBase> uploadedFiles)
        {
            HttpRequestBase request = ControllerContext.RequestContext.HttpContext.Request;
            string uploadedFilesPath = Server.MapPath("~/UploadedFiles");
            string baseAddress = string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, Url.Content("~"));

            string responseContent = await _validationUiService.ValidateFileAsync(uploadedFiles, request, uploadedFilesPath, baseAddress);
            return JsonConvert.DeserializeObject<ValidationResponseModel>(responseContent);
        }
    }
}