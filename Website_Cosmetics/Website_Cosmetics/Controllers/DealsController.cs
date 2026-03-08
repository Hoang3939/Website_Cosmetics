using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;

namespace Website_Cosmetics.Controllers
{
    public class DealsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DealsController> _logger;

        public DealsController(ApplicationDbContext context, ILogger<DealsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Deals
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 12;
            
            // Get products on sale (products with variants that have discount or lower price than base price)
            var allProducts = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.ProductVariantImages)
                .Where(p => p.IsActive == true)
                .ToListAsync();

            // Filter products that have variants with sale prices (CompareAtPrice > Price or Price < BasePrice)
            var saleProducts = allProducts
                .Where(p => 
                {
                    var defaultVariant = p.ProductVariants?.FirstOrDefault(v => v.IsDefault == true && (v.IsActive == true || v.IsActive == null))
                                     ?? p.ProductVariants?.FirstOrDefault(v => v.IsActive == true || v.IsActive == null)
                                     ?? p.ProductVariants?.FirstOrDefault();
                    if (defaultVariant == null) return false;
                    var currentPrice = defaultVariant.Price ?? p.BasePrice;
                    var comparePrice = defaultVariant.CompareAtPrice ?? p.BasePrice;
                    return comparePrice > currentPrice || (defaultVariant.Price.HasValue && defaultVariant.Price < p.BasePrice);
                })
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            var totalProducts = saleProducts.Count;
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var products = saleProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.PageTitle = "Sale & Offers";
            ViewBag.PageDescription = "Special deals and discounted products";

            return View(products);
        }
    }
}

