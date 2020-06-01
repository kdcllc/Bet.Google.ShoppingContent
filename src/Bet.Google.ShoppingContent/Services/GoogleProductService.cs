using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Bet.Google.ShoppingContent.Options;

using Google.Apis.ShoppingContent.v2_1;
using Google.Apis.ShoppingContent.v2_1.Data;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Google.ShoppingContent.Services
{
    /// <inheritdoc/>
    public class GoogleProductService : IGoogleProductService
    {
        private readonly GoogleShoppingOptions _options;
        private readonly ShoppingContentService _contentService;
        private readonly ILogger<GoogleProductService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleProductService"/> class.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="contentService"></param>
        /// <param name="logger"></param>
        public GoogleProductService(
            IOptions<GoogleShoppingOptions> options,
            ShoppingContentService contentService,
            ILogger<GoogleProductService> logger)
        {
            _options = options.Value;
            _contentService = contentService ?? throw new System.ArgumentNullException(nameof(contentService));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<Product> GetAsync(string productId, CancellationToken cancellationToken)
        {
            return await _contentService.Products.Get(_options.MerchantId, productId).ExecuteAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IList<Product>> GetAsync(CancellationToken cancellationToken)
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
                    _logger.LogInformation("No products found.");
                }

                pageToken = productsResponse.NextPageToken;
            }
            while (pageToken != null);

            return result;
        }

        /// <inheritdoc/>
        public ChannelReader<Product> GetChannelAsync(CancellationToken cancellationToken)
        {
            var output = Channel.CreateUnbounded<Product>();
            Task.Run(async () =>
            {
                try
                {
                    string? pageToken = null;
                    do
                    {
                        var accountRequest = _contentService.Products.List(_options.MerchantId);
                        accountRequest.MaxResults = _options.MaxListPageSize;
                        accountRequest.PageToken = pageToken;

                        var productsResponse = await accountRequest.ExecuteAsync(cancellationToken);

                        if (productsResponse.Resources != null && productsResponse.Resources.Count != 0)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                break;
                            }

                            foreach (var item in productsResponse.Resources)
                            {
                                await output.Writer.WriteAsync(item, cancellationToken);
                            }
                        }
                        else
                        {
                            _logger.LogInformation("No products found.");
                        }

                        pageToken = productsResponse.NextPageToken;
                    }
                    while (pageToken != null);
                }
                finally
                {
                    output.Writer.Complete();
                }
            });

            return output;
        }

        /// <inheritdoc/>
        public async Task<IList<ProductStatus>> GetStatusesAsync(CancellationToken cancellationToken)
        {
            string? pageToken = null;
            var result = new List<ProductStatus>();
            do
            {
                var accountRequest = _contentService.Productstatuses.List(_options.MerchantId);
                accountRequest.MaxResults = _options.MaxListPageSize;
                accountRequest.PageToken = pageToken;

                var productsResponse = await accountRequest.ExecuteAsync(cancellationToken);

                if (productsResponse.Resources != null
                    && productsResponse.Resources.Count != 0)
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

        /// <inheritdoc/>
        public async Task<Product> UpInsertAsync(Product product, CancellationToken cancellationToken)
        {
            return await _contentService.Products.Insert(product, _options.MerchantId).ExecuteAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IDictionary<long, (Product product, Errors errors)>> UpInsertBatchAsync(
            IDictionary<long, Product> productList,
            CancellationToken cancellationToken)
        {
            var result = new Dictionary<long, (Product product, Errors errors)>();

            var batchRequest = new ProductsCustomBatchRequest
            {
                Entries = new List<ProductsCustomBatchRequestEntry>()
            };

            foreach (var item in productList)
            {
                var entry = new ProductsCustomBatchRequestEntry
                {
                    BatchId = item.Key,
                    MerchantId = _options.MerchantId,
                    Method = "insert",
                    Product = item.Value
                };
                batchRequest.Entries.Add(entry);
            }

            var response = await _contentService.Products.Custombatch(batchRequest).ExecuteAsync(cancellationToken);

            if (response.Kind == "content#productsCustomBatchResponse")
            {
                for (var i = 0; i < response.Entries.Count; i++)
                {
                    var insertedProduct = response.Entries[i].Product;
                    result.Add(response.Entries[i].BatchId ?? 0, (insertedProduct, response.Entries[i].Errors));
                }
            }
            else
            {
                _logger.LogInformation("There was an error. Response: {responseCode}", response.ToString());
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<string> DeleteAsync(string productId, CancellationToken cancellationToken)
        {
            return await _contentService.Products.Delete(_options.MerchantId, productId).ExecuteAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IDictionary<long, string>> DeleteBatchAsync(IDictionary<long, string> productIdList, CancellationToken cancellationToken)
        {
            var result = new Dictionary<long, string>();

            var batchRequest = new ProductsCustomBatchRequest
            {
                Entries = new List<ProductsCustomBatchRequestEntry>()
            };

            foreach (var item in productIdList)
            {
                var entry = new ProductsCustomBatchRequestEntry
                {
                    BatchId = item.Key,
                    MerchantId = _options.MerchantId,
                    Method = "delete",
                    ProductId = item.Value
                };
                batchRequest.Entries.Add(entry);
            }

            var response = await _contentService.Products.Custombatch(batchRequest).ExecuteAsync(cancellationToken);

            if (response.Kind == "content#productsCustomBatchResponse")
            {
                for (var i = 0; i < response.Entries.Count; i++)
                {
                    var errors = response.Entries[i].Errors;
                    var flatError = string.Empty;
                    if (errors != null)
                    {
                        for (var j = 0; j < errors.ErrorsValue.Count; j++)
                        {
                            flatError += errors.ErrorsValue[j].ToString();
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Product deleted, batchId {0}", response.Entries[i].BatchId);
                    }

                    result.Add(response.Entries[i].BatchId ?? 0, flatError);
                }
            }
            else
            {
                _logger.LogError("There was an error. Response: {response}", response);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<ProductStatus> GetStatusAsync(string productId, CancellationToken cancellationToken)
        {
            return await _contentService.Productstatuses.Get(_options.MerchantId, productId).ExecuteAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IDictionary<long, ProductStatus>> GetStatusesAsync(IDictionary<long, string> productList, CancellationToken cancellationToken)
        {
            var result = new Dictionary<long, ProductStatus>();

            var batchRequest = new ProductstatusesCustomBatchRequest
            {
                Entries = new List<ProductstatusesCustomBatchRequestEntry>()
            };

            foreach (var item in productList)
            {
                var entry = new ProductstatusesCustomBatchRequestEntry
                {
                    BatchId = item.Key,
                    MerchantId = _options.MerchantId,
                    Method = "get",
                    ProductId = item.Value
                };
                batchRequest.Entries.Add(entry);
            }

            var response = await _contentService.Productstatuses.Custombatch(batchRequest).ExecuteAsync(cancellationToken);

            if (response.Kind == "content#productstatusesCustomBatchResponse")
            {
                for (var i = 0; i < response.Entries.Count; i++)
                {
                    var productStatus = response.Entries[i].ProductStatus;
                    result.Add(response.Entries[i].BatchId ?? 0, productStatus);
                }
            }
            else
            {
                _logger.LogDebug("There was an error. Response: {responseCode}", response.ToString());
            }

            return result;
        }

        /// <inheritdoc/>
        public ChannelReader<(long batchId, ProductStatus productStatus)> GetStatusesChannelAsync(
            ChannelReader<(long batchId, string productId)> channel,
            CancellationToken cancellationToken)
        {
            var output = Channel.CreateUnbounded<(long, ProductStatus)>();

            Task.Run(async () =>
            {
                try
                {
                    var batchRequest = new ProductstatusesCustomBatchRequest
                    {
                        Entries = new List<ProductstatusesCustomBatchRequestEntry>()
                    };

                    while (await channel.WaitToReadAsync(cancellationToken))
                    {
                        var (batchId, productId) = await channel.ReadAsync(cancellationToken);

                        var entry = new ProductstatusesCustomBatchRequestEntry
                        {
                            BatchId = batchId,
                            MerchantId = _options.MerchantId,
                            Method = "get",
                            ProductId = productId
                        };
                        batchRequest.Entries.Add(entry);
                    }

                    var response = await _contentService.Productstatuses.Custombatch(batchRequest).ExecuteAsync(cancellationToken);

                    if (response.Kind == "content#productstatusesCustomBatchResponse")
                    {
                        for (var i = 0; i < response.Entries.Count; i++)
                        {
                            var productStatus = response.Entries[i].ProductStatus;
                            await output.Writer.WriteAsync((response.Entries[i].BatchId ?? 0, productStatus), cancellationToken);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("There was an error. Response: {responseCode}", response.ToString());
                    }
                }
                finally
                {
                    output.Writer.Complete();
                }
            });

            return output;
        }

        /// <inheritdoc/>
        public ChannelReader<ProductStatus> GetStatusesChannelAsync(CancellationToken cancellationToken)
        {
            var output = Channel.CreateUnbounded<ProductStatus>();

            Task.Run(async () =>
            {
                string? pageToken = null;
                try
                {
                    do
                    {
                        var accountRequest = _contentService.Productstatuses.List(_options.MerchantId);
                        accountRequest.MaxResults = _options.MaxListPageSize;
                        accountRequest.PageToken = pageToken;

                        var productsResponse = await accountRequest.ExecuteAsync(cancellationToken);

                        if (productsResponse.Resources != null
                            && productsResponse.Resources.Count != 0)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                break;
                            }

                            foreach (var item in productsResponse.Resources)
                            {
                                await output.Writer.WriteAsync(item, cancellationToken);
                            }
                        }
                        else
                        {
                            _logger.LogInformation("No accounts found.");
                        }

                        pageToken = productsResponse.NextPageToken;
                    }
                    while (pageToken != null);
                }
                finally
                {
                    output.Writer.Complete();
                }
            });

            return output;
        }
    }
}
