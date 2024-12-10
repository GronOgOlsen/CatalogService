using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace CatalogServiceAPI.Models
{
    public class Catalog
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)] // Gem Guid som string i MongoDB
        public Guid _id { get; set; } = Guid.NewGuid();
        public string title { get; set; }
        public string description { get; set; }
        public string category { get; set; }
        public float startingPrice { get; set; }
        public string status { get; set; }

    }
}