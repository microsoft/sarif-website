using System.Web.Http;
using SarifWeb.Models;

namespace SarifWeb.Controllers
{
    /// <summary>
    /// This class receives requests to validate SARIF files, and returns JSON that
    /// describes the results of the validation. The bulk of the JSON will be a SARIF
    /// log file, but we introduce a wrapper class <see cref="ValidationResponse">
    /// to accommodate additional information about the request itself.
    /// </summary>
    public class ValidationController : ApiController
    {
        public ValidationResponse Post([FromBody] ValidationRequest model)
        {
            return new ValidationResponse { Message = $"The SARIF validation service received a request to validate {model.PostedFileName}" };
        }
    }
}
