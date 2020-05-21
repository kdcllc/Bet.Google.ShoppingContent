using System;
using System.Threading;
using System.Threading.Tasks;

using Bet.Google.ShoppingContent.Options;

using Google.Apis.ShoppingContent.v2_1;
using Google.Apis.ShoppingContent.v2_1.Data;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Google.ShoppingContent.Services
{
    public class MerchantConfigService
    {
        private readonly AppsOptions _options;
        private readonly ShoppingContentService _contentService;
        private readonly ILogger<MerchantConfigService> _logger;

        public MerchantConfigService(
            IOptions<AppsOptions> options,
            ShoppingContentService contentService,
            ILogger<MerchantConfigService> logger)
        {
            _options = options.Value;
            _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MerchantConfig> GetAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving information for authenticated user.");
            var authinfo = await _contentService.Accounts.Authinfo().ExecuteAsync(cancellationToken);

            if (authinfo.AccountIdentifiers.Count == 0)
            {
                throw new ArgumentException("Authenticated user has no access to any Merchant Center accounts.");
            }

            var config = new MerchantConfig();

            var firstAccount = authinfo.AccountIdentifiers[0];
            config.MerchantId = firstAccount.MerchantId ?? firstAccount.AggregatorId;
            _logger.LogInformation("Using Merchant Center {merchantId} for running samples.", config.MerchantId.GetValueOrDefault());

            var merchantId = config?.MerchantId.GetValueOrDefault();

            // We detect whether the requested Merchant Center ID is an MCA by checking
            // Accounts.authinfo(). If it is an MCA, then the authenticated user must be
            // a user of that account, which means it'll be listed here, and it must
            // appear in the AggregatorId field of one of the AccountIdentifier entries.
            config.IsMCA = false;
            foreach (var accountId in authinfo.AccountIdentifiers)
            {
                if (merchantId == accountId.AggregatorId)
                {
                    config.IsMCA = true;
                    break;
                }

                if (merchantId == accountId.MerchantId)
                {
                    break;
                }
            }

            _logger.LogInformation("Merchant Center {merchantId} is{isMCA} an MCA.", merchantId, config.IsMCA ? string.Empty : " not");

            return config;
        }

        public async Task<ShippingSettings> GetShippingSettingsAsync(CancellationToken cancellationToken)
        {
            return await _contentService.Shippingsettings.Get(_options.MerchantId, _options.MerchantId).ExecuteAsync(cancellationToken);
        }
    }
}
