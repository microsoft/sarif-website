using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SarifWeb.Utilities
{
    public class HttpClientProxy : IHttpClientProxy
    {
        public async Task<HttpResponseMessage> PostAsync(HttpClient client, string requestUri, HttpContent requestContent)
        {
            return await client.PostAsync(requestUri, requestContent);
        }
    }
}