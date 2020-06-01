using System.Threading;
using System.Threading.Tasks;

using Google.Apis.ShoppingContent.v2_1.Data;

namespace Bet.Google.ShoppingContent.Services
{
    public interface IGoogleMerchantConfigService
    {
        Task<GoogleMerchantConfig> GetAsync(CancellationToken cancellationToken);

        Task<ShippingSettings> GetShippingSettingsAsync(CancellationToken cancellationToken);
    }
}
