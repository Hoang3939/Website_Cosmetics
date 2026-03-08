using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Website_Cosmetics.Repositories;
using Website_Cosmetics.Models;
using Website_Cosmetics.Services;
using Website_Cosmetics.Data;
using Microsoft.EntityFrameworkCore;

namespace Website_Cosmetics.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly VirtualMakeupService _virtualMakeupService;
        private readonly ApplicationDbContext _context;
        private readonly IRecommendationService _recommendationService;

        public ProductsController(
            IProductRepository productRepository,
            VirtualMakeupService virtualMakeupService,
            ApplicationDbContext context,
            IRecommendationService recommendationService)
        {
            _productRepository = productRepository;
            _virtualMakeupService = virtualMakeupService;
            _context = context;
            _recommendationService = recommendationService;
        }

        // GET: /Products
        public async Task<IActionResult> Index(int page = 1, int? categoryId = null, string? search = null, string? sortBy = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            const int pageSize = 9; // 9 sản phẩm mỗi trang
            
            // Load categories for filter dropdown
            var categories = await _context.Categories
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.Name)
                .ToListAsync();
            ViewBag.Categories = categories;
            
            IEnumerable<Product> allProducts;
            
            // Filter by category or search
            if (categoryId.HasValue)
            {
                allProducts = await _productRepository.GetByCategoryAsync(categoryId.Value);
                ViewBag.CategoryId = categoryId;
            }
            else if (!string.IsNullOrWhiteSpace(search))
            {
                allProducts = await _productRepository.SearchAsync(search);
                ViewBag.SearchKeyword = search;
            }
            else
            {
                allProducts = await _productRepository.GetAllAsync();
            }
            
            // Apply price filter
            if (minPrice.HasValue || maxPrice.HasValue)
            {
                allProducts = allProducts.Where(p =>
                {
                    // Get the minimum price from variants or use BasePrice
                    var minVariantPrice = p.ProductVariants?
                        .Where(v => v.IsActive == true || v.IsActive == null)
                        .Select(v => v.Price ?? p.BasePrice)
                        .DefaultIfEmpty(p.BasePrice)
                        .Min() ?? p.BasePrice;
                    
                    var maxVariantPrice = p.ProductVariants?
                        .Where(v => v.IsActive == true || v.IsActive == null)
                        .Select(v => v.Price ?? p.BasePrice)
                        .DefaultIfEmpty(p.BasePrice)
                        .Max() ?? p.BasePrice;
                    
                    // Product matches if any variant price falls within range
                    var productMinPrice = Math.Min(minVariantPrice, p.BasePrice);
                    var productMaxPrice = Math.Max(maxVariantPrice, p.BasePrice);
                    
                    bool matchesMin = !minPrice.HasValue || productMaxPrice >= minPrice.Value;
                    bool matchesMax = !maxPrice.HasValue || productMinPrice <= maxPrice.Value;
                    
                    return matchesMin && matchesMax;
                });
                
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;
            }
            
            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "price-low":
                        allProducts = allProducts.OrderBy(p => p.BasePrice);
                        break;
                    case "price-high":
                        allProducts = allProducts.OrderByDescending(p => p.BasePrice);
                        break;
                    case "popular":
                        allProducts = allProducts.OrderByDescending(p => p.LikeCount).ThenByDescending(p => p.Rating ?? 0);
                        break;
                    case "newest":
                    default:
                        allProducts = allProducts.OrderByDescending(p => p.CreatedAt);
                        break;
                }
                ViewBag.SortBy = sortBy;
            }
            else
            {
                // Default: newest first
                allProducts = allProducts.OrderByDescending(p => p.CreatedAt);
            }
            
            // Get price range for slider
            var allProductsForRange = await _productRepository.GetAllAsync();
            var priceList = allProductsForRange
                .SelectMany(p => p.ProductVariants?
                    .Where(v => v.IsActive == true || v.IsActive == null)
                    .Select(v => v.Price ?? p.BasePrice) ?? new[] { p.BasePrice })
                .ToList();
            
            if (priceList.Any())
            {
                ViewBag.MinPriceRange = (int)Math.Floor(priceList.Min());
                ViewBag.MaxPriceRange = (int)Math.Ceiling(priceList.Max());
            }
            else
            {
                ViewBag.MinPriceRange = 0;
                ViewBag.MaxPriceRange = 1000;
            }
            
            var totalProducts = allProducts.Count();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);
            
            // Validate page number
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;
            
            // Lấy sản phẩm cho trang hiện tại
            var products = allProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            // Truyền thông tin phân trang vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.PageSize = pageSize;
            
            return View(products); 
        }

        // GET: /Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdWithVariantsAsync(id);
            if (product == null)
            {
                return NotFound(); 
            }
            
            // Get default variant if exists
            var defaultVariant = await _productRepository.GetDefaultVariantAsync(id);
            
            ViewBag.DefaultVariant = defaultVariant;
            
            // Get recommendations (async, won't block page load)
            ViewBag.ProductId = id;
            
            return View(product); 
        }

        // API: Get variant details by ID
        [HttpGet]
        public async Task<IActionResult> GetVariant(int variantId)
        {
            var variant = await _productRepository.GetVariantByIdAsync(variantId);
            if (variant == null)
            {
                return NotFound();
            }

            // Create images list - always return a list (empty if no images)
            // Use URL as-is from database (browser will handle URL-encoded paths automatically)
            var imagesList = (variant.ProductVariantImages ?? Enumerable.Empty<ProductVariantImage>())
                .OrderBy(img => img.DisplayOrder)
                .Select(img => new
                {
                    imageId = img.ImageId,
                    url = img.Url, // Use URL as-is (browser handles URL-encoded paths like %23 for # and %20 for space)
                    altText = img.AltText,
                    isPrimary = img.IsPrimary,
                    isMakeupReference = img.IsMakeupReference,
                    displayOrder = img.DisplayOrder
                })
                .ToList();

            // Calculate price: use variant.Price if exists, otherwise use product.BasePrice
            var price = variant.Price ?? variant.Product?.BasePrice ?? 0;
            
            var response = new
            {
                variantId = variant.VariantId,
                variantName = variant.VariantName,
                colorName = variant.ColorName,
                colorCode = variant.ColorCode,
                size = variant.Size,
                price = price,
                compareAtPrice = variant.CompareAtPrice,
                stock = variant.Stock,
                isLowStock = variant.IsLowStock,
                sku = variant.SKU,
                images = imagesList
            };

            return Json(response);
        }
        // 1. CREATE (TẠO MỚI)
        // GET: /Products/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                await _productRepository.AddAsync(product);

                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // 2. EDIT (CHỈNH SỬA)
        // GET: /Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: /Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _productRepository.UpdateAsync(product);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _productRepository.GetByIdAsync(id) == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // 3. DELETE (XÓA)
        // GET: /Products/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: /Products/Delete/5
        [HttpPost, ActionName("Delete")] 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // ========================================
        // VIRTUAL MAKEUP FEATURE
        // ========================================

        /// <summary>
        /// API endpoint để thử son ảo với variant cụ thể
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> VirtualTryOn(int variantId, IFormFile customerPhoto)
        {
            try
            {
                // Log incoming request
                Console.WriteLine($"=== VirtualTryOn Request ===");
                Console.WriteLine($"VariantId received: {variantId}");
                Console.WriteLine($"CustomerPhoto: {(customerPhoto != null ? customerPhoto.FileName : "null")}");
                
                // 0. Validate variantId
                if (variantId <= 0)
                {
                    Console.WriteLine($"ERROR: Invalid variantId: {variantId}");
                    return Json(new { 
                        success = false, 
                        message = $"VariantId không hợp lệ: {variantId}. Vui lòng chọn màu sản phẩm trước." 
                    });
                }
                
                // 1. Validate
                if (customerPhoto == null || customerPhoto.Length == 0)
                {
                    return Json(new { success = false, message = "Vui lòng upload ảnh của bạn" });
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(customerPhoto.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    return Json(new { success = false, message = "Chỉ hỗ trợ file JPG, JPEG, PNG" });
                }

                if (customerPhoto.Length > 10 * 1024 * 1024)
                {
                    return Json(new { success = false, message = "Kích thước ảnh tối đa 10MB" });
                }

                // 2. Lấy variant
                Console.WriteLine($"Fetching variant {variantId} from database...");
                var variant = await _productRepository.GetVariantByIdAsync(variantId);
                if (variant == null)
                {
                    Console.WriteLine($"ERROR: Variant {variantId} not found in database");
                    // Kiểm tra xem có variant nào khác không để gợi ý
                    var productId = Request.Query["productId"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(productId) && int.TryParse(productId, out int pid))
                    {
                        var availableVariants = await _productRepository.GetVariantsByProductIdAsync(pid);
                        var variantIds = string.Join(", ", availableVariants.Select(v => v.VariantId));
                        Console.WriteLine($"Available variantIds for product {pid}: {variantIds}");
                        return Json(new { 
                            success = false, 
                            message = $"Không tìm thấy màu sản phẩm với ID {variantId}. Các màu có sẵn: {variantIds}. Vui lòng chọn lại màu." 
                        });
                    }
                    return Json(new { 
                        success = false, 
                        message = $"Không tìm thấy màu sản phẩm với ID {variantId}. Vui lòng chọn màu sản phẩm và thử lại." 
                    });
                }
                
                Console.WriteLine($"Found variant: {variant.VariantName} (Color: {variant.ColorName})");
                Console.WriteLine($"ProductId: {variant.ProductId}, Images count: {variant.ProductVariantImages?.Count ?? 0}");

                // 3. Lấy ảnh model (có khuôn mặt) của variant để làm makeup reference
                // Ưu tiên: 1) Ảnh có IsMakeupReference = true (đánh dấu rõ ràng là ảnh model)
                //          2) Ảnh có AltText chứa "Model" hoặc "Virtual Try-On" (fallback cho dữ liệu cũ)
                //          3) Ảnh có DisplayOrder cao nhất (ảnh model thường có DisplayOrder = 5)
                var makeupReferenceImage = variant.ProductVariantImages?
                    .FirstOrDefault(img => img.IsMakeupReference == true);

                // Fallback: Nếu không có ảnh được đánh dấu IsMakeupReference, 
                // tìm ảnh có AltText chứa "Model" hoặc "Virtual Try-On"
                if (makeupReferenceImage == null)
                {
                    makeupReferenceImage = variant.ProductVariantImages?
                        .Where(img => !string.IsNullOrEmpty(img.AltText) && 
                                     (img.AltText.Contains("Model", StringComparison.OrdinalIgnoreCase) ||
                                      img.AltText.Contains("Virtual Try-On", StringComparison.OrdinalIgnoreCase)))
                        .OrderByDescending(img => img.DisplayOrder)
                        .FirstOrDefault();
                }

                // Fallback 2: Lấy ảnh có DisplayOrder cao nhất (ảnh model thường có DisplayOrder = 5)
                if (makeupReferenceImage == null)
                {
                    makeupReferenceImage = variant.ProductVariantImages?
                        .OrderByDescending(img => img.DisplayOrder)
                        .FirstOrDefault();
                }

                // Final fallback: Lấy ảnh Primary (không khuyến nghị vì thường là ảnh sản phẩm, không có khuôn mặt)
                if (makeupReferenceImage == null)
                {
                    makeupReferenceImage = variant.ProductVariantImages?
                        .FirstOrDefault(img => img.IsPrimary == true);
                }

                if (makeupReferenceImage == null || string.IsNullOrEmpty(makeupReferenceImage.Url))
                {
                    return Json(new { success = false, message = "Màu này chưa có ảnh mẫu. Vui lòng chọn màu khác." });
                }

                Console.WriteLine($"Selected makeup reference image: {makeupReferenceImage.AltText} (URL: {makeupReferenceImage.Url})");

                // 4. Đọc ảnh makeup reference từ disk (nhanh và đáng tin cậy hơn HTTP)
                byte[] makeupStyleImageData;
                try
                {
                    var imageUrl = makeupReferenceImage.Url;
                    Console.WriteLine($"Loading makeup reference image from URL: {imageUrl}");
                    
                    // Nếu URL là relative path (bắt đầu bằng /), đọc trực tiếp từ disk
                    if (imageUrl.StartsWith("/"))
                    {
                        // Remove leading slash
                        var relativePath = imageUrl.TrimStart('/');
                        
                        // Decode URL encoding (ví dụ: %23 → #, %20 → space) - decode từng phần để tránh lỗi
                        var pathParts = relativePath.Split('/');
                        var decodedParts = pathParts.Select(part => Uri.UnescapeDataString(part)).ToArray();
                        var decodedPath = string.Join(Path.DirectorySeparatorChar.ToString(), decodedParts);
                        
                        // Build local path từ wwwroot
                        var localPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", decodedPath);
                        
                        // Normalize path (resolve .. và .)
                        localPath = Path.GetFullPath(localPath);
                        
                        Console.WriteLine($"Local file path: {localPath}");
                        Console.WriteLine($"Decoded relative path: {decodedPath}");
                        
                        // Kiểm tra file có tồn tại không
                        if (!System.IO.File.Exists(localPath))
                        {
                            Console.WriteLine($"ERROR: File not found: {localPath}");
                            Console.WriteLine($"Original URL: {imageUrl}");
                            Console.WriteLine($"Decoded path: {decodedPath}");
                            
                            // Thử tìm file với các biến thể path khác
                            var alternatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.Replace('/', Path.DirectorySeparatorChar));
                            Console.WriteLine($"Trying alternate path: {alternatePath}");
                            
                            if (System.IO.File.Exists(alternatePath))
                            {
                                localPath = alternatePath;
                                Console.WriteLine($"Found file at alternate path!");
                            }
                            else
                            {
                                return Json(new { 
                                    success = false, 
                                    message = $"Không tìm thấy ảnh mẫu trên server.\nĐường dẫn database: {imageUrl}\nĐường dẫn file: {localPath}\nVui lòng kiểm tra file có tồn tại không." 
                                });
                            }
                        }
                        
                        // Đọc file trực tiếp từ disk
                        makeupStyleImageData = await System.IO.File.ReadAllBytesAsync(localPath);
                        Console.WriteLine($"✅ Read image from disk: {makeupStyleImageData.Length} bytes");
                    }
                    else if (imageUrl.StartsWith("http"))
                    {
                        // Nếu là HTTP URL, download qua HTTP
                        using (var httpClient = new HttpClient())
                        {
                            httpClient.Timeout = TimeSpan.FromSeconds(10);
                            Console.WriteLine($"Downloading image from HTTP: {imageUrl}");
                            var response = await httpClient.GetAsync(imageUrl);
                            
                            if (!response.IsSuccessStatusCode)
                            {
                                return Json(new { 
                                    success = false, 
                                    message = $"Không thể tải ảnh mẫu. Lỗi HTTP {response.StatusCode}: {response.ReasonPhrase}. URL: {imageUrl}" 
                                });
                            }
                            
                            makeupStyleImageData = await response.Content.ReadAsByteArrayAsync();
                            Console.WriteLine($"Downloaded image from HTTP: {makeupStyleImageData.Length} bytes");
                        }
                    }
                    else
                    {
                        return Json(new { 
                            success = false, 
                            message = $"Đường dẫn ảnh không hợp lệ: {imageUrl}" 
                        });
                    }
                    
                    // Validate image data
                    if (makeupStyleImageData == null || makeupStyleImageData.Length == 0)
                    {
                        return Json(new { 
                            success = false, 
                            message = "Ảnh mẫu trống hoặc không hợp lệ" 
                        });
                    }
                    
                    Console.WriteLine($"✅ Makeup reference image loaded successfully: {makeupStyleImageData.Length} bytes");
                }
                catch (DirectoryNotFoundException ex)
                {
                    Console.WriteLine($"Directory not found: {ex.Message}");
                    return Json(new { 
                        success = false, 
                        message = $"Thư mục chứa ảnh không tồn tại: {ex.Message}" 
                    });
                }
                catch (FileNotFoundException ex)
                {
                    Console.WriteLine($"File not found: {ex.Message}");
                    return Json(new { 
                        success = false, 
                        message = $"Không tìm thấy file ảnh: {ex.Message}" 
                    });
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"HTTP Error: {ex.Message}");
                    return Json(new { 
                        success = false, 
                        message = $"Lỗi khi tải ảnh mẫu: {ex.Message}. Vui lòng kiểm tra đường dẫn ảnh." 
                    });
                }
                catch (TaskCanceledException ex)
                {
                    Console.WriteLine($"Timeout: {ex.Message}");
                    return Json(new { 
                        success = false, 
                        message = "Hết thời gian chờ khi tải ảnh mẫu" 
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading image: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    return Json(new { 
                        success = false, 
                        message = $"Lỗi khi đọc ảnh mẫu: {ex.Message}" 
                    });
                }

                // 5. Kiểm tra PSGAN API có hoạt động không
                var isApiHealthy = await _virtualMakeupService.IsApiHealthyAsync();
                if (!isApiHealthy)
                {
                    return Json(new { 
                        success = false, 
                        message = "PSGAN API không khả dụng. Vui lòng kiểm tra xem API đã chạy chưa (http://localhost:5000)" 
                    });
                }

                // 6. Gọi PSGAN API
                byte[] resultImage;
                try
                {
                    Console.WriteLine($"Calling PSGAN API for variant {variantId}...");
                    resultImage = await _virtualMakeupService.ApplyVirtualMakeupAsync(
                        customerPhoto,
                        makeupStyleImageData
                    );
                    
                    if (resultImage == null || resultImage.Length == 0)
                    {
                        return Json(new { 
                            success = false, 
                            message = "PSGAN API trả về ảnh rỗng" 
                        });
                    }
                    
                    Console.WriteLine($"PSGAN API returned image: {resultImage.Length} bytes");
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"HTTP Error calling PSGAN API: {ex.Message}");
                    return Json(new { 
                        success = false, 
                        message = $"Lỗi khi gọi PSGAN API: {ex.Message}. Vui lòng kiểm tra xem API có đang chạy không (http://localhost:5000/api/health)" 
                    });
                }
                catch (TaskCanceledException ex)
                {
                    Console.WriteLine($"Timeout calling PSGAN API: {ex.Message}");
                    return Json(new { 
                        success = false, 
                        message = "PSGAN API xử lý quá lâu (timeout). Vui lòng thử lại." 
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error calling PSGAN API: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    return Json(new { 
                        success = false, 
                        message = $"Lỗi khi xử lý ảnh: {ex.Message}" 
                    });
                }

                // 7. Trả về ảnh kết quả
                // Return image with makeup reference URL in response headers for frontend to display
                Response.Headers["X-Makeup-Reference-URL"] = makeupReferenceImage.Url;
                return File(resultImage, "image/png");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VirtualTryOn error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}
   
