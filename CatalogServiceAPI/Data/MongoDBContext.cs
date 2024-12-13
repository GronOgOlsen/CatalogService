using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CatalogServiceAPI.Models;

namespace CatalogServiceAPI.Data
{
    public class MongoDBContext
    {
        private readonly IMongoDatabase _database;
        private readonly ILogger<MongoDBContext> _logger;

        public MongoDBContext(ILogger<MongoDBContext> logger, IConfiguration configuration)
        {
            _logger = logger;

            try
            {
                var connectionString = configuration["MongoConnectionString"];
                var databaseName = configuration["DatabaseName"];

                _logger.LogInformation($"Connecting to MongoDB: {connectionString}, Database: {databaseName}");

                var client = new MongoClient(connectionString);
                _database = client.GetDatabase(databaseName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing MongoDB context");
                throw;
            }
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }

        public async Task SeedDataAsync()
        {
            try
            {
                var productCollection = GetCollection<ProductDTO>("Products");

                // Check if products already exist
                var productExists = await productCollection.Find(_ => true).AnyAsync();
                if (!productExists)
                {
                    _logger.LogInformation("Seeding initial product data with status 'Pending'...");

                    var products = new List<ProductDTO>
            {
                new ProductDTO
                {
                    ProductId = Guid.NewGuid(),
                    ProductCategory = ProductCategory.Electronics,
                    Title = "Alienware Aurora R15 Gaming Desktop",
                    Description = "Cutting-edge gaming desktop with Intel i9 processor and NVIDIA RTX 4090 GPU.",
                    StartingPrice = 3500.00m,
                    Status = ProductStatus.Pending, // Status sat til Pending
                    CreatedAt = DateTime.UtcNow,
                    SellerId = Guid.NewGuid().ToString()
                },
                new ProductDTO
                {
                    ProductId = Guid.NewGuid(),
                    ProductCategory = ProductCategory.Jewelry,
                    Title = "Cartier Love Bracelet",
                    Description = "Iconic Cartier bracelet crafted in 18k gold, symbolizing eternal love.",
                    StartingPrice = 7000.00m,
                    Status = ProductStatus.Pending, // Status sat til Pending
                    CreatedAt = DateTime.UtcNow,
                    SellerId = Guid.NewGuid().ToString()
                }
            };

                    await productCollection.InsertManyAsync(products);
                    _logger.LogInformation("Product data seeded successfully with 'Pending' status.");
                }
                else
                {
                    _logger.LogInformation("Database already contains product data. Skipping seeding.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during data seeding");
                throw;
            }
        }
    }
}
