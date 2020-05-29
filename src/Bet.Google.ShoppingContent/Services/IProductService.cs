using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.ShoppingContent.v2_1.Data;

namespace Bet.Google.ShoppingContent.Services
{
    /// <summary>
    /// Google Shopping Content for Products in the Catalog.
    /// </summary>
    public interface IProductService
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
        Task<IList<Product>> GetAllAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Get All Product Statues listed on Google Shopping Content.
        /// </summary>
        /// <param name="cancellationToken">The CancellationToken.</param>
        /// <returns></returns>
        Task<IList<ProductStatus>> GetAllStatusAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Get A Product listed on Google Shopping Content.
        /// </summary>
        /// <param name="productId">The product id as string.</param>
        /// <param name="cancellationToken">The CancellationToken.</param>
        /// <returns></returns>
        Task<Product> GetAsync(string productId, CancellationToken cancellationToken);

        /// <summary>
        /// Get All Product Statutes based on the list of products.
        /// </summary>
        /// <param name="productList">The list of products.</param>
        /// <param name="cancellationToken">The CancellationToken.</param>
        /// <returns></returns>
        Task<IDictionary<long, ProductStatus>> GetStatusAsync(IDictionary<long, string> productList, CancellationToken cancellationToken);

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
        Task<IDictionary<long, Product>> UpInsertBatchAsync(IDictionary<long, Product> productList, CancellationToken cancellationToken);
    }
}
