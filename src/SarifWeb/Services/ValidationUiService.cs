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
    /// turn presents it in the UI.
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
            ValidationResponse validationResponse = null;

            if (postedFile != null)
            {
                string postedFileName = postedFile.FileName;
                string savedFileName = Guid.NewGuid() + Path.GetExtension(postedFileName);
                string savedFilePath = Path.Combine(postedFilesPath, savedFileName);

                try
                {
                    postedFile.SaveAs(savedFilePath);
                    request.ContentType = "application/json";

                    // Send request to Validation service
                    ValidationRequest validationRequest = new ValidationRequest
                    {
                        PostedFileName = postedFileName,
                        SavedFileName = savedFileName
                    };

                    validationResponse = await GetValidationResponse(validationRequest, baseAddress);
                }
                finally
                {
                    if (_fileSystem.FileExists(savedFilePath))
                    {
                        _fileSystem.DeleteFile(savedFilePath);
                    }
                }
            }

            return validationResponse;
        }

        public async Task<ValidationResponse> ValidateJSonAsync(
            string json,
            HttpRequestBase request,
            string postedFilesPath,
            string baseAddress)
        {
            string savedFileName = $"{Guid.NewGuid()}.sarif";
            string savedFilePath = Path.Combine(postedFilesPath, savedFileName);

            _fileSystem.WriteAllText(savedFilePath, json);
            request.ContentType = "application/json";

            // Send request to Validation service
            ValidationRequest validationRequest = new ValidationRequest
            {
                PostedFileName = savedFileName,
                SavedFileName = savedFileName
            };

            ValidationResponse validationResponse = null;

            try
            {
                validationResponse = await GetValidationResponse(validationRequest, baseAddress);
            }
            finally
            {
                if (_fileSystem.FileExists(savedFilePath))
                {
                    _fileSystem.DeleteFile(savedFilePath);
                }
            }

            return validationResponse;
        }

        private async Task<ValidationResponse> GetValidationResponse(
            ValidationRequest validationRequest,
            string baseAddress)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseAddress, UriKind.Absolute);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string requestBody = JsonConvert.SerializeObject(validationRequest);
                HttpContent requestContent = new StringContent(requestBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClientProxy.PostAsync(client, "api/Validation", requestContent);
                string responseContent = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<ValidationResponse>(responseContent);
            }
        }
    }
}