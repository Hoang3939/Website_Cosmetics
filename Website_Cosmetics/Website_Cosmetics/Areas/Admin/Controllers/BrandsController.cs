using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;
using Website_Cosmetics.Attributes;

namespace Website_Cosmetics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [RequirePermissionOrAdmin("Brand.Manage")]
    public class BrandsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BrandsController> _logger;

        public BrandsController(ApplicationDbContext context, ILogger<BrandsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Admin/Brands/
        public async Task<IActionResult> Index(
            string? search = null,
            string? status = null,
            int page = 1,
            int pageSize = 10)
        {
            // Lấy tất cả brands
            var brandsQuery = _context.Brands
                .Include(b => b.Products)
                .AsQueryable();

            // Filter theo search (Name, Country)
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                brandsQuery = brandsQuery.Where(b =>
                    b.Name.Contains(search) ||
                    (b.Country != null && b.Country.Contains(search))
                );
            }

            // Filter theo trạng thái (Active/Inactive)
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.ToLower() == "active")
                {
                    brandsQuery = brandsQuery.Where(b => b.IsActive == true);
                }
                else if (status.ToLower() == "inactive")
                {
                    brandsQuery = brandsQuery.Where(b => b.IsActive == false);
                }
            }

            // Lấy tổng số để phân trang
            var totalCount = await brandsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Validate page number
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // Phân trang
            var brands = await brandsQuery
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền filter parameters vào ViewBag
            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

            return View(brands);
        }

        // GET: /Admin/Brands/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands
                .Include(b => b.Products)
                .FirstOrDefaultAsync(b => b.BrandId == id);

            if (brand == null)
            {
                return NotFound();
            }

            return View(brand);
        }

        // GET: /Admin/Brands/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Brands/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Brand brand)
        {
            try
            {
                // VALIDATION: Check name length
                if (!string.IsNullOrWhiteSpace(brand.Name) && brand.Name.Length > 100)
                {
                    ModelState.AddModelError(nameof(brand.Name), "Brand name cannot exceed 100 characters.");
                    return View(brand);
                }

                if (ModelState.IsValid)
                {
                    // VALIDATION: Check if brand name already exists (case-insensitive)
                    var trimmedName = brand.Name?.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedName) && await _context.Brands.AnyAsync(b => b.Name != null && b.Name.Trim().ToLower() == trimmedName.ToLower()))
                    {
                        ModelState.AddModelError(nameof(brand.Name), "Brand name already exists. Please choose a different name.");
                        return View(brand);
                    }

                    // Set default values
                    brand.IsActive = brand.IsActive ?? true;
                    brand.CreatedAt = DateTime.UtcNow;
                    brand.UpdatedAt = DateTime.UtcNow;

                    _context.Brands.Add(brand);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Brand '{brand.Name}' has been created successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating brand");
                ModelState.AddModelError("", $"Error creating brand: {ex.Message}");
            }

            return View(brand);
        }

        // GET: /Admin/Brands/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }

            return View(brand);
        }

        // POST: /Admin/Brands/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Brand brand)
        {
            if (id != brand.BrandId)
            {
                return NotFound();
            }

            try
            {
                // VALIDATION: Check name length
                if (!string.IsNullOrWhiteSpace(brand.Name) && brand.Name.Length > 100)
                {
                    ModelState.AddModelError(nameof(brand.Name), "Brand name cannot exceed 100 characters.");
                    return View(brand);
                }

                if (ModelState.IsValid)
                {
                    // VALIDATION: Check if brand name already exists (case-insensitive, different from current brand)
                    var trimmedName = brand.Name?.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedName) && await _context.Brands.AnyAsync(b => b.BrandId != id && b.Name != null && b.Name.Trim().ToLower() == trimmedName.ToLower()))
                    {
                        ModelState.AddModelError(nameof(brand.Name), "Brand name already exists. Please choose a different name.");
                        return View(brand);
                    }

                    var existingBrand = await _context.Brands.FindAsync(id);
                    if (existingBrand == null)
                    {
                        return NotFound();
                    }

                    // Update brand properties
                    existingBrand.Name = brand.Name ?? existingBrand.Name;
                    existingBrand.Country = brand.Country;
                    existingBrand.IsActive = brand.IsActive ?? true;
                    existingBrand.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Brand '{existingBrand.Name}' has been updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await BrandExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing brand {BrandId}", id);
                ModelState.AddModelError("", $"Error updating brand: {ex.Message}");
            }

            return View(brand);
        }

        // GET: /Admin/Brands/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands
                .Include(b => b.Products)
                .FirstOrDefaultAsync(b => b.BrandId == id);

            if (brand == null)
            {
                return NotFound();
            }

            // Check if any products are using this brand
            var productCount = brand.Products?.Count ?? 0;
            ViewBag.ProductCount = productCount;

            return View(brand);
        }

        // POST: /Admin/Brands/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var brand = await _context.Brands
                    .Include(b => b.Products)
                    .FirstOrDefaultAsync(b => b.BrandId == id);

                if (brand == null)
                {
                    return NotFound();
                }

                // VALIDATION: Check if any products are using this brand
                var productCount = brand.Products?.Count ?? 0;
                if (productCount > 0)
                {
                    TempData["ErrorMessage"] = $"Cannot delete brand '{brand.Name}' because {productCount} product(s) are using this brand. Please remove or reassign those products first.";
                    _logger.LogWarning("Attempted to delete brand {BrandId} ({BrandName}) with {ProductCount} associated products", id, brand.Name, productCount);
                    return RedirectToAction(nameof(Delete), new { id = id });
                }

                var brandName = brand.Name;

                // Delete brand (according to database schema, Product.BrandId has ON DELETE SET NULL, so no need to worry)
                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Brand '{brandName}' has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting brand {BrandId}", id);
                TempData["ErrorMessage"] = $"Error deleting brand: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Helper method
        private async Task<bool> BrandExists(int id)
        {
            return await _context.Brands.AnyAsync(e => e.BrandId == id);
        }
    }
}

