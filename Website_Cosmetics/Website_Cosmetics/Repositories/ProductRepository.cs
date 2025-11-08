using Microsoft.EntityFrameworkCore;
using Website_Cosmetics.Models;
using Website_Cosmetics.Data;
using System;

namespace Website_Cosmetics.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===================================
        // PRODUCT CRUD OPERATIONS
        // ===================================

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                                 .Include(p => p.Brand)
                                 .Include(p => p.Category)
                                 .Include(p => p.ProductImages)
                                 .Include(p => p.ProductVariants)
                                     .ThenInclude(v => v.ProductVariantImages)
                                 .Where(p => p.IsActive == true)
                                 .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                                 .Include(p => p.Brand)
                                 .Include(p => p.Category)
                                 .Include(p => p.ProductImages)
                                 .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task<Product?> GetByIdWithVariantsAsync(int id)
        {
            return await _context.Products
                                 .Include(p => p.Brand)
                                 .Include(p => p.Category)
                                 .Include(p => p.ProductImages)
                                 .Include(p => p.ProductVariants.OrderBy(v => v.DisplayOrder))
                                     .ThenInclude(v => v.ProductVariantImages.OrderBy(i => i.DisplayOrder))
                                 .Include(p => p.ProductReviews.Where(r => r.IsActive == true))
                                     .ThenInclude(r => r.User)
                                 .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task AddAsync(Product product)
        {
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            product.UpdatedAt = DateTime.UtcNow;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // ===================================
        // PRODUCT QUERIES
        // ===================================

        public async Task<IEnumerable<Product>> GetMostLovedAsync(int count = 8)
        {
            // Step 1: Load product IDs first (fastest query)
            var productIds = await _context.Products
                                .Where(p => p.IsActive == true)
                                .OrderByDescending(p => p.LikeCount)
                                .ThenByDescending(p => p.Rating)
                                .Take(count)
                                .Select(p => p.ProductId)
                                .ToListAsync();

            if (!productIds.Any())
                return new List<Product>();

            // Step 2: Load products with basic data (no navigation properties)
            var products = await _context.Products
                                .AsNoTracking()
                                .Where(p => productIds.Contains(p.ProductId))
                                .ToListAsync();

            // Step 3: Load Brands and Categories separately
            var brandIds = products.Select(p => p.BrandId).Where(id => id.HasValue).Distinct().ToList();
            var categoryIds = products.Select(p => p.CategoryId).Where(id => id.HasValue).Distinct().ToList();

            var brands = brandIds.Any() 
                ? await _context.Brands.AsNoTracking().Where(b => brandIds.Contains(b.BrandId)).ToListAsync()
                : new List<Brand>();

            var categories = categoryIds.Any()
                ? await _context.Categories.AsNoTracking().Where(c => categoryIds.Contains(c.CategoryId)).ToListAsync()
                : new List<Category>();

            // Step 4: Attach Brands and Categories to products
            foreach (var product in products)
            {
                if (product.BrandId.HasValue)
                    product.Brand = brands.FirstOrDefault(b => b.BrandId == product.BrandId);
                if (product.CategoryId.HasValue)
                    product.Category = categories.FirstOrDefault(c => c.CategoryId == product.CategoryId);
            }

            // Step 5: Load cover images separately
            var coverImages = await _context.ProductImages
                                .AsNoTracking()
                                .Where(img => productIds.Contains(img.ProductId) && img.ImageType == "cover")
                                .ToListAsync();

            // Step 6: If no cover image, get first image
            var productsWithoutCover = productIds.Except(coverImages.Select(img => img.ProductId)).ToList();
            if (productsWithoutCover.Any())
            {
                var firstImages = await _context.ProductImages
                                    .AsNoTracking()
                                    .Where(img => productsWithoutCover.Contains(img.ProductId))
                                    .GroupBy(img => img.ProductId)
                                    .Select(g => g.OrderBy(img => img.DisplayOrder).First())
                                    .ToListAsync();
                coverImages.AddRange(firstImages);
            }

            // Step 7: Attach images to products
            foreach (var product in products)
            {
                var image = coverImages.FirstOrDefault(img => img.ProductId == product.ProductId);
                if (image != null)
                {
                    product.ProductImages = new List<ProductImage> { image };
                }
                else
                {
                    product.ProductImages = new List<ProductImage>();
                }
            }

            return products;
        }

        public async Task<IEnumerable<Product>> GetOnSaleAsync(int count = 8)
        {
            // Step 1: Find product IDs with sale variants
            var productIdsWithSale = await _context.ProductVariants
                                .AsNoTracking()
                                .Where(v => v.CompareAtPrice != null && v.CompareAtPrice > v.Price && v.IsActive == true)
                                .Select(v => v.ProductId)
                                .Distinct()
                                .ToListAsync();

            if (!productIdsWithSale.Any())
                return new List<Product>();

            // Step 2: Get product IDs ordered by UpdatedAt
            var productIds = await _context.Products
                                .AsNoTracking()
                                .Where(p => p.IsActive == true && productIdsWithSale.Contains(p.ProductId))
                                .OrderByDescending(p => p.UpdatedAt)
                                .Take(count)
                                .Select(p => p.ProductId)
                                .ToListAsync();

            if (!productIds.Any())
                return new List<Product>();

            // Step 3: Load products with basic data
            var products = await _context.Products
                                .AsNoTracking()
                                .Where(p => productIds.Contains(p.ProductId))
                                .ToListAsync();

            // Step 4: Load Brands and Categories separately
            var brandIds = products.Select(p => p.BrandId).Where(id => id.HasValue).Distinct().ToList();
            var categoryIds = products.Select(p => p.CategoryId).Where(id => id.HasValue).Distinct().ToList();

            var brands = brandIds.Any()
                ? await _context.Brands.AsNoTracking().Where(b => brandIds.Contains(b.BrandId)).ToListAsync()
                : new List<Brand>();

            var categories = categoryIds.Any()
                ? await _context.Categories.AsNoTracking().Where(c => categoryIds.Contains(c.CategoryId)).ToListAsync()
                : new List<Category>();

            // Step 5: Attach Brands and Categories to products
            foreach (var product in products)
            {
                if (product.BrandId.HasValue)
                    product.Brand = brands.FirstOrDefault(b => b.BrandId == product.BrandId);
                if (product.CategoryId.HasValue)
                    product.Category = categories.FirstOrDefault(c => c.CategoryId == product.CategoryId);
            }

            // Step 6: Load cover images separately
            var coverImages = await _context.ProductImages
                                .AsNoTracking()
                                .Where(img => productIds.Contains(img.ProductId) && img.ImageType == "cover")
                                .ToListAsync();

            // Step 7: If no cover image, get first image
            var productsWithoutCover = productIds.Except(coverImages.Select(img => img.ProductId)).ToList();
            if (productsWithoutCover.Any())
            {
                var firstImages = await _context.ProductImages
                                    .AsNoTracking()
                                    .Where(img => productsWithoutCover.Contains(img.ProductId))
                                    .GroupBy(img => img.ProductId)
                                    .Select(g => g.OrderBy(img => img.DisplayOrder).First())
                                    .ToListAsync();
                coverImages.AddRange(firstImages);
            }

            // Step 8: Attach images to products
            foreach (var product in products)
            {
                var image = coverImages.FirstOrDefault(img => img.ProductId == product.ProductId);
                if (image != null)
                {
                    product.ProductImages = new List<ProductImage> { image };
                }
                else
                {
                    product.ProductImages = new List<ProductImage>();
                }
            }

            return products;
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
        {
            return await _context.Products
                                .Include(p => p.Brand)
                                .Include(p => p.Category)
                                .Include(p => p.ProductImages)
                                .Include(p => p.ProductVariants)
                                .Where(p => p.IsActive == true && p.CategoryId == categoryId)
                                .OrderByDescending(p => p.CreatedAt)
                                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchAsync(string keyword)
        {
            keyword = keyword.ToLower();
            return await _context.Products
                                .Include(p => p.Brand)
                                .Include(p => p.Category)
                                .Include(p => p.ProductImages)
                                .Include(p => p.ProductVariants)
                                .Where(p => p.IsActive == true && 
                                           (p.Name.ToLower().Contains(keyword) ||
                                            (p.Description != null && p.Description.ToLower().Contains(keyword)) ||
                                            (p.Brand != null && p.Brand.Name.ToLower().Contains(keyword))))
                                .OrderByDescending(p => p.Rating)
                                .ToListAsync();
        }

        // ===================================
        // VARIANT OPERATIONS
        // ===================================

        public async Task<ProductVariant?> GetVariantByIdAsync(int variantId)
        {
            return await _context.ProductVariants
                                .Include(v => v.Product)
                                    .ThenInclude(p => p.Brand)
                                .Include(v => v.Product)
                                    .ThenInclude(p => p.Category)
                                .Include(v => v.ProductVariantImages.OrderBy(i => i.DisplayOrder))
                                .FirstOrDefaultAsync(v => v.VariantId == variantId);
        }

        public async Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(int productId)
        {
            return await _context.ProductVariants
                                .Include(v => v.ProductVariantImages.OrderBy(i => i.DisplayOrder))
                                .Where(v => v.ProductId == productId && v.IsActive == true)
                                .OrderBy(v => v.DisplayOrder)
                                .ToListAsync();
        }

        public async Task<ProductVariant?> GetDefaultVariantAsync(int productId)
        {
            var defaultVariant = await _context.ProductVariants
                                              .Include(v => v.ProductVariantImages.OrderBy(i => i.DisplayOrder))
                                              .FirstOrDefaultAsync(v => v.ProductId == productId && 
                                                                       v.IsDefault == true && 
                                                                       v.IsActive == true);

            // If no default variant, return the first active variant
            if (defaultVariant == null)
            {
                defaultVariant = await _context.ProductVariants
                                              .Include(v => v.ProductVariantImages.OrderBy(i => i.DisplayOrder))
                                              .Where(v => v.ProductId == productId && v.IsActive == true)
                                              .OrderBy(v => v.DisplayOrder)
                                              .FirstOrDefaultAsync();
            }

            return defaultVariant;
        }
    }
}