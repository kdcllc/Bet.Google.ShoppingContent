using System.Threading.Tasks;

using Google.Apis.Http;

namespace Bet.Google.ShoppingContent.Services
{
    public interface IGoogleAuthenticationService
    {
        Task<IConfigurableHttpClientInitializer> AuthenticateAsync(string scope = "");
    }
}
