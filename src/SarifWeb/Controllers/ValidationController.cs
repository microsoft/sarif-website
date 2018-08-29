using System.Web.Hosting;
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
    /// which is unit testable.. 
    /// </summary>
    /// </summary>
    public class ValidationController : ApiController
    {
        private readonly ValidationService _validationService;

        public ValidationController()
        {
            string multitoolDirectory = HostingEnvironment.MapPath("~/Multitool");
            IFileSystem fileSystem = new FileSystem();

            _validationService = new ValidationService(multitoolDirectory, fileSystem);
        }

        public ValidationResponse Post([FromBody] ValidationRequest validationRequest)
        {
            return _validationService.Validate(validationRequest);
        }
    }
}
