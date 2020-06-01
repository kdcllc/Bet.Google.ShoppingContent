using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Bet.Google.ShoppingContent.Services;

using Google.Apis.ShoppingContent.v2_1.Data;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GoogleShoppingApp
{
    public class MerchantDemo
    {
        private readonly IGoogleMerchantConfigService _googleMerchant;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly ILogger<MerchantDemo> _logger;

        public MerchantDemo(
            IGoogleMerchantConfigService googleMerchant,
            IHostApplicationLifetime applicationLifetime,
            ILogger<MerchantDemo> logger)
        {
            _googleMerchant = googleMerchant ?? throw new ArgumentNullException(nameof(googleMerchant));
            _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<GoogleMerchantConfig?> GetMerchantAccountInfoAsync(
            bool saveToFile,
            CancellationToken cancellationToken = default)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _applicationLifetime.ApplicationStopping);

            var merchant = await _googleMerchant.GetAsync(linkedCts.Token);

            if (saveToFile)
            {
                var json = JsonSerializer.Serialize(merchant);

                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, $"{nameof(merchant)}.json"), json);

                _logger.LogInformation("Merchant Account: {json}", json);
            }

            return merchant;
        }

        public async Task<ShippingSettings> GetMerchantAccountShippingAsync(
            bool saveToFile,
            CancellationToken cancellationToken = default)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _applicationLifetime.ApplicationStopping);
            var shipping = await _googleMerchant.GetShippingSettingsAsync(linkedCts.Token);

            if (saveToFile)
            {
                var json = JsonSerializer.Serialize(shipping);

                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, $"{nameof(shipping)}.json"), json);

                _logger.LogInformation("Merchant Shipping Account: {json}", json);
            }

            return shipping;
        }
    }
}
