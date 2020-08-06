using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;
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
        private const string VersionRegexPattern = @"\""version\"":\s*\""(?<version>[\d.]+)\""";

        private readonly string _postedFilesDirectory;
        private readonly string _multitoolExePath;
        private readonly string _schemaFilePath;
        private readonly IFileSystem _fileSystem;
        private readonly IProcessRunner _processRunner;

        public ValidationService(
            string postedFilesDirectory,
            string multitoolDirectory,
            IFileSystem fileSystem,
            IProcessRunner processRunner)
        {
            _postedFilesDirectory = postedFilesDirectory;
            _multitoolExePath = Path.Combine(multitoolDirectory, ToolExeName);
            _schemaFilePath = Path.Combine(multitoolDirectory, SchemaFileName);
            _fileSystem = fileSystem;
            _processRunner = processRunner;
        }

        public async Task<ValidationResponse> Validate(ValidationRequest validationRequest)
        {
            string inputFilePath = Path.Combine(_postedFilesDirectory, validationRequest.SavedFileName);
            string transformedFileName = Path.GetFileNameWithoutExtension(inputFilePath) + ".transformed.sarif";
            string transformedFilePath = Path.Combine(_postedFilesDirectory, transformedFileName);
            string outputFileName = Path.GetFileNameWithoutExtension(validationRequest.PostedFileName) + ValidationLogSuffix;
            string outputFilePath = Path.Combine(_postedFilesDirectory, outputFileName);

            string arguments = $"validate --output \"{outputFilePath}\" --json-schema \"{_schemaFilePath}\" --force --pretty-print --verbose --rich-return-code \"{transformedFilePath}\"";

            ValidationResponse validationResponse;
            try
            {
                string inputText = File.ReadAllText(inputFilePath);
                string inputVersion = null;

                // Get the schema version of the unmodified input log
                using (StringReader reader = new StringReader(inputText))
                {
                    // Read the first 100 characters. This avoids the problem of reading a huge line from a minified log.
                    char[] buffer = new char[100];
                    reader.ReadBlock(buffer, 0, buffer.Length);
                    Match match = Regex.Match(inputText, VersionRegexPattern, RegexOptions.Compiled);

                    if (match.Success)
                    {
                        inputVersion = match.Groups["version"].Value;
                    }
                }

                string transformedText;
                string transformedVersion = null;
                PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(inputText, Formatting.Indented, out transformedText);

                // Get the schema version of the transformed log
                using (StringReader reader = new StringReader(transformedText))
                {
                    // We know we have indent-formatted JSON, so read the third line which has the version property.
                    string line = null;
                    for (int i = 0; i < 3; i++)
                    {
                        line = reader.ReadLine();
                    }

                    Match match = Regex.Match(line, VersionRegexPattern, RegexOptions.Compiled);

                    if (match.Success)
                    {
                        transformedVersion = match.Groups["version"].Value;
                    }
                }

                File.WriteAllText(transformedFilePath, transformedText);

                ProcessResult processResult = await _processRunner.RunProcess(_multitoolExePath, arguments);

                validationResponse = new ValidationResponse
                {
                    Message = $"The SARIF validation service received a request to validate \"{validationRequest.PostedFileName}\".",
                    Arguments = arguments,
                    ExitCode = processResult.ExitCode,
                    StandardError = processResult.StandardError,
                    StandardOutput = processResult.StandardOutput,
                    IsTransformed = inputVersion != transformedVersion,
                    TransformedLogContents = transformedText,
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

                if (_fileSystem.FileExists(transformedFilePath))
                {
                    _fileSystem.DeleteFile(transformedFilePath);
                }
            }

            return validationResponse;
        }
    }
}