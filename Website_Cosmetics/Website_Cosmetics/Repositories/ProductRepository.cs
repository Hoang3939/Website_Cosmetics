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

            // Step 8: Load ProductVariants with images for stock calculation and image display
            var variants = await _context.ProductVariants
                                .AsNoTracking()
                                .Include(v => v.ProductVariantImages.OrderBy(i => i.DisplayOrder))
                                .Where(v => productIds.Contains(v.ProductId) && (v.IsActive == true || v.IsActive == null))
                                .ToListAsync();

            // Attach variants to products
            foreach (var product in products)
            {
                product.ProductVariants = variants.Where(v => v.ProductId == product.ProductId).ToList();
            }

            return products;
        }

        public async Task<IEnumerable<Product>> GetOnSaleAsync(int count = 8)
        {
            // Step 1: Load all active products with their variants and base prices
            var allProducts = await _context.Products
                                .AsNoTracking()
                                .Where(p => p.IsActive == true)
                                .ToListAsync();

            if (!allProducts.Any())
                return new List<Product>();

            // Step 2: Load all active variants
            var allVariants = await _context.ProductVariants
                                .AsNoTracking()
                                .Where(v => v.IsActive == true || v.IsActive == null)
                                .ToListAsync();

            // Step 3: Filter products that have default variants with discount >= 40%
            var productsWith40PercentDiscount = new List<Product>();
            
            foreach (var product in allProducts)
            {
                // Get default variant
                var defaultVariant = allVariants
                    .Where(v => v.ProductId == product.ProductId)
                    .OrderByDescending(v => v.IsDefault == true)
                    .ThenByDescending(v => v.IsActive == true || v.IsActive == null)
                    .ThenBy(v => v.DisplayOrder)
                    .FirstOrDefault();

                if (defaultVariant == null) continue;

                // Calculate current price
                var currentPrice = defaultVariant.Price ?? product.BasePrice;
                
                // Calculate compare price (original price)
                // If variant has CompareAtPrice, use it; otherwise use BasePrice
                var comparePrice = defaultVariant.CompareAtPrice ?? product.BasePrice;
                
                // Calculate discount percentage
                // Check if there's a discount: CompareAtPrice > currentPrice OR (variant.Price < BasePrice)
                if (comparePrice > currentPrice && comparePrice > 0)
                {
                    var discountPercent = (1 - (currentPrice / comparePrice)) * 100;
                    
                    // Only include products with discount >= 40%
                    if (discountPercent >= 40)
                    {
                        productsWith40PercentDiscount.Add(product);
                    }
                }
                // Also check if variant.Price < BasePrice (even without CompareAtPrice)
                else if (defaultVariant.Price != null && defaultVariant.Price < product.BasePrice && product.BasePrice > 0)
                {
                    var discountPercent = (1 - (defaultVariant.Price.Value / product.BasePrice)) * 100;
                    
                    // Only include products with discount >= 40%
                    if (discountPercent >= 40)
                    {
                        productsWith40PercentDiscount.Add(product);
                    }
                }
            }

            if (!productsWith40PercentDiscount.Any())
                return new List<Product>();

            // Step 4: Get product IDs ordered by UpdatedAt, limited to count
            var productIds = productsWith40PercentDiscount
                                .OrderByDescending(p => p.UpdatedAt)
                                .Take(count)
                                .Select(p => p.ProductId)
                                .ToList();

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

            // Step 9: Load ProductVariants with images for stock calculation and image display
            var variants = await _context.ProductVariants
                                .AsNoTracking()
                                .Include(v => v.ProductVariantImages.OrderBy(i => i.DisplayOrder))
                                .Where(v => productIds.Contains(v.ProductId) && (v.IsActive == true || v.IsActive == null))
                                .ToListAsync();

            // Attach variants to products
            foreach (var product in products)
            {
                product.ProductVariants = variants.Where(v => v.ProductId == product.ProductId).ToList();
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
                                    .ThenInclude(v => v.ProductVariantImages)
                                .Where(p => p.IsActive == true && p.CategoryId == categoryId)
                                .OrderByDescending(p => p.CreatedAt)
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
            // IsActive is bool? (nullable), so check for true or null (treat null as active)
            // If multiple variants have IsDefault = true, prioritize the one with stock > 0
            var defaultVariants = await _context.ProductVariants
                                              .Include(v => v.ProductVariantImages.OrderBy(i => i.DisplayOrder))
                                              .Where(v => v.ProductId == productId && 
                                                                       v.IsDefault == true && 
                                                         (v.IsActive == true || v.IsActive == null))
                                              .ToListAsync();

            ProductVariant? defaultVariant = null;
            
            // If multiple default variants, prioritize the one with stock > 0
            if (defaultVariants.Any())
            {
                defaultVariant = defaultVariants
                    .OrderByDescending(v => v.Stock > 0) // Variants with stock > 0 first
                    .ThenBy(v => v.DisplayOrder)
                    .FirstOrDefault();
            }

            // If no default variant, return the first active variant with stock > 0 (if available)
            if (defaultVariant == null)
            {
                var activeVariants = await _context.ProductVariants
                                              .Include(v => v.ProductVariantImages.OrderBy(i => i.DisplayOrder))
                                                  .Where(v => v.ProductId == productId && (v.IsActive == true || v.IsActive == null))
                                                  .ToListAsync();
                
                // Prioritize variants with stock > 0
                defaultVariant = activeVariants
                    .OrderByDescending(v => v.Stock > 0) // Variants with stock > 0 first
                    .ThenBy(v => v.DisplayOrder)
                    .FirstOrDefault();
            }

            return defaultVariant;
        }

        // ===================================
        // PRODUCT COUNT OPERATIONS
        // ===================================

        public async Task<int> GetTrendingCountAsync()
        {
            // Count all active products (trending = all products)
            return await _context.Products
                .Where(p => p.IsActive == true)
                .CountAsync();
        }

        public async Task<int> GetMakeupCountAsync()
        {
            // Get Makeup category
            var makeupCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower().Contains("makeup") || c.Name.ToLower() == "makeup");

            if (makeupCategory != null)
            {
                return await _context.Products
                    .Where(p => p.IsActive == true && p.CategoryId == makeupCategory.CategoryId)
                    .CountAsync();
            }
            else
            {
                // Fallback: count by name containing "makeup"
                return await _context.Products
                    .Where(p => p.IsActive == true && 
                           (p.Name.ToLower().Contains("makeup") || 
                            p.Category != null && p.Category.Name.ToLower().Contains("makeup")))
                    .CountAsync();
            }
        }

        public async Task<int> GetToolsCountAsync()
        {
            // Get Tools/Brushes category
            var toolsCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower().Contains("tool") || 
                                         c.Name.ToLower().Contains("brush") ||
                                         c.Name.ToLower() == "tools");

            if (toolsCategory != null)
            {
                return await _context.Products
                    .Where(p => p.IsActive == true && p.CategoryId == toolsCategory.CategoryId)
                    .CountAsync();
            }
            else
            {
                // Fallback: count by name containing "tool" or "brush"
                return await _context.Products
                    .Where(p => p.IsActive == true && 
                           (p.Name.ToLower().Contains("tool") || 
                            p.Name.ToLower().Contains("brush") ||
                            p.Category != null && (p.Category.Name.ToLower().Contains("tool") || 
                                                   p.Category.Name.ToLower().Contains("brush"))))
                    .CountAsync();
            }
        }

        public async Task<IEnumerable<Product>> SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<Product>();

            var searchTerm = keyword.ToLower().Trim();

            // Get brand and category IDs that match the search term
            var matchingBrandIds = await _context.Brands
                .AsNoTracking()
                .Where(b => b.Name.ToLower().Contains(searchTerm))
                .Select(b => b.BrandId)
                .ToListAsync();

            var matchingCategoryIds = await _context.Categories
                .AsNoTracking()
                .Where(c => c.Name.ToLower().Contains(searchTerm))
                .Select(c => c.CategoryId)
                .ToListAsync();

            // Search in product name, brand name, and category name
            var productIds = await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive == true && (
                    p.Name.ToLower().Contains(searchTerm) ||
                    (p.BrandId.HasValue && matchingBrandIds.Contains(p.BrandId.Value)) ||
                    (p.CategoryId.HasValue && matchingCategoryIds.Contains(p.CategoryId.Value))
                ))
                .OrderByDescending(p => p.UpdatedAt)
                .Take(10) // Limit to 10 results for autocomplete
                .Select(p => p.ProductId)
                .ToListAsync();

            if (!productIds.Any())
                return new List<Product>();

            // Load products with basic data
            var products = await _context.Products
                .AsNoTracking()
                .Where(p => productIds.Contains(p.ProductId))
                .ToListAsync();

            // Load Brands and Categories separately
            var brandIds = products.Select(p => p.BrandId).Where(id => id.HasValue).Distinct().ToList();
            var categoryIds = products.Select(p => p.CategoryId).Where(id => id.HasValue).Distinct().ToList();

            var brands = brandIds.Any()
                ? await _context.Brands.AsNoTracking().Where(b => brandIds.Contains(b.BrandId)).ToListAsync()
                : new List<Brand>();

            var categories = categoryIds.Any()
                ? await _context.Categories.AsNoTracking().Where(c => categoryIds.Contains(c.CategoryId)).ToListAsync()
                : new List<Category>();

            // Attach Brands and Categories to products
            foreach (var product in products)
            {
                if (product.BrandId.HasValue)
                    product.Brand = brands.FirstOrDefault(b => b.BrandId == product.BrandId);
                if (product.CategoryId.HasValue)
                    product.Category = categories.FirstOrDefault(c => c.CategoryId == product.CategoryId);
            }

            // Load variant images for display
            var variantImages = await _context.ProductVariants
                .AsNoTracking()
                .Include(v => v.ProductVariantImages.OrderBy(i => i.DisplayOrder))
                .Where(v => productIds.Contains(v.ProductId) && (v.IsActive == true || v.IsActive == null))
                .OrderByDescending(v => v.IsDefault == true)
                .ThenBy(v => v.DisplayOrder)
                .ToListAsync();

            // Attach variants to products
            foreach (var product in products)
            {
                product.ProductVariants = variantImages
                    .Where(v => v.ProductId == product.ProductId)
                    .ToList();
            }

            // Maintain order
            return products.OrderBy(p => productIds.IndexOf(p.ProductId));
        }
    }
}