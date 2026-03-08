using Website_Cosmetics.Models;

namespace Website_Cosmetics.Repositories
{
    public interface IProductRepository
    {
        // Product CRUD
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<Product?> GetByIdWithVariantsAsync(int id);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);

        // Product queries
        Task<IEnumerable<Product>> GetMostLovedAsync(int count = 8);
        Task<IEnumerable<Product>> GetOnSaleAsync(int count = 8);
        Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> SearchAsync(string keyword);

        // Variant operations
        Task<ProductVariant?> GetVariantByIdAsync(int variantId);
        Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(int productId);
        Task<ProductVariant?> GetDefaultVariantAsync(int productId);

        // Product count operations
        Task<int> GetTrendingCountAsync();
        Task<int> GetMakeupCountAsync();
        Task<int> GetToolsCountAsync();
    }
}