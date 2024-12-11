using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

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
                // Læs konfigurationsværdier for MongoDB
                var connectionString = configuration["MongoConnectionString"]
                    ?? throw new ArgumentNullException("MongoConnectionString not found in configuration");
                var databaseName = configuration["DatabaseName"]
                    ?? throw new ArgumentNullException("DatabaseName not found in configuration");

                _logger.LogInformation("Initializing MongoDB connection");

                // Opret forbindelse til MongoDB
                var client = new MongoClient(connectionString);
                _database = client.GetDatabase(databaseName);

                _logger.LogInformation($"Connected to database: {databaseName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to establish MongoDB connection");
                throw;
            }
        }

        // Hent en specifik collection fra MongoDB
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            _logger.LogInformation($"Getting collection: {collectionName}");
            return _database.GetCollection<T>(collectionName);
        }
    }
}
