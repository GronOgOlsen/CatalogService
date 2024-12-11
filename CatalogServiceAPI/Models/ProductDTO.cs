using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CatalogServiceAPI.Models
{
    public class ProductDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid ProductId { get; set; }

        public ProductCategory ProductCategory { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal StartingPrice { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.Available;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? SellerId { get; set; }
        public string? CurrentAuctionId { get; set; }
    }

    public enum ProductCategory
    {
        None = 0,
        Art = 1,
        Electronics = 2,
        Fashion = 3,
        Collectibles = 4,
        Jewelry = 5,
        Vehicles = 6,
        Other = 7
    }

    public enum ProductStatus
    {
        Available,      // Produkt er tilgængeligt for auktion
        PendingAuction, // Produkt er ved at blive sat på auktion
        InAuction,      // Produkt er aktivt i en auktion
        Sold,          // Produkt er solgt gennem auktion
        Withdrawn      // Produkt er trukket tilbage fra auktion
    }
}