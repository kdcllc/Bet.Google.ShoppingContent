using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Bet.Google.ShoppingContent.Services;

using Google.Apis.ShoppingContent.v2_1.Data;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualBasic.CompilerServices;

namespace GoogleShoppingApp
{
    public class ProductDemo
    {
        private readonly IGoogleProductService _productService;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly ILogger<ProductDemo> _logger;

        public ProductDemo(
            IGoogleProductService productService,
            IHostApplicationLifetime applicationLifetime,
            ILogger<ProductDemo> logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IList<Product>> GetProductsAsync(
            bool saveToFile,
            CancellationToken cancellationToken = default)
        {
            var products = new List<Product>();

            try
            {
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _applicationLifetime.ApplicationStopping);
                var channel = _productService.GetChannelAsync(linkedCts.Token);

                await foreach (var item in channel.ReadAllAsync(linkedCts.Token))
                {
                    if (linkedCts.IsCancellationRequested)
                    {
                        break;
                    }

                    products.Add(item);
                }
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogDebug("Operation was canceled during products retrieval", ex.ToString());
            }
            finally
            {
                if (saveToFile)
                {
                    var json = JsonSerializer.Serialize(products);

                    File.WriteAllText(Path.Combine(AppContext.BaseDirectory, $"{nameof(products)}.json"), json);

                    _logger.LogInformation("Products : {json}", json);
                }
            }

            return products;
        }

        public async Task<IList<ProductStatus>> GetProductStatusesAsync(
            int take,
            IList<Product> products,
            bool saveToFile,
            CancellationToken cancellationToken = default)
        {
            var productStatuses = new List<ProductStatus>();
            var requestChannel = Channel.CreateUnbounded<(long batchId, string productId)>();

            try
            {
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _applicationLifetime.ApplicationStopping);
                await Task.Run((Func<Task?>)(async () =>
                {
                    var i = 0;

                    // var queryList = products.Take(take).ToDictionary(_ => (long)++i, p => p.Id);
                    foreach (var item in products.Take<Product>(take))
                    {
                        await requestChannel.Writer.WriteAsync((i, item.Id), cancellationToken);
                        ++i;
                    }

                    requestChannel.Writer.Complete();

                    var respChannel = _productService.GetStatusesChannelAsync(requestChannel, linkedCts.Token);

                    await foreach (var (batchId, productStatus) in respChannel.ReadAllAsync(cancellationToken))
                    {
                        productStatuses.Add(productStatus);
                    }
                }));
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogDebug("Operation was canceled during Product Statuses retrieval", ex.ToString());
            }
            finally
            {
                if (saveToFile)
                {
                    var json = JsonSerializer.Serialize(productStatuses);

                    File.WriteAllText(Path.Combine(AppContext.BaseDirectory, $"{nameof(productStatuses)}.json"), json);

                    _logger.LogInformation("Products : {productStatuses}", json);
                }
            }

            foreach (var item in productStatuses)
            {
                _logger.LogInformation(BuildStatusResult(item));
            }

            return productStatuses;
        }

        public async Task<IList<ProductStatus>> GetProductsWithErrorsAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<ProductStatus>();

            try
            {
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _applicationLifetime.ApplicationStopping);

                await Task.Run(async () =>
                {
                    var prodChannel = _productService.GetChannelAsync(linkedCts.Token);

                    var batchId = 0;

                    while (await prodChannel.WaitToReadAsync(linkedCts.Token))
                    {
                        var statusChannel = Channel.CreateUnbounded<(long batchId, string productId)>();

                        var prodItem = await prodChannel.ReadAsync(linkedCts.Token);
                        await statusChannel.Writer.WriteAsync((batchId, prodItem.Id), linkedCts.Token);
                        ++batchId;

                        statusChannel.Writer.Complete();

                        var statuses = _productService.GetStatusesChannelAsync(statusChannel, linkedCts.Token);

                        while (await statuses.WaitToReadAsync(linkedCts.Token))
                        {
                            var item = await statuses.ReadAsync(linkedCts.Token);

                            if (item.productStatus.ItemLevelIssues != null)
                            {
                                result.Add(item.productStatus);
                            }
                        }
                    }
                });
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogDebug("Operation was canceled during Product Statuses retrieval", ex.ToString());
            }

            return result;
        }

        private string BuildStatusResult(ProductStatus status)
        {
            var builder = new StringBuilder();

            builder.Append("Information for product: ").AppendLine(status.ProductId);
            builder.Append("- Title: ").AppendLine(status.Title);

            builder.AppendLine("- Destination statuses:");
            foreach (var stat in status.DestinationStatuses)
            {
                builder.AppendFormat("  - {0}: {1}", stat.Destination, stat.Status).AppendLine();
            }

            if (status.ItemLevelIssues == null)
            {
                builder.AppendLine("- No issues.");
            }
            else
            {
                var issues = status.ItemLevelIssues;
                builder.AppendFormat("- There are {0} issues:", issues.Count).AppendLine();
                foreach (var issue in issues)
                {
                    builder.AppendFormat("  - Code: {0}", issue.Code).AppendLine();
                    builder.AppendFormat("    Description: {0}", issue.Description).AppendLine();
                    builder.AppendFormat("    Detailed description: {0}", issue.Detail).AppendLine();
                    builder.AppendFormat("    Resolution: {0}", issue.Resolution).AppendLine();
                    builder.AppendFormat("    Servability: {0}", issue.Servability).AppendLine();
                }
            }

            return builder.ToString();
        }
    }
}
