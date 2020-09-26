﻿using System;
using System.IO;
using System.Threading.Tasks;
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
        private const string ToolExeName = "Sarif.Multitool.exe";
        private const string ValidationLogSuffix = ".validation.sarif";
        private const string SchemaFileName = "sarif-schema.json";
        private const string PolicyFileName = "allRules.config.xml";

        private readonly string _postedFilesDirectory;
        private readonly string _multitoolExePath;
        private readonly string _policyFilesDirectory;
        private readonly string _schemaFilePath;
        private readonly IFileSystem _fileSystem;
        private readonly IProcessRunner _processRunner;

        public ValidationService(
            string postedFilesDirectory,
            string multitoolDirectory,
            string policyFilesDirectory,
            IFileSystem fileSystem,
            IProcessRunner processRunner)
        {
            _postedFilesDirectory = postedFilesDirectory;
            _multitoolExePath = Path.Combine(multitoolDirectory, ToolExeName);
            _schemaFilePath = Path.Combine(multitoolDirectory, SchemaFileName);
            _policyFilesDirectory = policyFilesDirectory;
            _fileSystem = fileSystem;
            _processRunner = processRunner;
        }

        public async Task<ValidationResponse> Validate(ValidationRequest validationRequest)
        {
            string inputFilePath = Path.Combine(_postedFilesDirectory, validationRequest.SavedFileName);
            string outputFileName = Path.GetFileNameWithoutExtension(validationRequest.PostedFileName) + ValidationLogSuffix;
            string outputFilePath = Path.Combine(_postedFilesDirectory, outputFileName);
            string configFilePath = Path.Combine(_policyFilesDirectory, PolicyFileName);

            string arguments = $"validate --output \"{outputFilePath}\" --json-schema \"{_schemaFilePath}\" --force --pretty-print --verbose --config \"{configFilePath}\" --rich-return-code \"{inputFilePath}\"";

            ValidationResponse validationResponse;
            try
            {
                string inputText = File.ReadAllText(inputFilePath);

                ProcessResult processResult = await _processRunner.RunProcess(_multitoolExePath, arguments);

                validationResponse = new ValidationResponse
                {
                    Message = $"The SARIF validation service received a request to validate \"{validationRequest.PostedFileName}\".",
                    Arguments = arguments,
                    ExitCode = processResult.ExitCode,
                    StandardError = processResult.StandardError,
                    StandardOutput = processResult.StandardOutput,
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