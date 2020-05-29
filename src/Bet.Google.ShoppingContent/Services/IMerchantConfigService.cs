using System.Threading;
using System.Threading.Tasks;

using Google.Apis.ShoppingContent.v2_1.Data;

namespace Bet.Google.ShoppingContent.Services
{
    public interface IMerchantConfigService
    {
        Task<MerchantConfig> GetAsync(CancellationToken cancellationToken);

        Task<ShippingSettings> GetShippingSettingsAsync(CancellationToken cancellationToken);
    }
}
