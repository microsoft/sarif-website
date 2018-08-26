using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public async Task<string> ValidateFileAsync(
            IEnumerable<HttpPostedFileBase> uploadedFiles,
            HttpRequestBase request,
            string uploadedFilesPath,
            string baseAddress)
        {
            string responseContent = string.Empty;

            HttpPostedFileBase uploadedFile = uploadedFiles.FirstOrDefault();
            if (uploadedFile != null)
            {
                string uploadedFileName = uploadedFile.FileName;
                string savedFileName = Guid.NewGuid() + Path.GetExtension(uploadedFileName);
                string savedFilePath = Path.Combine(uploadedFilesPath, savedFileName);

                try
                {
                    uploadedFile.SaveAs(savedFilePath);

                    using (var client = new HttpClient())
                    {
                        request.ContentType = "application/json";
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

                        HttpResponseMessage response = await _httpClientProxy.PostAsync(client, "api/Validation", requestContent);
                        responseContent = response.Content.ReadAsStringAsync().Result;
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

            return responseContent;
        }
    }
}