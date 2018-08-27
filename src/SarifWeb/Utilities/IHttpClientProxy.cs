using System.Net.Http;
using System.Threading.Tasks;

namespace SarifWeb.Utilities
{
    public interface IHttpClientProxy
    {
        Task<HttpResponseMessage> PostAsync(HttpClient client, string requestUri, HttpContent requestContent);
    }
}