using Microsoft.EntityFrameworkCore;
using Website_Cosmetics.Models;
using Website_Cosmetics.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Website_Cosmetics.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

      
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                                .Where(c => c.IsActive == true)
                                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task AddAsync(Category category)
        {
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync(); 
        }

        public async Task UpdateAsync(Category category)
        {
            category.UpdatedAt = DateTime.UtcNow;
            _context.Categories.Update(category);
            await _context.SaveChangesAsync(); 
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                category.IsActive = false;
                category.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(); 
            }
        }
    }
}