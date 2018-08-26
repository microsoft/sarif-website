using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using SarifWeb.Models;

namespace SarifWeb.Controllers
{
    public class ValidationUiController : Controller
    {
        // GET: Validation
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task ValidateFilesAsync(IEnumerable<HttpPostedFileBase> uploadedFiles)
        {
            HttpPostedFileBase uploadedFile = uploadedFiles.FirstOrDefault();
            if (uploadedFile != null)
            {
                string uploadedFileName = uploadedFile.FileName;
                string savedFileName = Guid.NewGuid() + Path.GetExtension(uploadedFileName);
                string savedFilePath = Path.Combine(Server.MapPath("~/UploadedFiles"), savedFileName);

                try
                {
                    uploadedFile.SaveAs(savedFilePath);

                    using (var client = new HttpClient())
                    {
                        HttpRequestBase request = ControllerContext.RequestContext.HttpContext.Request;
                        request.ContentType = "application/json";
                        string baseAddress = string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, Url.Content("~"));
                        client.BaseAddress = new Uri(baseAddress, UriKind.Absolute);
                        client.DefaultRequestHeaders.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Send request to Validation service
                        ValidationRequestModel model = new ValidationRequestModel
                        {
                            UploadedFileName = uploadedFileName,
                            SavedFileName = savedFileName
                        };

                        string requestBody = JsonConvert.SerializeObject(model);
                        HttpContent requestContent = new StringContent(requestBody, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client.PostAsync("api/Validation", requestContent);
                        string responseContent = response.Content.ReadAsStringAsync().Result;
                    }
                }
                finally
                {
                    if (System.IO.File.Exists(savedFilePath))
                    {
                        System.IO.File.Delete(savedFilePath);
                    }
                }
            }
        }
    }
}