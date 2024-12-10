using CatalogServiceAPI.Models;

namespace CatalogServiceAPI.Interfaces
{    
    public interface ICatalogInterface
    {
        Task<Catalog?> GetCatalog(Guid _id);
        Task<IEnumerable<Catalog>?> GetCatalogList();
        Task<Guid> AddCatalog(Catalog catalog);
        Task<long> UpdateCatalog(Catalog catalog);
        Task<long> DeleteCatalog(Guid _id);
    }
}