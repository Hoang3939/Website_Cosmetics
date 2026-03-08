using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;
using Website_Cosmetics.Repositories;

namespace Website_Cosmetics.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(
            ApplicationDbContext context,
            IProductRepository productRepository,
            ILogger<CatalogController> logger)
        {
            _context = context;
            _productRepository = productRepository;
            _logger = logger;
        }

        // GET: /Catalog/Trending
        public async Task<IActionResult> Trending(int page = 1)
        {
            const int pageSize = 12;
            
            // Get trending products (newest products, sorted by CreatedAt desc)
            var allProducts = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.ProductVariantImages)
                .Where(p => p.IsActive == true)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var totalProducts = allProducts.Count;
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var products = allProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.PageTitle = "New & Trending";
            ViewBag.PageDescription = "Discover the latest beauty products and trending items";

            return View("Catalog", products);
        }

        // GET: /Catalog/Makeup
        public async Task<IActionResult> Makeup(int page = 1)
        {
            const int pageSize = 12;
            
            // Get Makeup category
            var makeupCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower().Contains("makeup") || c.Name.ToLower() == "makeup");

            IEnumerable<Product> allProducts;

            if (makeupCategory != null)
            {
                allProducts = await _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductVariants)
                        .ThenInclude(v => v.ProductVariantImages)
                    .Where(p => p.IsActive == true && p.CategoryId == makeupCategory.CategoryId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            else
            {
                // Fallback: search by name containing "makeup"
                allProducts = await _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductVariants)
                        .ThenInclude(v => v.ProductVariantImages)
                    .Where(p => p.IsActive == true && 
                           (p.Name.ToLower().Contains("makeup") || 
                            p.Category != null && p.Category.Name.ToLower().Contains("makeup")))
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }

            var totalProducts = allProducts.Count();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var products = allProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.PageTitle = "Makeup";
            ViewBag.PageDescription = "Explore our collection of makeup products";

            return View("Catalog", products);
        }

        // GET: /Catalog/Tools
        public async Task<IActionResult> Tools(int page = 1)
        {
            const int pageSize = 12;
            
            // Get Tools/Brushes category
            var toolsCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower().Contains("tool") || 
                                         c.Name.ToLower().Contains("brush") ||
                                         c.Name.ToLower() == "tools");

            IEnumerable<Product> allProducts;

            if (toolsCategory != null)
            {
                allProducts = await _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductVariants)
                        .ThenInclude(v => v.ProductVariantImages)
                    .Where(p => p.IsActive == true && p.CategoryId == toolsCategory.CategoryId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            else
            {
                // Fallback: search by name containing "tool" or "brush"
                allProducts = await _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductVariants)
                        .ThenInclude(v => v.ProductVariantImages)
                    .Where(p => p.IsActive == true && 
                           (p.Name.ToLower().Contains("tool") || 
                            p.Name.ToLower().Contains("brush") ||
                            p.Category != null && (p.Category.Name.ToLower().Contains("tool") || 
                                                   p.Category.Name.ToLower().Contains("brush"))))
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }

            var totalProducts = allProducts.Count();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var products = allProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.PageTitle = "Tools & Brushes";
            ViewBag.PageDescription = "Professional beauty tools and brushes";

            return View("Catalog", products);
        }

        // GET: /Catalog/Gifts
        public IActionResult Gifts()
        {
            ViewBag.PageTitle = "Gift & Value Sets";
            ViewBag.PageDescription = "Special gift sets and value bundles";
            ViewBag.CurrentPage = 1;
            ViewBag.TotalPages = 0;
            ViewBag.TotalProducts = 0;

            // Return empty list to show "No products found" message
            return View("Catalog", new List<Product>());
        }
    }
}

