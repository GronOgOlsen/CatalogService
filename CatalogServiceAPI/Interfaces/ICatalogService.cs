using CatalogServiceAPI.Models;

namespace CatalogServiceAPI.Interfaces
{
    public interface ICatalogInterface
    {
        Task<ProductDTO?> GetProduct(Guid id);
        Task<IEnumerable<ProductDTO>> GetProductsByCategory(ProductCategory? category);
        Task<IEnumerable<ProductDTO>> GetAvailableProducts();
        Task<Guid> AddProduct(ProductDTO product);
        Task<long> UpdateProduct(ProductDTO product);
        Task<long> UpdateProductStatus(Guid id, ProductStatus newStatus, string? auctionId = null);
        Task<long> DeleteProduct(Guid id);
        Task<bool> PrepareForAuction(Guid productId);

    }
}
