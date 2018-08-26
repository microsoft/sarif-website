using System.Web.Http;
using SarifWeb.Models;

namespace SarifWeb.Controllers
{
    public class ValidationController : ApiController
    {
        public ValidationResponseModel Post([FromBody] ValidationRequestModel model)
        {
            return new ValidationResponseModel { Message = $"The SARIF validation service received a request to validate {model.UploadedFileName}" };
        }
    }
}
