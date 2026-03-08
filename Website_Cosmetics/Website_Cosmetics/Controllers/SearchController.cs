using Microsoft.AspNetCore.Mvc;
using Website_Cosmetics.Repositories;

namespace Website_Cosmetics.Controllers
{
    public class SearchController : Controller
    {
        private readonly IProductRepository _productRepository;

        public SearchController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // GET: /Search/Autocomplete?keyword=...
        [HttpGet]
        public async Task<IActionResult> Autocomplete(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Json(new { success = true, products = new List<object>() });
            }

            var products = await _productRepository.SearchAsync(keyword);

            var results = products.Select(p =>
            {
                // Get default variant for image
                var defaultVariant = p.ProductVariants?.FirstOrDefault(v => v.IsDefault == true && (v.IsActive == true || v.IsActive == null))
                                 ?? p.ProductVariants?.FirstOrDefault(v => v.IsActive == true || v.IsActive == null)
                                 ?? p.ProductVariants?.FirstOrDefault();

                var imageUrl = defaultVariant?.ProductVariantImages?.FirstOrDefault(img => img.IsPrimary)?.Url
                            ?? defaultVariant?.ProductVariantImages?.FirstOrDefault()?.Url
                            ?? p.ProductImages?.FirstOrDefault()?.Url
                            ?? "/public/images/products/img-cart-lip.png";

                var price = defaultVariant?.Price ?? p.BasePrice;

                return new
                {
                    id = p.ProductId,
                    name = p.Name,
                    brand = p.Brand?.Name ?? "Unknown",
                    price = price,
                    imageUrl = imageUrl,
                    url = Url.Action("Details", "Products", new { id = p.ProductId })
                };
            }).ToList();

            return Json(new { success = true, products = results });
        }
    }
}

