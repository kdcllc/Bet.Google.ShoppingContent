using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Google.Apis.ShoppingContent.v2_1.Data;

namespace Bet.Google.ShoppingContent.Services
{
    /// <summary>
    /// Google Shopping Content for Products in the Catalog.
    /// </summary>
    public interface IGoogleProductService
    {
        /// <summary>
        /// Delete the Product from Google Shopping Content.
        /// </summary>
        /// <param name="productId">The product id as string.</param>
        /// <param name="cancellationToken">The CancellationToken.</param>
        /// <returns></returns>
        Task<string> DeleteAsync(string productId, CancellationToken cancellationToken);

        /// <summary>
        /// Delete the Batch of Product from Google Shopping Content.
        /// </summary>
        /// <param name="productIdList"></param>
        /// <param name="cancellationToken">The CancellationToken.</param>
        /// <returns></returns>
        Task<IDictionary<long, string>> DeleteBatchAsync(IDictionary<long, string> productIdList, CancellationToken cancellationToken);

        /// <summary>
        /// Get all of the Products listing on Google Shopping Content.
        /// </summary>
        /// <param name="cancellationToken">The CancellationToken.</param>
        /// <returns></returns>
        Task<IList<Product>> GetAsync(CancellationToken cancellationToken);

        ChannelReader<Product> GetChannelAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Get All Product Statues listed on Google Shopping Content.
        /// </summary>
        /// <param name="cancellationToken">The CancellationToken.</param>
        /// <returns></returns>
        Task<IList<ProductStatus>> GetStatusesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Get Product Statuses.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        ChannelReader<ProductStatus> GetStatusesChannelAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Get A Product listed on Google Shopping Content.
        /// </summary>
        /// <param name="productId">The product id as string.</param>
        /// <param name="cancellationToken">The CancellationToken.</param>
        /// <returns></returns>
        Task<Product> GetAsync(string productId, CancellationToken cancellationToken);

        /// <summary>
        /// Get Product Statutes based on the list of products.
        /// </summary>
        /// <param name="productList">The list of products.</param>
        /// <param name="cancellationToken">The CancellationToken.</param>
        /// <returns></returns>
        Task<IDictionary<long, ProductStatus>> GetStatusesAsync(IDictionary<long, string> productList, CancellationToken cancellationToken);

        /// <summary>
        /// Get Product Statues based on the list of products.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public ChannelReader<(long batchId, ProductStatus productStatus)> GetStatusesChannelAsync(ChannelReader<(long batchId, string productId)> channel, CancellationToken cancellation);

        /// <summary>
        /// Get Status for specific product id.
        /// </summary>
        /// <param name="productId">The product id as string.</param>
        /// <param name="cancellationToken">The CancellationToken.</param>
        /// <returns></returns>
        Task<ProductStatus> GetStatusAsync(string productId, CancellationToken cancellationToken);

        /// <summary>
        /// Update or Insert a product into Google Shopping Content.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="cancellationToken">The CancellationToken.</param>
        /// <returns></returns>
        Task<Product> UpInsertAsync(Product product, CancellationToken cancellationToken);

        /// <summary>
        /// Update or Insert a batch of Products into Google Shopping Content.
        /// </summary>
        /// <param name="productList"></param>
        /// <param name="cancellationToken">The CancellationToken.</param>
        /// <returns></returns>
        Task<IDictionary<long, (Product product, Errors errors)>> UpInsertBatchAsync(IDictionary<long, Product> productList, CancellationToken cancellationToken);
    }
}
