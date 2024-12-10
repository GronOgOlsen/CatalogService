using CatalogServiceAPI.Models;
using CatalogServiceAPI.Data;
using MongoDB.Driver;
using CatalogServiceAPI.Interfaces;

namespace CatalogServiceAPI.Services
{
    public class CatalogMongoDBService : ICatalogInterface
    {
        private readonly ILogger<CatalogMongoDBService> _logger;
        private readonly IMongoCollection<Catalog> _catalogCollection;

        public CatalogMongoDBService(ILogger<CatalogMongoDBService> logger, MongoDBContext dbContext, IConfiguration configuration)
        {
            var collectionName = configuration["collectionName"];
            if (string.IsNullOrEmpty(collectionName))
            {
                throw new ApplicationException("CatalogCollectionName is not configured.");
            }

            _logger = logger;
            _catalogCollection = dbContext.GetCollection<Catalog>(collectionName);
            _logger.LogInformation($"Collection name: {collectionName}");
        }

        public async Task<Catalog?> GetCatalog(Guid _id)
        {
            var filter = Builders<Catalog>.Filter.Eq(x => x._id, _id);
            return await _catalogCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Catalog>?> GetCatalogList()
        {
            return await _catalogCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Guid> AddCatalog(Catalog catalog)
        {
            if (catalog._id == Guid.Empty)
            {
                catalog._id = Guid.NewGuid();
            }
            await _catalogCollection.InsertOneAsync(catalog);
            return catalog._id;
        }

        public async Task<long> UpdateCatalog(Catalog catalog)
        {
            var filter = Builders<Catalog>.Filter.Eq(x => x._id, catalog._id);
            var result = await _catalogCollection.ReplaceOneAsync(filter, catalog);
            return result.ModifiedCount;
        }

        public async Task<long> DeleteCatalog(Guid _id)
        {
            var filter = Builders<Catalog>.Filter.Eq(x => x._id, _id);
            var result = await _catalogCollection.DeleteOneAsync(filter);
            return result.DeletedCount;
        }
    }
}