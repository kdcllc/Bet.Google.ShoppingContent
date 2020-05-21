using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Bet.Google.ShoppingContent.Options;

using Google.Apis.ShoppingContent.v2_1;
using Google.Apis.ShoppingContent.v2_1.Data;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Google.ShoppingContent.Services
{
    public class ProductService
    {
        private readonly AppsOptions _options;
        private readonly ShoppingContentService _contentService;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IOptions<AppsOptions> options,
            ShoppingContentService contentService,
            ILogger<ProductService> logger)
        {
            _options = options.Value;
            _contentService = contentService ?? throw new System.ArgumentNullException(nameof(contentService));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        public async Task<IList<Product>> GetProducts(CancellationToken cancellationToken)
        {
            string? pageToken = null;
            var result = new List<Product>();

            do
            {
                var accountRequest = _contentService.Products.List(_options.MerchantId);
                accountRequest.MaxResults = _options.MaxListPageSize;
                accountRequest.PageToken = pageToken;

                var productsResponse = await accountRequest.ExecuteAsync(cancellationToken);

                if (productsResponse.Resources != null && productsResponse.Resources.Count != 0)
                {
                   result.AddRange(productsResponse.Resources);
                }
                else
                {
                    _logger.LogInformation("No accounts found.");
                }

                pageToken = productsResponse.NextPageToken;
            }
            while (pageToken != null);

            return result;
        }
    }
}
