using System.Threading.Tasks;
using System.Web.Http;
using SarifWeb.Models;
using SarifWeb.Services;
using SarifWeb.Utilities;

namespace SarifWeb.Controllers
{
    /// <summary>
    /// This class receives requests to validate SARIF files, and returns JSON that
    /// describes the results of the validation. The bulk of the JSON will be a SARIF
    /// log file, but we introduce a wrapper class <see cref="ValidationResponse">
    /// to accommodate additional information about the request itself.
    ///
    /// This class delegates most of the work to the <see cref="ValidationService" />,
    /// which is unit testable.
    /// </summary>
    /// </summary>
    public class ValidationController : ApiController
    {
        private readonly ValidationService _validationService;

        public ValidationController()
        {
            _validationService = new ValidationService(
                HostingHelper.PostedFilesDirectory,
                HostingHelper.ValidationToolDirectory,
                HostingHelper.PolicyFilesDirectory,
                new FileSystem(),
                new ProcessRunner());
        }

        public async Task<ValidationResponse> Post([FromBody] ValidationRequest validationRequest)
        {
            return await _validationService.Validate(validationRequest);
        }
    }
}
