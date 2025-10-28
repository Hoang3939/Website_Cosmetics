using Website_Cosmetics.Models;

namespace Website_Cosmetics.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();

        Task<Product?> GetByIdAsync(Guid id);

        Task AddAsync(Product product);

        Task UpdateAsync(Product product);

        Task DeleteAsync(Guid id);

        Task<IEnumerable<Product>> GetMostLovedAsync(int count = 8);

        Task<IEnumerable<Product>> GetOnSaleAsync(int count = 8);
    }
}