using System;
using System.IO;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Multitool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SarifWeb.Models;

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
            string outputFileName = Path.GetFileNameWithoutExtension(validationRequest.SavedFileName) + ValidationLogSuffix;
            string outputFilePath = Path.Combine(_postedFilesDirectory, outputFileName);
            string configFilePath = Path.Combine(_policyFilesDirectory, PolicyFileName);

            ValidationResponse validationResponse;
            try
            {
                var validateOptions = new ValidateOptions
                {
                    OutputFilePath = outputFilePath,
                    Force = true,
                    PrettyPrint = true,
                    Level = new FailureLevel[] {
                        FailureLevel.Error, FailureLevel.Warning, FailureLevel.Note
                    },
                    Kind = new ResultKind[] {
                        ResultKind.Fail, ResultKind.Informational, ResultKind.NotApplicable, ResultKind.Open, ResultKind.Open, ResultKind.Pass, ResultKind.Review
                    },
                    ConfigurationFilePath = configFilePath,
                    TargetFileSpecifiers = new string[] { inputFilePath },
                    MaxFileSizeInKilobytes = 1024 * 1024 // 1 GB
                };

                // Ensure that the input file is formatted as pretty-printed JSON.
                JObject jObject = JObject.Parse(File.ReadAllText(inputFilePath));
                string prettyJson = jObject.ToString(Formatting.Indented);
                File.WriteAllText(inputFilePath, prettyJson);

                var validateResponse = new ValidateCommand().Run(validateOptions);

                validationResponse = new ValidationResponse
                {
                    Message = $"The SARIF validation service received a request to validate \"{validationRequest.PostedFileName}\".",
                    ExitCode = validateResponse,
                    Arguments = string.Empty,
                    StandardError = string.Empty,
                    StandardOutput = string.Empty,
                    InputLogContents = prettyJson,
                    ResultsLogContents = _fileSystem.FileReadAllText(outputFilePath)
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
                    _fileSystem.FileDelete(outputFilePath);
                }

                if (_fileSystem.FileExists(inputFilePath))
                {
                    _fileSystem.FileDelete(inputFilePath);
                }
            }

            return validationResponse;
        }
    }
}