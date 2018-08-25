using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SarifWeb.Controllers
{
    public class ValidationController : ApiController
    {
        // GET: api/Validation
        public IEnumerable<string> Get()
        {
            return new string[] { "SARIF Validation API is online" };
        }

        // POST: api/Validation
        public void Post([FromBody]string value)
        {
        }
    }
}
