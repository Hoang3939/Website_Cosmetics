using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;

namespace Website_Cosmetics.Controllers
{
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WishlistController> _logger;

        public WishlistController(ApplicationDbContext context, ILogger<WishlistController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Wishlist
        public async Task<IActionResult> Index()
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var wishlistItems = await _context.ProductLikes
                .Where(pl => pl.UserId == userId)
                .Include(pl => pl.Product)
                    .ThenInclude(p => p.Brand)
                .Include(pl => pl.Product)
                    .ThenInclude(p => p.ProductImages)
                .Include(pl => pl.Product)
                    .ThenInclude(p => p.ProductVariants)
                        .ThenInclude(v => v.ProductVariantImages)
                .OrderByDescending(pl => pl.CreatedAt)
                .ToListAsync();

            return View(wishlistItems);
        }

        // POST: /Wishlist/Toggle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int productId)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return Json(new { success = false, message = "Please login to add items to wishlist", requiresLogin = true });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var existingLike = await _context.ProductLikes
                .FirstOrDefaultAsync(pl => pl.ProductId == productId && pl.UserId == userId);

            if (existingLike != null)
            {
                // Remove from wishlist
                _context.ProductLikes.Remove(existingLike);
                
                // Update product like count
                var product = await _context.Products.FindAsync(productId);
                if (product != null && product.LikeCount > 0)
                {
                    product.LikeCount--;
                    _context.Products.Update(product);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, isLiked = false, message = "Removed from wishlist" });
            }
            else
            {
                // Add to wishlist
                var newLike = new ProductLike
                {
                    ProductId = productId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ProductLikes.Add(newLike);

                // Update product like count
                var product = await _context.Products.FindAsync(productId);
                if (product != null)
                {
                    product.LikeCount++;
                    _context.Products.Update(product);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, isLiked = true, message = "Added to wishlist" });
            }
        }

        // POST: /Wishlist/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int likeId)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            
            _logger.LogInformation("Attempting to remove wishlist item. LikeId: {LikeId}, UserId: {UserId}", likeId, userId);
            
            var like = await _context.ProductLikes
                .Include(pl => pl.Product)
                .FirstOrDefaultAsync(pl => pl.LikeId == likeId && pl.UserId == userId);

            if (like == null)
            {
                _logger.LogWarning("Wishlist item not found. LikeId: {LikeId}, UserId: {UserId}", likeId, userId);
                TempData["ErrorMessage"] = "Item not found in wishlist.";
                return RedirectToAction("Index");
            }

            var productId = like.ProductId;
            _context.ProductLikes.Remove(like);

            // Update product like count
            var product = await _context.Products.FindAsync(productId);
            if (product != null && product.LikeCount > 0)
            {
                product.LikeCount--;
                _context.Products.Update(product);
            }

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully removed wishlist item. LikeId: {LikeId}, ProductId: {ProductId}, UserId: {UserId}", 
                    likeId, productId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing wishlist item. LikeId: {LikeId}, UserId: {UserId}", likeId, userId);
                TempData["ErrorMessage"] = "An error occurred while removing the item. Please try again.";
                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] = "Item removed from wishlist.";
            return RedirectToAction("Index");
        }

        // GET: /Wishlist/Check
        [HttpGet]
        public async Task<IActionResult> Check(int productId)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return Json(new { isLiked = false });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var isLiked = await _context.ProductLikes
                .AnyAsync(pl => pl.ProductId == productId && pl.UserId == userId);

            return Json(new { isLiked });
        }
    }
}
