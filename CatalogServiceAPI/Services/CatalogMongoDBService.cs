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

        public async Task<Guid> AddProduct(ProductDTO product)
        {
            _logger.LogInformation($"Adding new product: {product.Title}");
            product.ProductId = Guid.NewGuid();
            product.CreatedAt = DateTime.UtcNow;
            await _productCollection.InsertOneAsync(product);
            return product.ProductId;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllProducts()
        {
            _logger.LogInformation("Getting all products");
            return await _productCollection.Find(Builders<ProductDTO>.Filter.Empty).ToListAsync();
        }

        public async Task<ProductDTO?> GetProduct(Guid id)
        {
            _logger.LogInformation($"Getting product with ID: {id}");
            var filter = Builders<ProductDTO>.Filter.Eq(p => p.ProductId, id);
            return await _productCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ProductDTO>> GetAvailableProducts()
        {
            _logger.LogInformation("Getting available products");
            var filter = Builders<ProductDTO>.Filter.Eq(p => p.Status, ProductStatus.Available);
            return await _productCollection.Find(filter).ToListAsync();
        }

        public async Task<long> UpdateProduct(ProductDTO product)
        {
            _logger.LogInformation($"Updating product: {product.ProductId}");
            var filter = Builders<ProductDTO>.Filter.Eq(p => p.ProductId, product.ProductId);
            var result = await _productCollection.ReplaceOneAsync(filter, product);
            return result.ModifiedCount;
        }

        // Metoden skal ikke kræve at et produktets status er pending, hvis en auktion slettes skal produktet sættes til available igen.
        public async Task<bool> PrepareForAuction(Guid productId)
        {
            var filter = Builders<ProductDTO>.Filter.And(
                Builders<ProductDTO>.Filter.Eq(p => p.ProductId, productId)
            );

            var update = Builders<ProductDTO>.Update
                .Set(p => p.Status, ProductStatus.Available);

            var result = await _productCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> SetInAuction(Guid productId, Guid auctionId)
        {
            var filter = Builders<ProductDTO>.Filter.And(
                Builders<ProductDTO>.Filter.Eq(p => p.ProductId, productId),
                Builders<ProductDTO>.Filter.Eq(p => p.Status, ProductStatus.Available)
            );

            var update = Builders<ProductDTO>.Update
                .Set(p => p.Status, ProductStatus.InAuction)
                .Set(p => p.CurrentAuctionId, auctionId); 

            var result = await _productCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> SetSold(Guid productId)
        {
            var filter = Builders<ProductDTO>.Filter.And(
                Builders<ProductDTO>.Filter.Eq(p => p.ProductId, productId),
                Builders<ProductDTO>.Filter.Eq(p => p.Status, ProductStatus.InAuction)
            );

            var update = Builders<ProductDTO>.Update
                .Set(p => p.Status, ProductStatus.Sold);

            var result = await _productCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> SetFailedInAuction(Guid productId)
        {
            var filter = Builders<ProductDTO>.Filter.And(
                Builders<ProductDTO>.Filter.Eq(p => p.ProductId, productId),
                Builders<ProductDTO>.Filter.Eq(p => p.Status, ProductStatus.InAuction)
            );

            var update = Builders<ProductDTO>.Update
                .Set(p => p.Status, ProductStatus.FailedInAuction);

            var result = await _productCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<long> DeleteProduct(Guid id)
        {
            _logger.LogInformation($"Deleting product: {id}");
            var filter = Builders<ProductDTO>.Filter.Eq(p => p.ProductId, id);
            var result = await _productCollection.DeleteOneAsync(filter);
            return result.DeletedCount;
        }
    }
}