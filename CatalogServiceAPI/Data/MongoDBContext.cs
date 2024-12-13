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
                    _logger.LogInformation("Seeding initial product data...");

                    var products = new List<ProductDTO>
                    {
                        new ProductDTO
                        {
                            ProductId = Guid.NewGuid(),
                            Title = "Gaming Laptop",
                            Description = "High-performance laptop for gaming",
                            StartingPrice = 1500.00m,
                            Status = ProductStatus.Available,
                            CreatedAt = DateTime.UtcNow
                        },
                        new ProductDTO
                        {
                            ProductId = Guid.NewGuid(),
                            Title = "Wireless Headphones",
                            Description = "Noise-cancelling wireless headphones",
                            StartingPrice = 300.00m,
                            Status = ProductStatus.Available,
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    await productCollection.InsertManyAsync(products);
                    _logger.LogInformation("Product data seeded successfully.");
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
