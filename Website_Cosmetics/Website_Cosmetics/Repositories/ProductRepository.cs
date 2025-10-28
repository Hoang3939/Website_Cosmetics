using Microsoft.EntityFrameworkCore;
using Website_Cosmetics.Models;

namespace Website_Cosmetics.Repositories
{
    // 1. Class này hiện thực hóa Interface
    public class ProductRepository : IProductRepository
    {
        private readonly WebsiteCosmeticContext _context;

        public ProductRepository(WebsiteCosmeticContext context)
        {
            _context = context;
        }

        // Hiện thực hàm Lấy tất cả
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                                 .Include(p => p.Brand)
                                 .Include(p => p.Category)
                                 .Include(p => p.ProductImages)
                                 .Where(p => p.IsActive == true)
                                 .ToListAsync();
        }

        // Hiện thực hàm Lấy theo ID
        public async Task<Product> GetByIdAsync(Guid id)
        {
            return await _context.Products
                                 .Include(p => p.Brand)
                                 .Include(p => p.Category)
                                 .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        // Hiện thực hàm Thêm
        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        // Hiện thực hàm Cập nhật
        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        // Hiện thực hàm Xóa
        public async Task DeleteAsync(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        // Lấy các sản phẩm Most Loved (top rated)
        public async Task<IEnumerable<Product>> GetMostLovedAsync(int count = 8)
        {
            return await _context.Products
                                .Include(p => p.Brand)
                                .Include(p => p.Category)
                                .Include(p => p.ProductImages)
                                .Where(p => p.IsActive == true)
                                .OrderByDescending(p => p.CreatedAt)
                                .Take(count)
                                .ToListAsync();
        }

        // Lấy các sản phẩm đang sale (có thể filter theo price sau)
        public async Task<IEnumerable<Product>> GetOnSaleAsync(int count = 8)
        {
            return await _context.Products
                                .Include(p => p.Brand)
                                .Include(p => p.Category)
                                .Include(p => p.ProductImages)
                                .Where(p => p.IsActive == true)
                                .OrderByDescending(p => p.UpdatedAt)
                                .Take(count)
                                .ToListAsync();
        }
    }
}