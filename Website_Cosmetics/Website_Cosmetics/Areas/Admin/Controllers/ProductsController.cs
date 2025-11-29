using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;
using System.Text.RegularExpressions;

namespace Website_Cosmetics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.ProductVariantImages.Where(img => img.IsPrimary))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(products);
        }

        // GET: Admin/Products/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Brands = await _context.Brands.Where(b => b.IsActive == true).OrderBy(b => b.Name).ToListAsync();
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive == true).OrderBy(c => c.Name).ToListAsync();
            return View();
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Product product,
            int? brandId,
            int? categoryId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Set brand and category
                    product.BrandId = brandId;
                    product.CategoryId = categoryId;
                    product.CreatedAt = DateTime.Now;
                    product.UpdatedAt = DateTime.Now;

                    // Generate slug if not provided
                    if (string.IsNullOrEmpty(product.Slug))
                    {
                        product.Slug = GenerateSlug(product.Name);
                    }

                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();

                    // Tạo folder cho sản phẩm theo tên sản phẩm
                    CreateProductFolder(product);

                    // Bước 1: Tự động tạo variant mặc định cho sản phẩm
                    var defaultVariant = new ProductVariant
                    {
                        ProductId = product.ProductId,
                        VariantName = product.Name, // Tên variant mặc định = tên sản phẩm
                        IsDefault = true,
                        IsActive = true,
                        Stock = 0,
                        DisplayOrder = 0,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    _context.ProductVariants.Add(defaultVariant);
                    await _context.SaveChangesAsync();

                    // Redirect đến trang quản lý variants để thêm variant chi tiết
                    TempData["SuccessMessage"] = $"Sản phẩm '{product.Name}' đã được tạo thành công. Vui lòng thêm các biến thể (variants) cho sản phẩm này.";
                    return RedirectToAction("Index", "Variants", new { productId = product.ProductId });
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating product: {ex.Message}");
            }

            ViewBag.Brands = await _context.Brands.Where(b => b.IsActive == true).OrderBy(b => b.Name).ToListAsync();
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive == true).OrderBy(c => c.Name).ToListAsync();
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.ProductVariantImages.OrderBy(img => img.DisplayOrder))
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Brands = await _context.Brands.Where(b => b.IsActive == true).OrderBy(b => b.Name).ToListAsync();
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive == true).OrderBy(c => c.Name).ToListAsync();
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Product product,
            int? brandId,
            int? categoryId,
            List<IFormFile>? newVariantImages,
            List<int>? primaryImageIndices,
            List<int>? makeupReferenceIndices,
            List<int>? existingImageIds,
            List<string>? existingIsPrimary,
            List<string>? existingIsMakeupReference,
            List<int>? existingDisplayOrder)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products
                        .Include(p => p.ProductVariants)
                            .ThenInclude(v => v.ProductVariantImages)
                        .FirstOrDefaultAsync(p => p.ProductId == id);

                    if (existingProduct == null)
                    {
                        return NotFound();
                    }

                    // Update product properties
                    existingProduct.Name = product.Name;
                    existingProduct.Slug = string.IsNullOrEmpty(product.Slug) ? GenerateSlug(product.Name) : product.Slug;
                    existingProduct.BasePrice = product.BasePrice;
                    existingProduct.Description = product.Description;
                    existingProduct.Ingredients = product.Ingredients;
                    existingProduct.SPF = product.SPF;
                    existingProduct.Size = product.Size;
                    existingProduct.Finish = product.Finish;
                    existingProduct.IsActive = product.IsActive;
                    existingProduct.BrandId = brandId;
                    existingProduct.CategoryId = categoryId;
                    existingProduct.UpdatedAt = DateTime.Now;

                    // Update existing images
                    if (existingImageIds != null && existingDisplayOrder != null)
                    {
                        // First, reset all primary flags for this variant
                        var firstVariantId = existingProduct.ProductVariants.FirstOrDefault()?.VariantId ?? 0;
                        if (firstVariantId > 0)
                        {
                            var allVariantImages = await _context.ProductVariantImages
                                .Where(img => img.VariantId == firstVariantId)
                                .ToListAsync();
                            foreach (var img in allVariantImages)
                            {
                                img.IsPrimary = false;
                                img.IsMakeupReference = false;
                            }
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

                    // Upload new images for the first variant (or create a default variant if none exists)
                    var firstVariant = existingProduct.ProductVariants.FirstOrDefault();
                    if (firstVariant == null && newVariantImages != null && newVariantImages.Count > 0)
                    {
                        // Create a default variant
                        firstVariant = new ProductVariant
                        {
                            ProductId = existingProduct.ProductId,
                            VariantName = existingProduct.Name,
                            IsDefault = true,
                            IsActive = true,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };
                        _context.ProductVariants.Add(firstVariant);
                        await _context.SaveChangesAsync();
                    }

                    if (firstVariant != null && newVariantImages != null && newVariantImages.Count > 0)
                    {
                        // If no existing primary image, ensure at least one new image is primary
                        var hasPrimary = existingProduct.ProductVariants
                            .SelectMany(v => v.ProductVariantImages)
                            .Any(img => img.IsPrimary);
                        
                        if (!hasPrimary && (primaryImageIndices == null || primaryImageIndices.Count == 0))
                        {
                            primaryImageIndices = new List<int> { 0 }; // Set first image as primary
                        }
                        
                        await UploadVariantImages(firstVariant.VariantId, newVariantImages, primaryImageIndices, makeupReferenceIndices);
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Brands = await _context.Brands.Where(b => b.IsActive == true).OrderBy(b => b.Name).ToListAsync();
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive == true).OrderBy(c => c.Name).ToListAsync();
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.ProductVariantImages)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product != null)
            {
                // Delete image files
                foreach (var variant in product.ProductVariants)
                {
                    foreach (var image in variant.ProductVariantImages)
                    {
                        DeleteImageFile(image.Url);
                    }
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Products/DeleteImage
        [HttpPost]
        public async Task<IActionResult> DeleteImage([FromBody] DeleteImageRequest request)
        {
            var image = await _context.ProductVariantImages.FindAsync(request.imageId);
            if (image == null)
            {
                return Json(new { success = false, message = "Image not found" });
            }

            DeleteImageFile(image.Url);
            _context.ProductVariantImages.Remove(image);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
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

            // Create directory path: wwwroot/public/images/products
            var uploadPath = Path.Combine(_environment.WebRootPath, "public", "images", "products");
            
            // Check and create directory if it doesn't exist
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

                    var imageUrl = $"/public/images/products/{fileName}";
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

            // Remove special characters
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");

            // Replace spaces with hyphens
            text = Regex.Replace(text, @"\s+", "-");

            // Remove multiple hyphens
            text = Regex.Replace(text, @"-+", "-");

            // Trim hyphens from start and end
            text = text.Trim('-');

            return text;
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }

        /// <summary>
        /// Tạo folder cho sản phẩm theo tên sản phẩm
        /// </summary>
        private void CreateProductFolder(Product product)
        {
            if (string.IsNullOrEmpty(_environment.WebRootPath))
            {
                return;
            }

            try
            {
                // Tạo folder name từ tên sản phẩm (dùng slug)
                var folderName = GenerateSlug(product.Name);
                
                // Path: wwwroot/public/images/products/{ProductName}/
                var productFolderPath = Path.Combine(_environment.WebRootPath, "public", "images", "products", folderName);
                
                // Tạo folder nếu chưa tồn tại
                if (!Directory.Exists(productFolderPath))
                {
                    Directory.CreateDirectory(productFolderPath);
                }
            }
            catch (Exception ex)
            {
                // Log error nhưng không throw để không ảnh hưởng đến việc tạo sản phẩm
                Console.WriteLine($"Error creating product folder: {ex.Message}");
            }
        }
    }
}

