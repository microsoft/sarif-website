using System;
using System.Collections.Generic;
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
            string postedFilesPath,
            List<RuleKind> ruleKinds)
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

                validationResponse = ValidateSavedFile(postedFileName, savedFilePath, ruleKinds);
            }

            return validationResponse;
        }

        public ValidationResponse ValidateJson(
            string json,
            string postedFilesPath,
            List<RuleKind> ruleKinds)
        {
            string savedFileName = $"{Guid.NewGuid()}.sarif";
            string savedFilePath = Path.Combine(postedFilesPath, savedFileName);

            _fileSystem.FileWriteAllText(savedFilePath, json);

            return ValidateSavedFile(savedFileName, savedFilePath, ruleKinds);
        }

        private ValidationResponse ValidateSavedFile(
            string originalFileName,
            string savedFilePath,
            List<RuleKind> ruleKinds)
        {
            ValidationResponse validationResponse = null;

            // Send request to Validation service
            ValidationRequest validationRequest = new ValidationRequest
            {
                PostedFileName = originalFileName,
                SavedFileName = Path.GetFileName(savedFilePath),
                RuleKinds = ruleKinds
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