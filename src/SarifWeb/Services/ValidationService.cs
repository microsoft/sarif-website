using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Sarif.Multitool;
using SarifWeb.Models;
using SarifWeb.Utilities;

namespace SarifWeb.Services
{
    /// <summary>
    /// This class processes requests from the Validation Web API to validate SARIF
    /// files. It invokes "Sarif.Multitool validate" on the specified file
    /// and passes the response back to the ValidationController, which in
    /// turns passes it back to its invoker.
    /// </summary>
    /// <remarks>
    /// This class is factored out from the ValidationController so that as
    /// much of the code as possible is unit testable. ValidationController
    /// contains the minimal remaining code that is difficult to unit test.
    /// </remarks>
    public class ValidationService
    {
        private const string ValidationLogSuffix = ".validation.sarif";
        private const string PolicyFileName = "allRules.config.xml";

        private readonly string _postedFilesDirectory;
        private readonly string _policyFilesDirectory;
        private readonly IFileSystem _fileSystem;

        public ValidationService(
            string postedFilesDirectory,
            string policyFilesDirectory,
            IFileSystem fileSystem)
        {
            _postedFilesDirectory = postedFilesDirectory;
            _policyFilesDirectory = policyFilesDirectory;
            _fileSystem = fileSystem;
        }

        public ValidationResponse Validate(ValidationRequest validationRequest)
        {
            string inputFilePath = Path.Combine(_postedFilesDirectory, validationRequest.SavedFileName);
            string outputFileName = Path.GetFileNameWithoutExtension(validationRequest.PostedFileName) + ValidationLogSuffix;
            string outputFilePath = Path.Combine(_postedFilesDirectory, outputFileName);
            string configFilePath = Path.Combine(_policyFilesDirectory, PolicyFileName);

            ValidationResponse validationResponse;
            try
            {
                string inputText = File.ReadAllText(inputFilePath);
                var validateOptions = new ValidateOptions
                {
                    OutputFilePath = outputFilePath,
                    Force = true,
                    PrettyPrint = true,
                    Verbose = true,
                    ConfigurationFilePath = configFilePath,
                    TargetFileSpecifiers = new string[] { inputFilePath }
                };

                var validateResponse = new ValidateCommand().Run(validateOptions);

                validationResponse = new ValidationResponse
                {
                    Message = $"The SARIF validation service received a request to validate \"{validationRequest.PostedFileName}\".",
                    ExitCode = validateResponse,
                    Arguments = string.Empty,
                    StandardError = string.Empty,
                    StandardOutput = string.Empty,
                    InputLogContents = inputText,
                    ResultsLogContents = _fileSystem.ReadAllText(outputFilePath)
                };
            }
            catch (Exception ex)
            {
                validationResponse = new ValidationResponse
                {
                    ExitCode = int.MaxValue,
                    Message = $"Validation of file {validationRequest.PostedFileName} failed: {ex.Message}"
                };
            }
            finally
            {
                if (_fileSystem.FileExists(outputFilePath))
                {
                    _fileSystem.DeleteFile(outputFilePath);
                }

                if (_fileSystem.FileExists(inputFilePath))
                {
                    _fileSystem.DeleteFile(inputFilePath);
                }
            }

            return validationResponse;
        }
    }
}