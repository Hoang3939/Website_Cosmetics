using Website_Cosmetics.Models;

namespace Website_Cosmetics.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();

        Task<Category?> GetByIdAsync(Guid id);

        Task AddAsync(Category category);

        Task UpdateAsync(Category category);

        Task DeleteAsync(Guid id);
    }
}