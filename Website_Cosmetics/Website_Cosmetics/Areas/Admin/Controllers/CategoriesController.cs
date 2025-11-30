using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Website_Cosmetics.Attributes;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;

namespace Website_Cosmetics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [RequirePermissionOrAdmin("Category.Manage")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ApplicationDbContext context, ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Admin/Categories/
        public async Task<IActionResult> Index(
            string? search = null,
            string? status = null,
            int page = 1,
            int pageSize = 10)
        {
            // Lấy tất cả categories
            var categoriesQuery = _context.Categories
                .Include(c => c.Products)
                .AsQueryable();

            // Filter theo search (Name, Description)
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                categoriesQuery = categoriesQuery.Where(c =>
                    c.Name.Contains(search) ||
                    (c.Description != null && c.Description.Contains(search))
                );
            }

            // Filter theo trạng thái (Active/Inactive)
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.ToLower() == "active")
                {
                    categoriesQuery = categoriesQuery.Where(c => c.IsActive == true);
                }
                else if (status.ToLower() == "inactive")
                {
                    categoriesQuery = categoriesQuery.Where(c => c.IsActive == false);
                }
            }

            // Lấy tổng số để phân trang
            var totalCount = await categoriesQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Validate page number
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // Phân trang
            var categories = await categoriesQuery
                .OrderByDescending(c => c.CreatedAt)
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

            return View(categories);
        }

        // GET: /Admin/Categories/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: /Admin/Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            try
            {
                // VALIDATION: Check name length
                if (!string.IsNullOrWhiteSpace(category.Name) && category.Name.Length > 100)
                {
                    ModelState.AddModelError(nameof(category.Name), "Category name cannot exceed 100 characters.");
                    return View(category);
                }

                if (ModelState.IsValid)
                {
                    // VALIDATION: Check if category name already exists (case-insensitive)
                    var trimmedName = category.Name?.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedName) && await _context.Categories.AnyAsync(c => c.Name != null && c.Name.Trim().ToLower() == trimmedName.ToLower()))
                    {
                        ModelState.AddModelError(nameof(category.Name), "Category name already exists. Please choose a different name.");
                        return View(category);
                    }

                    // Set default values
                    category.IsActive = category.IsActive ?? true;
                    category.CreatedAt = DateTime.UtcNow;
                    category.UpdatedAt = DateTime.UtcNow;

                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Category '{category.Name}' has been created successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                ModelState.AddModelError("", $"Error creating category: {ex.Message}");
            }

            return View(category);
        }

        // GET: /Admin/Categories/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: /Admin/Categories/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            try
            {
                // VALIDATION: Check name length
                if (!string.IsNullOrWhiteSpace(category.Name) && category.Name.Length > 100)
                {
                    ModelState.AddModelError(nameof(category.Name), "Category name cannot exceed 100 characters.");
                    return View(category);
                }

                if (ModelState.IsValid)
                {
                    // VALIDATION: Check if category name already exists (case-insensitive, different from current category)
                    var trimmedName = category.Name?.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedName) && await _context.Categories.AnyAsync(c => c.CategoryId != id && c.Name != null && c.Name.Trim().ToLower() == trimmedName.ToLower()))
                    {
                        ModelState.AddModelError(nameof(category.Name), "Category name already exists. Please choose a different name.");
                        return View(category);
                    }

                    var existingCategory = await _context.Categories.FindAsync(id);
                    if (existingCategory == null)
                    {
                        return NotFound();
                    }

                    // Update category properties
                    existingCategory.Name = category.Name ?? existingCategory.Name;
                    existingCategory.Description = category.Description;
                    existingCategory.IsActive = category.IsActive ?? true;
                    existingCategory.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Category '{existingCategory.Name}' has been updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CategoryExists(id))
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
                _logger.LogError(ex, "Error editing category {CategoryId}", id);
                ModelState.AddModelError("", $"Error updating category: {ex.Message}");
            }

            return View(category);
        }

        // GET: /Admin/Categories/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound();
            }

            // Kiểm tra xem có sản phẩm nào đang sử dụng danh mục này không
            var productCount = category.Products?.Count ?? 0;
            ViewBag.ProductCount = productCount;

            return View(category);
        }

        // POST: /Admin/Categories/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.CategoryId == id);

                if (category == null)
                {
                    return NotFound();
                }

                // VALIDATION: Check if there are any products using this category
                var productCount = category.Products?.Count ?? 0;
                if (productCount > 0)
                {
                    TempData["ErrorMessage"] = $"Cannot delete category '{category.Name}' because {productCount} product(s) are using this category. Please remove or reassign those products first.";
                    _logger.LogWarning("Attempted to delete category {CategoryId} ({CategoryName}) with {ProductCount} associated products", id, category.Name, productCount);
                    return RedirectToAction(nameof(Delete), new { id = id });
                }

                var categoryName = category.Name;

                // Delete category (according to database schema, Product.CategoryId has ON DELETE SET NULL, so no need to worry)
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Category '{categoryName}' has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", id);
                TempData["ErrorMessage"] = $"Error deleting category: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Helper method
        private async Task<bool> CategoryExists(int id)
        {
            return await _context.Categories.AnyAsync(e => e.CategoryId == id);
        }
    }
}

