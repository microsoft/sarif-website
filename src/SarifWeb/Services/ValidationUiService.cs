using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using SarifWeb.Models;
using SarifWeb.Utilities;

namespace SarifWeb.Services
{
    /// <summary>
    /// This class processes requests from the Web UI to validate SARIF files.
    /// It synthesized a request to the Validation Web API, sends the request,
    /// and passes the response back to the ValidationUiController, which in
    /// turns presents it in the UI.
    /// </summary>
    /// <remarks>
    /// This class is factored out from the ValidationUiController so that as
    /// much of the code as possible is unit testable. ValidationUiController
    /// contains the minimal remaining code that is difficult to unit test.
    /// </remarks>
    public class ValidationUiService
    {
        private readonly IFileSystem _fileSystem;
        private readonly IHttpClientProxy _httpClientProxy;

        public ValidationUiService(
            IFileSystem fileSystem,
            IHttpClientProxy httpClientProxy)
        {
            _fileSystem = fileSystem;
            _httpClientProxy = httpClientProxy;
        }

        public async Task<ValidationResponse> ValidateFileAsync(
            HttpPostedFileBase postedFile,
            HttpRequestBase request,
            string postedFilesPath,
            string baseAddress)
        {
            ValidationResponse responseModel = null;

            if (postedFile != null)
            {
                string postedFileName = postedFile.FileName;
                string savedFileName = Guid.NewGuid() + Path.GetExtension(postedFileName);
                string savedFilePath = Path.Combine(postedFilesPath, savedFileName);

                try
                {
                    postedFile.SaveAs(savedFilePath);

                    using (var client = new HttpClient())
                    {
                        request.ContentType = "application/json";
                        client.BaseAddress = new Uri(baseAddress, UriKind.Absolute);
                        client.DefaultRequestHeaders.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Send request to Validation service
                        ValidationRequest model = new ValidationRequest
                        {
                            PostedFileName = postedFileName,
                            SavedFileName = savedFileName
                        };

                        string requestBody = JsonConvert.SerializeObject(model);
                        HttpContent requestContent = new StringContent(requestBody, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await _httpClientProxy.PostAsync(client, "api/Validation", requestContent);
                        string responseContent = response.Content.ReadAsStringAsync().Result;
                        responseModel = JsonConvert.DeserializeObject<ValidationResponse>(responseContent);

                    }
                }
                finally
                {
                    if (_fileSystem.FileExists(savedFilePath))
                    {
                        _fileSystem.DeleteFile(savedFilePath);
                    }
                }
            }

            return responseModel;
        }
    }
}