using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;


namespace CatalogServiceAPI.Data
{
    public class MongoDBContext
    {
        private readonly IMongoDatabase _database;

        public MongoDBContext(ILogger<MongoDBContext> iLogger, IConfiguration configuration)
            {
                var connectionString = configuration["MongoConnectionString"];
                var databaseName = configuration["DatabaseName"]; 
                
                iLogger.LogInformation($"Connection string: {connectionString}");
                iLogger.LogInformation($"Database name: {databaseName}");

                // Konfigurer MongoClient med GuidRepresentation
                var mongoClientSettings = MongoClientSettings.FromConnectionString(connectionString);
                mongoClientSettings.GuidRepresentation = GuidRepresentation.Standard;

                var client = new MongoClient(connectionString);
                _database = client.GetDatabase(databaseName);
            }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
            {
                return _database.GetCollection<T>(collectionName);
            }
    }
}