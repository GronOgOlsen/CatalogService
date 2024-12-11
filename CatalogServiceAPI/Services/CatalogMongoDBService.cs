using CatalogServiceAPI.Models;
using CatalogServiceAPI.Data;
using CatalogServiceAPI.Interfaces;
using MongoDB.Driver;

namespace CatalogServiceAPI.Services
{
    public class CatalogMongoDBService : ICatalogInterface
    {
        private readonly ILogger<CatalogMongoDBService> _logger;
        private readonly IMongoCollection<ProductDTO> _productCollection;

        public CatalogMongoDBService(
            ILogger<CatalogMongoDBService> logger,
            MongoDBContext dbContext,
            IConfiguration configuration)
        {
            var collectionName = configuration["CollectionName"];
            if (string.IsNullOrEmpty(collectionName))
            {
                throw new ApplicationException("CatalogCollectionName is not configured.");
            }

            _logger = logger;
            _productCollection = dbContext.GetCollection<ProductDTO>(collectionName);
            _logger.LogInformation($"Collection name: {collectionName}");
        }

        public async Task<ProductDTO?> GetProduct(Guid id)
        {
            _logger.LogInformation($"Getting product with ID: {id}");
            var filter = Builders<ProductDTO>.Filter.Eq(p => p.ProductId, id);
            return await _productCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsByCategory(ProductCategory? category)
        {
            _logger.LogInformation($"Getting products for category: {category}");
            var filter = category.HasValue
                ? Builders<ProductDTO>.Filter.Eq(p => p.ProductCategory, category.Value)
                : Builders<ProductDTO>.Filter.Empty;
            return await _productCollection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<ProductDTO>> GetAvailableProducts()
        {
            _logger.LogInformation("Getting available products");
            var filter = Builders<ProductDTO>.Filter.Eq(p => p.Status, ProductStatus.Available);
            return await _productCollection.Find(filter).ToListAsync();
        }

        public async Task<Guid> AddProduct(ProductDTO product)
        {
            _logger.LogInformation($"Adding new product: {product.Title}");
            product.ProductId = Guid.NewGuid();
            product.CreatedAt = DateTime.UtcNow;
            product.Status = ProductStatus.Available;
            await _productCollection.InsertOneAsync(product);
            return product.ProductId;
        }

        public async Task<long> UpdateProduct(ProductDTO product)
        {
            _logger.LogInformation($"Updating product: {product.ProductId}");
            var filter = Builders<ProductDTO>.Filter.Eq(p => p.ProductId, product.ProductId);
            var result = await _productCollection.ReplaceOneAsync(filter, product);
            return result.ModifiedCount;
        }

        public async Task<long> UpdateProductStatus(Guid id, ProductStatus newStatus, string? auctionId = null)
        {
            _logger.LogInformation($"Updating product status: {id} to {newStatus}");

            var update = Builders<ProductDTO>.Update
                .Set(p => p.Status, newStatus);

            if (newStatus == ProductStatus.InAuction && !string.IsNullOrEmpty(auctionId))
            {
                update = update.Set(p => p.CurrentAuctionId, auctionId);
            }

            var filter = Builders<ProductDTO>.Filter.Eq(p => p.ProductId, id);
            var result = await _productCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount;
        }

        public async Task<long> DeleteProduct(Guid id)
        {
            _logger.LogInformation($"Deleting product: {id}");
            var filter = Builders<ProductDTO>.Filter.Eq(p => p.ProductId, id);
            var result = await _productCollection.DeleteOneAsync(filter);
            return result.DeletedCount;
        }

        public async Task<bool> PrepareForAuction(Guid productId)
        {
            var filter = Builders<ProductDTO>.Filter.And(
                Builders<ProductDTO>.Filter.Eq(p => p.ProductId, productId),
                Builders<ProductDTO>.Filter.Eq(p => p.Status, ProductStatus.Available)
            );

            var update = Builders<ProductDTO>.Update
                .Set(p => p.Status, ProductStatus.PendingAuction);

            var result = await _productCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
    }
}