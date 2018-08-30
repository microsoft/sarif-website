using System.Diagnostics;
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

        private readonly string _multitoolExePath;
        private readonly IFileSystem _fileSystem;
        private readonly IProcessRunner _processRunner;

        public ValidationService(string multitoolDirectory, IFileSystem fileSystem, IProcessRunner processRunner)
        {
            _multitoolExePath = Path.Combine(multitoolDirectory, ToolExeName);
            _fileSystem = fileSystem;
            _processRunner = processRunner;
        }

        public async Task<ValidationResponse> Validate(ValidationRequest validationRequest)
        {
            string stdout = string.Empty;
            string stderr = string.Empty;

            string arguments = "validate --help";
            ProcessResult processResult = await _processRunner.RunProcess(_multitoolExePath, arguments);

            return new ValidationResponse
            {
                Message = $"The SARIF validation service received a request to validate \"{validationRequest.PostedFileName}\".",
                ExitCode = processResult.ExitCode,
                StdErr = processResult.StdErr,
                StdOut = processResult.StdOut
            };
        }
    }
}