using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.Sarif;
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
        private readonly ValidationService _validationService;
        private readonly IFileSystem _fileSystem;

        public ValidationUiService(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _validationService = new ValidationService(
                HostingHelper.PostedFilesDirectory,
                HostingHelper.PolicyFilesDirectory,
                fileSystem);
        }

        public ValidationResponse ValidateFile(
            IFormFile postedFile,
            string postedFilesPath)
        {
            ValidationResponse validationResponse = null;

            if (postedFile != null)
            {
                string postedFileName = postedFile.FileName;
                string savedFileName = Guid.NewGuid() + Path.GetExtension(postedFileName);
                string savedFilePath = Path.Combine(postedFilesPath, savedFileName);

                using (Stream fileStream = new FileStream(savedFilePath, FileMode.Create))
                {
                    postedFile.CopyTo(fileStream);
                }

                validationResponse = ValidateSavedFile(postedFileName, savedFilePath);
            }

            return validationResponse;
        }

        public ValidationResponse ValidateJson(
            string json,
            string postedFilesPath)
        {
            string savedFileName = $"{Guid.NewGuid()}.sarif";
            string savedFilePath = Path.Combine(postedFilesPath, savedFileName);

            _fileSystem.FileWriteAllText(savedFilePath, json);

            return ValidateSavedFile(savedFileName, savedFilePath);
        }

        private ValidationResponse ValidateSavedFile(
            string originalFileName,
            string savedFilePath)
        {
            ValidationResponse validationResponse = null;

            // Send request to Validation service
            ValidationRequest validationRequest = new ValidationRequest
            {
                PostedFileName = originalFileName,
                SavedFileName = Path.GetFileName(savedFilePath)
            };

            try
            {
                validationResponse = _validationService.Validate(validationRequest);
            }
            finally
            {
                if (_fileSystem.FileExists(savedFilePath))
                {
                    _fileSystem.FileDelete(savedFilePath);
                }
            }

            return validationResponse;
        }
    }
}