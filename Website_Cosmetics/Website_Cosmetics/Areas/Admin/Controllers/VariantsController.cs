using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;

namespace Website_Cosmetics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class VariantsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<VariantsController> _logger;

        public VariantsController(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<VariantsController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        // GET: Admin/Variants?productId=5
        public async Task<IActionResult> Index(int productId, string searchTerm, bool? isActive, bool? isDefault)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                return NotFound();
            }

            var query = _context.ProductVariants
                .Include(v => v.ProductVariantImages.OrderBy(img => img.DisplayOrder))
                .Where(v => v.ProductId == productId)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(v => 
                    v.VariantName.Contains(searchTerm) || 
                    (v.ColorName != null && v.ColorName.Contains(searchTerm)) ||
                    (v.SKU != null && v.SKU.Contains(searchTerm)) ||
                    (v.Barcode != null && v.Barcode.Contains(searchTerm)));
            }

            // Status filter
            if (isActive.HasValue)
            {
                query = query.Where(v => v.IsActive == isActive.Value);
            }

            // Default filter
            if (isDefault.HasValue)
            {
                query = query.Where(v => v.IsDefault == isDefault.Value);
            }

            var variants = await query
                .OrderBy(v => v.DisplayOrder)
                .ThenBy(v => v.VariantId)
                .ToListAsync();

            ViewBag.Product = product;
            ViewBag.ProductId = productId;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedIsActive = isActive;
            ViewBag.SelectedIsDefault = isDefault;
            ViewBag.HasActiveFilters = !string.IsNullOrWhiteSpace(searchTerm) || isActive.HasValue || isDefault.HasValue;

            return View(variants);
        }

        // GET: Admin/Variants/Create?productId=5
        public async Task<IActionResult> Create(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Product = product;
            ViewBag.ProductId = productId;

            return View(new ProductVariant());
        }

        // POST: Admin/Variants/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int productId,
            ProductVariant variant,
            List<IFormFile>? variantImages,
            List<int>? primaryImageIndices,
            List<int>? makeupReferenceIndices)
        {
            try
            {
                // Verify product exists
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    return NotFound();
                }

                // Remove fields not posted by the form to avoid ModelState invalidation
                ModelState.Remove(nameof(variant.Product));
                ModelState.Remove(nameof(variant.ProductVariantImages));
                ModelState.Remove(nameof(variant.CartItems));
                ModelState.Remove(nameof(variant.OrderItems));
                ModelState.Remove(nameof(variant.ProductId));
                ModelState.Remove(nameof(variant.CreatedAt));
                ModelState.Remove(nameof(variant.UpdatedAt));
                ModelState.Remove(nameof(variant.VariantId));

                // VALIDATION: Check if images are provided BEFORE saving variant
                if (variantImages == null || variantImages.Count == 0)
                {
                    ModelState.AddModelError("", "Variant must have at least 1 image. Please upload at least 1 image and mark it as primary.");
                }

                // VALIDATION: Check if variant with same color name or color code already exists for this product
                bool hasColorConflict = false;
                List<string> conflictMessages = new List<string>();

                if (!string.IsNullOrWhiteSpace(variant.ColorName))
                {
                    var existingVariantWithSameColorName = await _context.ProductVariants
                        .Where(v => v.ProductId == productId && 
                                    v.ColorName != null && 
                                    v.ColorName.ToLower().Trim() == variant.ColorName.ToLower().Trim())
                        .FirstOrDefaultAsync();

                    if (existingVariantWithSameColorName != null)
                    {
                        hasColorConflict = true;
                        conflictMessages.Add($"Color name '{variant.ColorName}' is already used by another variant in this product.");
                    }
                }

                if (!string.IsNullOrWhiteSpace(variant.ColorCode))
                {
                    var existingVariantWithSameColorCode = await _context.ProductVariants
                        .Where(v => v.ProductId == productId && 
                                    v.ColorCode != null && 
                                    v.ColorCode.ToUpper().Trim() == variant.ColorCode.ToUpper().Trim())
                        .FirstOrDefaultAsync();

                    if (existingVariantWithSameColorCode != null)
                    {
                        hasColorConflict = true;
                        conflictMessages.Add($"Color code '{variant.ColorCode}' is already used by another variant in this product.");
                    }
                }

                if (hasColorConflict)
                {
                    string finalMessage = "Color cannot be duplicated. " + string.Join(" ", conflictMessages) + " Please choose a different color name or color code.";
                    ModelState.AddModelError(nameof(variant.ColorName), finalMessage);
                }

                if (ModelState.IsValid)
                {
                    variant.ProductId = productId;
                    variant.CreatedAt = DateTime.Now;
                    variant.UpdatedAt = DateTime.Now;

                    // If this is the first variant, set it as default
                    var existingVariants = await _context.ProductVariants
                        .Where(v => v.ProductId == productId)
                        .CountAsync();
                    
                    if (existingVariants == 0)
                    {
                        variant.IsDefault = true;
                    }
                    
                    // If setting this variant as default, unset default flag from other variants
                    if (variant.IsDefault == true)
                    {
                        var otherDefaultVariants = await _context.ProductVariants
                            .Where(v => v.ProductId == productId && v.VariantId != variant.VariantId && v.IsDefault == true)
                            .ToListAsync();
                        
                        foreach (var otherVariant in otherDefaultVariants)
                        {
                            otherVariant.IsDefault = false;
                        }
                    }

                    _context.ProductVariants.Add(variant);
                    await _context.SaveChangesAsync();

                    // Upload images for the variant
                    // If no primary image is selected, set the first one as primary
                    if (primaryImageIndices == null || primaryImageIndices.Count == 0)
                    {
                        primaryImageIndices = new List<int> { 0 };
                    }
                    
                    try
                    {
                        await UploadVariantImages(variant.VariantId, variantImages!, primaryImageIndices, makeupReferenceIndices);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading variant images for variant {VariantId}", variant.VariantId);
                        ModelState.AddModelError("", $"Error uploading images: {ex.Message}");
                        
                        var productForError = await _context.Products
                            .Include(p => p.Brand)
                            .Include(p => p.Category)
                            .FirstOrDefaultAsync(p => p.ProductId == productId);
                        
                        ViewBag.Product = productForError;
                        ViewBag.ProductId = productId;
                        return View(variant);
                    }

                    // VALIDATION: Check if variant has at least 1 primary image after upload
                    var hasPrimaryImage = await _context.ProductVariantImages
                        .AnyAsync(img => img.VariantId == variant.VariantId && img.IsPrimary);
                    
                    if (!hasPrimaryImage)
                    {
                        // If no primary image, set the first image as primary
                        var firstImage = await _context.ProductVariantImages
                            .Where(img => img.VariantId == variant.VariantId)
                            .OrderBy(img => img.DisplayOrder)
                            .FirstOrDefaultAsync();
                        
                        if (firstImage != null)
                        {
                            firstImage.IsPrimary = true;
                            await _context.SaveChangesAsync();
                        }
                    }

                    TempData["SuccessMessage"] = $"Variant '{variant.VariantName}' has been created successfully.";
                    return RedirectToAction(nameof(Index), new { productId = productId });
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating variant: {ex.Message}");
            }

            var productForView = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (productForView == null)
            {
                return NotFound();
            }

            ViewBag.Product = productForView;
            ViewBag.ProductId = productId;
            return View(variant ?? new ProductVariant());
        }

        // GET: Admin/Variants/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                    .ThenInclude(p => p.Brand)
                .Include(v => v.Product)
                    .ThenInclude(p => p.Category)
                .Include(v => v.ProductVariantImages.OrderBy(img => img.DisplayOrder))
                .FirstOrDefaultAsync(v => v.VariantId == id);

            if (variant == null)
            {
                return NotFound();
            }

            ViewBag.Product = variant.Product;
            ViewBag.ProductId = variant.ProductId;

            return View(variant);
        }

        // POST: Admin/Variants/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ProductVariant variant,
            List<IFormFile>? newVariantImages,
            List<int>? primaryImageIndices,
            List<int>? makeupReferenceIndices,
            List<int>? existingImageIds,
            List<string>? existingIsPrimary,
            List<string>? existingIsMakeupReference,
            List<int>? existingDisplayOrder)
        {
            // Fallback: if id is 0, use variant.VariantId
            if (id == 0 && variant.VariantId > 0)
            {
                id = variant.VariantId;
            }

            if (id != variant.VariantId)
            {
                return NotFound();
            }

            // Remove fields not posted by the form to avoid ModelState invalidation
            ModelState.Remove(nameof(variant.Product));
            ModelState.Remove(nameof(variant.ProductVariantImages));
            ModelState.Remove(nameof(variant.CartItems));
            ModelState.Remove(nameof(variant.OrderItems));
            ModelState.Remove(nameof(variant.CreatedAt));
            ModelState.Remove(nameof(variant.UpdatedAt));

            // VALIDATION: Check if variant with same color name or color code already exists for this product (excluding current variant)
            bool hasColorConflict = false;
            List<string> conflictMessages = new List<string>();

            if (!string.IsNullOrWhiteSpace(variant.ColorName))
            {
                var existingVariantWithSameColorName = await _context.ProductVariants
                    .Where(v => v.ProductId == variant.ProductId && 
                                v.VariantId != id &&
                                v.ColorName != null && 
                                v.ColorName.ToLower().Trim() == variant.ColorName.ToLower().Trim())
                    .FirstOrDefaultAsync();

                if (existingVariantWithSameColorName != null)
                {
                    hasColorConflict = true;
                    conflictMessages.Add($"Color name '{variant.ColorName}' is already used by another variant in this product.");
                }
            }

            if (!string.IsNullOrWhiteSpace(variant.ColorCode))
            {
                var existingVariantWithSameColorCode = await _context.ProductVariants
                    .Where(v => v.ProductId == variant.ProductId && 
                                v.VariantId != id &&
                                v.ColorCode != null && 
                                v.ColorCode.ToUpper().Trim() == variant.ColorCode.ToUpper().Trim())
                    .FirstOrDefaultAsync();

                if (existingVariantWithSameColorCode != null)
                {
                    hasColorConflict = true;
                    conflictMessages.Add($"Color code '{variant.ColorCode}' is already used by another variant in this product.");
                }
            }

            if (hasColorConflict)
            {
                string finalMessage = "Color cannot be duplicated. " + string.Join(" ", conflictMessages) + " Please choose a different color name or color code.";
                ModelState.AddModelError(nameof(variant.ColorName), finalMessage);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingVariant = await _context.ProductVariants
                        .Include(v => v.ProductVariantImages)
                        .FirstOrDefaultAsync(v => v.VariantId == id);

                    if (existingVariant == null)
                    {
                        return NotFound();
                    }

                    // Update variant properties
                    existingVariant.VariantName = variant.VariantName;
                    existingVariant.ColorName = variant.ColorName;
                    existingVariant.ColorCode = variant.ColorCode;
                    existingVariant.ColorFamily = variant.ColorFamily;
                    existingVariant.Size = variant.Size;
                    existingVariant.SizeValue = variant.SizeValue;
                    existingVariant.SizeUnit = variant.SizeUnit;
                    existingVariant.VariantType = variant.VariantType;
                    existingVariant.SKU = variant.SKU;
                    existingVariant.Barcode = variant.Barcode;
                    existingVariant.Price = variant.Price;
                    existingVariant.CompareAtPrice = variant.CompareAtPrice;
                    existingVariant.Stock = variant.Stock;
                    existingVariant.LowStockThreshold = variant.LowStockThreshold;
                    existingVariant.IsActive = variant.IsActive;
                    
                    // If setting this variant as default, unset default flag from other variants
                    if (variant.IsDefault == true && existingVariant.IsDefault != true)
                    {
                        var otherDefaultVariants = await _context.ProductVariants
                            .Where(v => v.ProductId == existingVariant.ProductId && v.VariantId != id && v.IsDefault == true)
                            .ToListAsync();
                        
                        foreach (var otherVariant in otherDefaultVariants)
                        {
                            otherVariant.IsDefault = false;
                        }
                    }
                    
                    existingVariant.IsDefault = variant.IsDefault;
                    existingVariant.DisplayOrder = variant.DisplayOrder;
                    existingVariant.UpdatedAt = DateTime.Now;

                    // Update existing images
                    if (existingImageIds != null && existingDisplayOrder != null)
                    {
                        // First, reset all primary and makeup reference flags for this variant
                        var allVariantImages = await _context.ProductVariantImages
                            .Where(img => img.VariantId == id)
                            .ToListAsync();
                        foreach (var img in allVariantImages)
                        {
                            img.IsPrimary = false;
                            img.IsMakeupReference = false;
                        }

                        // Parse primary and makeup reference indices from form values
                        var primaryIndices = existingIsPrimary?.Select(s => int.Parse(s)).ToList() ?? new List<int>();
                        var makeupIndices = existingIsMakeupReference?.Select(s => int.Parse(s)).ToList() ?? new List<int>();

                        // Then set the selected ones
                        for (int i = 0; i < existingImageIds.Count; i++)
                        {
                            var imageId = existingImageIds[i];
                            var image = await _context.ProductVariantImages.FindAsync(imageId);
                            if (image != null)
                            {
                                // Check if this index is in the primary or makeup reference lists
                                image.IsPrimary = primaryIndices.Contains(i);
                                image.IsMakeupReference = makeupIndices.Contains(i);
                                image.DisplayOrder = i < existingDisplayOrder.Count ? existingDisplayOrder[i] : i;
                            }
                        }
                    }

                    // Upload new images
                    if (newVariantImages != null && newVariantImages.Count > 0)
                    {
                        // If no existing primary image, ensure at least one new image is primary
                        var hasPrimary = existingVariant.ProductVariantImages.Any(img => img.IsPrimary);
                        
                        if (!hasPrimary && (primaryImageIndices == null || primaryImageIndices.Count == 0))
                        {
                            primaryImageIndices = new List<int> { 0 }; // Set first image as primary
                        }
                        
                        await UploadVariantImages(id, newVariantImages, primaryImageIndices, makeupReferenceIndices);
                    }

                    // VALIDATION: Check if variant has at least 1 image after update
                    var totalImages = await _context.ProductVariantImages
                        .CountAsync(img => img.VariantId == id);
                    
                    if (totalImages == 0)
                    {
                        // If no images, show error
                        ModelState.AddModelError("", "Variant must have at least 1 image. Please upload at least 1 image and mark it as primary.");
                        
                        var variantForError = await _context.ProductVariants
                            .Include(v => v.Product)
                                .ThenInclude(p => p.Brand)
                            .Include(v => v.Product)
                                .ThenInclude(p => p.Category)
                            .Include(v => v.ProductVariantImages.OrderBy(img => img.DisplayOrder))
                            .FirstOrDefaultAsync(v => v.VariantId == id);
                        
                        ViewBag.Product = variantForError?.Product;
                        ViewBag.ProductId = variantForError?.ProductId;
                        return View(variant);
                    }

                    // VALIDATION: Check if variant has at least 1 primary image
                    var hasPrimaryImage = await _context.ProductVariantImages
                        .AnyAsync(img => img.VariantId == id && img.IsPrimary);
                    
                    if (!hasPrimaryImage)
                    {
                        // If no primary image, set the first image as primary
                        var firstImage = await _context.ProductVariantImages
                            .Where(img => img.VariantId == id)
                            .OrderBy(img => img.DisplayOrder)
                            .FirstOrDefaultAsync();
                        
                        if (firstImage != null)
                        {
                            firstImage.IsPrimary = true;
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Variant '{variant.VariantName}' has been updated successfully.";
                    return RedirectToAction(nameof(Index), new { productId = existingVariant.ProductId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VariantExists(variant.VariantId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            var variantForView = await _context.ProductVariants
                .Include(v => v.Product)
                    .ThenInclude(p => p.Brand)
                .Include(v => v.Product)
                    .ThenInclude(p => p.Category)
                .Include(v => v.ProductVariantImages.OrderBy(img => img.DisplayOrder))
                .FirstOrDefaultAsync(v => v.VariantId == id);

            ViewBag.Product = variantForView?.Product;
            ViewBag.ProductId = variantForView?.ProductId;
            return View(variant);
        }

        // GET: Admin/Variants/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .Include(v => v.ProductVariantImages)
                .FirstOrDefaultAsync(v => v.VariantId == id);

            if (variant == null)
            {
                return NotFound();
            }

            ViewBag.Product = variant.Product;
            return View(variant);
        }

        // POST: Admin/Variants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .Include(v => v.ProductVariantImages)
                .FirstOrDefaultAsync(v => v.VariantId == id);

            if (variant != null)
            {
                var productId = variant.ProductId;

                var variantName = variant.VariantName;

                // Delete image files
                foreach (var image in variant.ProductVariantImages)
                {
                    DeleteImageFile(image.Url);
                }

                // Delete variant folder after deleting all images
                DeleteVariantFolder(variant);

                _context.ProductVariants.Remove(variant);
                await _context.SaveChangesAsync();

                // Check if there are any remaining variants, if not, delete product folder
                var remainingVariants = await _context.ProductVariants
                    .Where(v => v.ProductId == productId)
                    .ToListAsync();

                // If the deleted variant was default, set the first remaining variant as default
                if (remainingVariants.Any() && !remainingVariants.Any(v => v.IsDefault))
                {
                    var firstVariant = remainingVariants.First();
                    firstVariant.IsDefault = true;
                    await _context.SaveChangesAsync();
                }
                else if (!remainingVariants.Any())
                {
                    // If no variants remain, delete product folder
                    DeleteProductFolder(variant.Product);
                }

                TempData["SuccessMessage"] = $"Variant '{variantName}' has been deleted successfully.";
                return RedirectToAction(nameof(Index), new { productId = productId });
            }

            TempData["ErrorMessage"] = "Variant not found.";
            return RedirectToAction(nameof(Index), new { productId = variant?.ProductId ?? 0 });
        }

        // POST: Admin/Variants/DeleteImage
        [HttpPost]
        public async Task<IActionResult> DeleteImage([FromBody] DeleteImageRequest request)
        {
            var image = await _context.ProductVariantImages.FindAsync(request.imageId);
            if (image == null)
            {
                return Json(new { success = false, message = "Image not found" });
            }

            var variantId = image.VariantId;
            var wasPrimary = image.IsPrimary;

            DeleteImageFile(image.Url);
            _context.ProductVariantImages.Remove(image);
            await _context.SaveChangesAsync();

            // VALIDATION: If deleted image was primary, set the first remaining image as primary
            if (wasPrimary)
            {
                var firstRemainingImage = await _context.ProductVariantImages
                    .Where(img => img.VariantId == variantId)
                    .OrderBy(img => img.DisplayOrder)
                    .FirstOrDefaultAsync();
                
                if (firstRemainingImage != null)
                {
                    firstRemainingImage.IsPrimary = true;
                    await _context.SaveChangesAsync();
                }
            }

            return Json(new { success = true });
        }

        // POST: Admin/Variants/BulkDelete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteRequest request)
        {
            if (request.VariantIds == null || request.VariantIds.Count == 0)
            {
                return Json(new { success = false, message = "Please select at least 1 variant to delete" });
            }

            var variants = await _context.ProductVariants
                .Include(v => v.Product)
                .Include(v => v.ProductVariantImages)
                .Where(v => request.VariantIds.Contains(v.VariantId))
                .ToListAsync();

            if (variants.Count == 0)
            {
                return Json(new { success = false, message = "No variants found to delete" });
            }

            var productId = variants.First().ProductId;
            
            // Get product information to delete folder
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            // Delete image files and delete variant folder
            foreach (var variant in variants)
            {
                foreach (var image in variant.ProductVariantImages)
                {
                    DeleteImageFile(image.Url);
                }
                
                // Delete variant folder
                DeleteVariantFolder(variant);
            }

            _context.ProductVariants.RemoveRange(variants);
            await _context.SaveChangesAsync();

            // If deleted variants include default, set the first remaining variant as default
            var remainingVariants = await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .ToListAsync();

            if (remainingVariants.Any() && !remainingVariants.Any(v => v.IsDefault))
            {
                var firstVariant = remainingVariants.First();
                firstVariant.IsDefault = true;
                await _context.SaveChangesAsync();
            }
            else if (!remainingVariants.Any() && product != null)
            {
                // If no variants remain, delete product folder
                DeleteProductFolder(product);
            }

            return Json(new { 
                success = true, 
                message = $"Successfully deleted {variants.Count} variant(s)",
                deletedCount = variants.Count
            });
        }

        // POST: Admin/Variants/UpdateImageOrder
        [HttpPost]
        public async Task<IActionResult> UpdateImageOrder([FromBody] UpdateImageOrderRequest request)
        {
            if (request.ImageOrders == null || request.ImageOrders.Count == 0)
            {
                return Json(new { success = false, message = "Invalid request" });
            }

            foreach (var imageOrder in request.ImageOrders)
            {
                var image = await _context.ProductVariantImages.FindAsync(imageOrder.ImageId);
                if (image != null)
                {
                    image.DisplayOrder = imageOrder.DisplayOrder;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        public class BulkDeleteRequest
        {
            public List<int> VariantIds { get; set; } = new();
        }

        public class UpdateImageOrderRequest
        {
            public List<ImageOrder> ImageOrders { get; set; } = new();
        }

        public class ImageOrder
        {
            public int ImageId { get; set; }
            public int DisplayOrder { get; set; }
        }

        public class DeleteImageRequest
        {
            public int imageId { get; set; }
        }

        private async Task UploadVariantImages(
            int variantId,
            List<IFormFile> images,
            List<int>? primaryImageIndices,
            List<int>? makeupReferenceIndices)
        {
            // Ensure wwwroot exists
            if (string.IsNullOrEmpty(_environment.WebRootPath))
            {
                throw new InvalidOperationException("WebRootPath is not set");
            }

            // Get variant and product information to create folder structure
            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.VariantId == variantId);

            if (variant == null)
            {
                throw new InvalidOperationException("Variant not found");
            }

            // Generate folder name for product (from product name)
            var productFolderName = GenerateSlug(variant.Product.Name);
            
            // Generate folder name for variant (from variant name or color)
            string variantFolderName;
            if (!string.IsNullOrEmpty(variant.ColorName))
            {
                // If color exists, use color name
                variantFolderName = GenerateSlug(variant.ColorName);
            }
            else if (!string.IsNullOrEmpty(variant.VariantName))
            {
                // If no color, use variant name
                variantFolderName = GenerateSlug(variant.VariantName);
            }
            else
            {
                // Fallback: use VariantId
                variantFolderName = $"variant-{variantId}";
            }

            // Create directory path: wwwroot/public/images/products/{ProductName}/{VariantName}/
            var uploadPath = Path.Combine(_environment.WebRootPath, "public", "images", "products", productFolderName, variantFolderName);
            
            // Create product folder if it doesn't exist
            var productFolderPath = Path.Combine(_environment.WebRootPath, "public", "images", "products", productFolderName);
            if (!Directory.Exists(productFolderPath))
            {
                Directory.CreateDirectory(productFolderPath);
            }
            
            // Create variant folder if it doesn't exist
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // If setting primary images, first unset all existing primary images for this variant
            if (primaryImageIndices != null && primaryImageIndices.Count > 0)
            {
                var existingImages = await _context.ProductVariantImages
                    .Where(img => img.VariantId == variantId)
                    .ToListAsync();
                foreach (var img in existingImages)
                {
                    img.IsPrimary = false;
                }
            }

            int displayOrder = await _context.ProductVariantImages
                .Where(img => img.VariantId == variantId)
                .CountAsync();

            // Allowed image extensions
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".avif" };
            
            for (int i = 0; i < images.Count; i++)
            {
                var file = images[i];
                if (file != null && file.Length > 0)
                {
                    // Generate unique filename
                    var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                    
                    // Validate file extension
                    if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                    {
                        throw new InvalidOperationException($"File type {extension} is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");
                    }
                    
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadPath, fileName);

                    // Check if file already exists (shouldn't happen with GUID, but check anyway)
                    if (System.IO.File.Exists(filePath))
                    {
                        // Generate new filename if file exists
                        fileName = $"{Guid.NewGuid()}{extension}";
                        filePath = Path.Combine(uploadPath, fileName);
                    }

                    // Ensure directory still exists before writing
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    // Write file
                    using (var stream = new FileStream(filePath, FileMode.CreateNew))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // URL path: /public/images/products/{ProductName}/{VariantName}/{fileName}
                    var imageUrl = $"/public/images/products/{productFolderName}/{variantFolderName}/{fileName}";
                    var isPrimary = primaryImageIndices != null && primaryImageIndices.Contains(i);
                    var isMakeupReference = makeupReferenceIndices != null && makeupReferenceIndices.Contains(i);

                    var variantImage = new ProductVariantImage
                    {
                        VariantId = variantId,
                        Url = imageUrl,
                        IsPrimary = isPrimary,
                        IsMakeupReference = isMakeupReference,
                        DisplayOrder = displayOrder + i,
                        CreatedAt = DateTime.Now
                    };

                    _context.ProductVariantImages.Add(variantImage);
                }
            }

            await _context.SaveChangesAsync();
        }

        private void DeleteImageFile(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return;

            try
            {
                var filePath = imageUrl.StartsWith("/") 
                    ? Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'))
                    : Path.Combine(_environment.WebRootPath, imageUrl);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch
            {
                // Ignore errors when deleting files
            }
        }

        private bool VariantExists(int id)
        {
            return _context.ProductVariants.Any(e => e.VariantId == id);
        }

        /// <summary>
        /// Delete variant folder when deleting variant
        /// </summary>
        private void DeleteVariantFolder(ProductVariant variant)
        {
            if (string.IsNullOrEmpty(_environment.WebRootPath) || variant.Product == null)
            {
                return;
            }

            try
            {
                // Generate folder name for product
                var productFolderName = GenerateSlug(variant.Product.Name);
                
                // Generate folder name for variant
                string variantFolderName;
                if (!string.IsNullOrEmpty(variant.ColorName))
                {
                    variantFolderName = GenerateSlug(variant.ColorName);
                }
                else if (!string.IsNullOrEmpty(variant.VariantName))
                {
                    variantFolderName = GenerateSlug(variant.VariantName);
                }
                else
                {
                    variantFolderName = $"variant-{variant.VariantId}";
                }

                // Path: wwwroot/public/images/products/{ProductName}/{VariantName}/
                var variantFolderPath = Path.Combine(_environment.WebRootPath, "public", "images", "products", productFolderName, variantFolderName);
                
                // Delete variant folder if it exists
                if (Directory.Exists(variantFolderPath))
                {
                    Directory.Delete(variantFolderPath, true); // true = delete subfolders and files inside
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw to avoid affecting variant deletion
                Console.WriteLine($"Error deleting variant folder: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete product folder when no variants remain
        /// </summary>
        private void DeleteProductFolder(Product product)
        {
            if (string.IsNullOrEmpty(_environment.WebRootPath) || product == null)
            {
                return;
            }

            try
            {
                // Generate folder name for product
                var productFolderName = GenerateSlug(product.Name);
                
                // Path: wwwroot/public/images/products/{ProductName}/
                var productFolderPath = Path.Combine(_environment.WebRootPath, "public", "images", "products", productFolderName);
                
                // Delete product folder if it exists and is empty
                if (Directory.Exists(productFolderPath))
                {
                    // Check if folder is empty
                    var hasFiles = Directory.GetFiles(productFolderPath, "*", SearchOption.AllDirectories).Length > 0;
                    var hasSubFolders = Directory.GetDirectories(productFolderPath).Length > 0;
                    
                    // If folder is empty or has no subfolders, delete it
                    if (!hasFiles && !hasSubFolders)
                    {
                        Directory.Delete(productFolderPath, true);
                    }
                    // If subfolders exist but are empty, delete empty subfolders
                    else if (!hasFiles)
                    {
                        var subFolders = Directory.GetDirectories(productFolderPath);
                        foreach (var subFolder in subFolders)
                        {
                            try
                            {
                                if (Directory.GetFiles(subFolder, "*", SearchOption.AllDirectories).Length == 0)
                                {
                                    Directory.Delete(subFolder, true);
                                }
                            }
                            catch
                            {
                                // Ignore errors when deleting subfolders
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw
                Console.WriteLine($"Error deleting product folder: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate slug from text (used to create folder names)
        /// </summary>
        private string GenerateSlug(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Convert to lowercase
            text = text.ToLower();

            // Replace Vietnamese characters
            text = text.Replace("à", "a").Replace("á", "a").Replace("ạ", "a").Replace("ả", "a").Replace("ã", "a");
            text = text.Replace("â", "a").Replace("ầ", "a").Replace("ấ", "a").Replace("ậ", "a").Replace("ẩ", "a").Replace("ẫ", "a");
            text = text.Replace("ă", "a").Replace("ằ", "a").Replace("ắ", "a").Replace("ặ", "a").Replace("ẳ", "a").Replace("ẵ", "a");
            text = text.Replace("è", "e").Replace("é", "e").Replace("ẹ", "e").Replace("ẻ", "e").Replace("ẽ", "e");
            text = text.Replace("ê", "e").Replace("ề", "e").Replace("ế", "e").Replace("ệ", "e").Replace("ể", "e").Replace("ễ", "e");
            text = text.Replace("ì", "i").Replace("í", "i").Replace("ị", "i").Replace("ỉ", "i").Replace("ĩ", "i");
            text = text.Replace("ò", "o").Replace("ó", "o").Replace("ọ", "o").Replace("ỏ", "o").Replace("õ", "o");
            text = text.Replace("ô", "o").Replace("ồ", "o").Replace("ố", "o").Replace("ộ", "o").Replace("ổ", "o").Replace("ỗ", "o");
            text = text.Replace("ơ", "o").Replace("ờ", "o").Replace("ớ", "o").Replace("ợ", "o").Replace("ở", "o").Replace("ỡ", "o");
            text = text.Replace("ù", "u").Replace("ú", "u").Replace("ụ", "u").Replace("ủ", "u").Replace("ũ", "u");
            text = text.Replace("ư", "u").Replace("ừ", "u").Replace("ứ", "u").Replace("ự", "u").Replace("ử", "u").Replace("ữ", "u");
            text = text.Replace("ỳ", "y").Replace("ý", "y").Replace("ỵ", "y").Replace("ỷ", "y").Replace("ỹ", "y");
            text = text.Replace("đ", "d");

            // Remove special characters (keep letters, numbers, hyphens and spaces)
            text = System.Text.RegularExpressions.Regex.Replace(text, @"[^a-z0-9\s-]", "");

            // Replace spaces with hyphens
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", "-");

            // Remove multiple hyphens
            text = System.Text.RegularExpressions.Regex.Replace(text, @"-+", "-");

            // Trim hyphens from start and end
            text = text.Trim('-');

            return text;
        }
    }
}

