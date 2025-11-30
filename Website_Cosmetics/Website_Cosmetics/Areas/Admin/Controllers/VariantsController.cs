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

        public VariantsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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

            // Hiển thị thông báo nếu có
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

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

            return View();
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

                if (ModelState.IsValid)
                {
                    variant.ProductId = productId;
                    variant.CreatedAt = DateTime.Now;
                    variant.UpdatedAt = DateTime.Now;

                    // Nếu đây là variant đầu tiên, set làm default
                    var existingVariants = await _context.ProductVariants
                        .Where(v => v.ProductId == productId)
                        .CountAsync();
                    
                    if (existingVariants == 0)
                    {
                        variant.IsDefault = true;
                    }

                    _context.ProductVariants.Add(variant);
                    await _context.SaveChangesAsync();

                    // Upload images for the variant
                    if (variantImages != null && variantImages.Count > 0)
                    {
                        // If no primary image is selected, set the first one as primary
                        if (primaryImageIndices == null || primaryImageIndices.Count == 0)
                        {
                            primaryImageIndices = new List<int> { 0 };
                        }
                        await UploadVariantImages(variant.VariantId, variantImages, primaryImageIndices, makeupReferenceIndices);
                    }
                    else
                    {
                        // VALIDATION: Variant phải có ít nhất 1 ảnh primary
                        ModelState.AddModelError("", "Variant phải có ít nhất 1 ảnh. Vui lòng upload ít nhất 1 ảnh và đánh dấu làm ảnh chính.");
                        
                        var productForError = await _context.Products
                            .Include(p => p.Brand)
                            .Include(p => p.Category)
                            .FirstOrDefaultAsync(p => p.ProductId == productId);
                        
                        ViewBag.Product = productForError;
                        ViewBag.ProductId = productId;
                        return View(variant);
                    }

                    // VALIDATION: Kiểm tra variant có ít nhất 1 ảnh primary sau khi upload
                    var hasPrimaryImage = await _context.ProductVariantImages
                        .AnyAsync(img => img.VariantId == variant.VariantId && img.IsPrimary);
                    
                    if (!hasPrimaryImage)
                    {
                        // Nếu không có primary image, set ảnh đầu tiên làm primary
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

                    TempData["SuccessMessage"] = $"Variant '{variant.VariantName}' đã được tạo thành công.";
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

            ViewBag.Product = productForView;
            ViewBag.ProductId = productId;
            return View(variant);
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
            if (id != variant.VariantId)
            {
                return NotFound();
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

                    // VALIDATION: Kiểm tra variant có ít nhất 1 ảnh sau khi cập nhật
                    var totalImages = await _context.ProductVariantImages
                        .CountAsync(img => img.VariantId == id);
                    
                    if (totalImages == 0)
                    {
                        // Nếu không có ảnh nào, báo lỗi
                        ModelState.AddModelError("", "Variant phải có ít nhất 1 ảnh. Vui lòng upload ít nhất 1 ảnh và đánh dấu làm ảnh chính.");
                        
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

                    // VALIDATION: Kiểm tra variant có ít nhất 1 ảnh primary
                    var hasPrimaryImage = await _context.ProductVariantImages
                        .AnyAsync(img => img.VariantId == id && img.IsPrimary);
                    
                    if (!hasPrimaryImage)
                    {
                        // Nếu không có primary image, set ảnh đầu tiên làm primary
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
                    TempData["SuccessMessage"] = $"Variant '{variant.VariantName}' đã được cập nhật thành công.";
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

                // Delete image files
                foreach (var image in variant.ProductVariantImages)
                {
                    DeleteImageFile(image.Url);
                }

                // Xóa folder variant sau khi xóa tất cả ảnh
                DeleteVariantFolder(variant);

                _context.ProductVariants.Remove(variant);
                await _context.SaveChangesAsync();

                // Kiểm tra xem còn variant nào không, nếu không còn thì xóa folder sản phẩm
                var remainingVariants = await _context.ProductVariants
                    .Where(v => v.ProductId == productId)
                    .ToListAsync();

                // Nếu variant bị xóa là default, set variant đầu tiên làm default
                if (remainingVariants.Any() && !remainingVariants.Any(v => v.IsDefault))
                {
                    var firstVariant = remainingVariants.First();
                    firstVariant.IsDefault = true;
                    await _context.SaveChangesAsync();
                }
                else if (!remainingVariants.Any())
                {
                    // Nếu không còn variant nào, xóa folder sản phẩm
                    DeleteProductFolder(variant.Product);
                }

                TempData["SuccessMessage"] = $"Variant '{variant.VariantName}' đã được xóa thành công.";
                return RedirectToAction(nameof(Index), new { productId = productId });
            }

            return NotFound();
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

            // VALIDATION: Nếu ảnh bị xóa là primary, set ảnh đầu tiên còn lại làm primary
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
                return Json(new { success = false, message = "Vui lòng chọn ít nhất 1 variant để xóa" });
            }

            var variants = await _context.ProductVariants
                .Include(v => v.Product)
                .Include(v => v.ProductVariantImages)
                .Where(v => request.VariantIds.Contains(v.VariantId))
                .ToListAsync();

            if (variants.Count == 0)
            {
                return Json(new { success = false, message = "Không tìm thấy variants để xóa" });
            }

            var productId = variants.First().ProductId;
            
            // Lấy thông tin product để xóa folder
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            // Delete image files và xóa folder variant
            foreach (var variant in variants)
            {
                foreach (var image in variant.ProductVariantImages)
                {
                    DeleteImageFile(image.Url);
                }
                
                // Xóa folder variant
                DeleteVariantFolder(variant);
            }

            _context.ProductVariants.RemoveRange(variants);
            await _context.SaveChangesAsync();

            // Nếu variant bị xóa có default, set variant đầu tiên còn lại làm default
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
                // Nếu không còn variant nào, xóa folder sản phẩm
                DeleteProductFolder(product);
            }

            return Json(new { 
                success = true, 
                message = $"Đã xóa {variants.Count} variant(s) thành công",
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

            // Lấy thông tin variant và product để tạo folder structure
            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.VariantId == variantId);

            if (variant == null)
            {
                throw new InvalidOperationException("Variant not found");
            }

            // Tạo folder name cho sản phẩm (từ tên sản phẩm)
            var productFolderName = GenerateSlug(variant.Product.Name);
            
            // Tạo folder name cho variant (từ tên variant hoặc màu sắc)
            string variantFolderName;
            if (!string.IsNullOrEmpty(variant.ColorName))
            {
                // Nếu có màu sắc, dùng tên màu
                variantFolderName = GenerateSlug(variant.ColorName);
            }
            else if (!string.IsNullOrEmpty(variant.VariantName))
            {
                // Nếu không có màu, dùng tên variant
                variantFolderName = GenerateSlug(variant.VariantName);
            }
            else
            {
                // Fallback: dùng VariantId
                variantFolderName = $"variant-{variantId}";
            }

            // Create directory path: wwwroot/public/images/products/{ProductName}/{VariantName}/
            var uploadPath = Path.Combine(_environment.WebRootPath, "public", "images", "products", productFolderName, variantFolderName);
            
            // Tạo folder sản phẩm nếu chưa tồn tại
            var productFolderPath = Path.Combine(_environment.WebRootPath, "public", "images", "products", productFolderName);
            if (!Directory.Exists(productFolderPath))
            {
                Directory.CreateDirectory(productFolderPath);
            }
            
            // Tạo folder variant nếu chưa tồn tại
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
        /// Xóa folder variant khi xóa variant
        /// </summary>
        private void DeleteVariantFolder(ProductVariant variant)
        {
            if (string.IsNullOrEmpty(_environment.WebRootPath) || variant.Product == null)
            {
                return;
            }

            try
            {
                // Tạo folder name cho sản phẩm
                var productFolderName = GenerateSlug(variant.Product.Name);
                
                // Tạo folder name cho variant
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
                
                // Xóa folder variant nếu tồn tại
                if (Directory.Exists(variantFolderPath))
                {
                    Directory.Delete(variantFolderPath, true); // true = xóa cả folder con và file bên trong
                }
            }
            catch (Exception ex)
            {
                // Log error nhưng không throw để không ảnh hưởng đến việc xóa variant
                Console.WriteLine($"Error deleting variant folder: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa folder sản phẩm khi không còn variant nào
        /// </summary>
        private void DeleteProductFolder(Product product)
        {
            if (string.IsNullOrEmpty(_environment.WebRootPath) || product == null)
            {
                return;
            }

            try
            {
                // Tạo folder name cho sản phẩm
                var productFolderName = GenerateSlug(product.Name);
                
                // Path: wwwroot/public/images/products/{ProductName}/
                var productFolderPath = Path.Combine(_environment.WebRootPath, "public", "images", "products", productFolderName);
                
                // Xóa folder sản phẩm nếu tồn tại và rỗng (hoặc xóa luôn nếu muốn)
                if (Directory.Exists(productFolderPath))
                {
                    // Kiểm tra xem folder có rỗng không
                    var hasFiles = Directory.GetFiles(productFolderPath, "*", SearchOption.AllDirectories).Length > 0;
                    var hasSubFolders = Directory.GetDirectories(productFolderPath).Length > 0;
                    
                    // Nếu folder rỗng hoặc không còn subfolder nào, xóa luôn
                    if (!hasFiles && !hasSubFolders)
                    {
                        Directory.Delete(productFolderPath, true);
                    }
                    // Nếu còn subfolder nhưng rỗng, xóa các subfolder rỗng
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
                // Log error nhưng không throw
                Console.WriteLine($"Error deleting product folder: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo slug từ text (dùng để tạo tên folder)
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

            // Remove special characters (giữ lại chữ, số, dấu gạch ngang và khoảng trắng)
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

