using CatalogServiceAPI.Models;

namespace CatalogServiceAPI.Interfaces
{
    public interface ICatalogInterface
    {
        Task<IEnumerable<ProductDTO>> GetAllProducts();
        Task<ProductDTO?> GetProduct(Guid id);
        Task<IEnumerable<ProductDTO>> GetAvailableProducts();
        Task<Guid> AddProduct(ProductDTO product);
        Task<long> UpdateProduct(ProductDTO product);
        Task<bool> PrepareForAuction(Guid productId);
        Task<bool> SetInAuction(Guid productId, Guid auctionId);
        Task<bool> SetSold(Guid productId);
        Task<bool> SetFailedInAuction(Guid productId);
        Task<long> DeleteProduct(Guid id);
    }
}
