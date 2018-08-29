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
        private readonly string _multitoolDirectory;
        private readonly IFileSystem _fileSystem;

        public ValidationService(string multitoolDirectory, IFileSystem fileSystem)
        {
            _multitoolDirectory = multitoolDirectory;
            _fileSystem = fileSystem;
        }

        public ValidationResponse Validate(ValidationRequest validationRequest)
        {
            return new ValidationResponse { Message = $"The SARIF validation service received a request to validate \"{validationRequest.PostedFileName}\"." };
        }
    }
}