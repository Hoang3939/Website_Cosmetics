using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;

namespace Website_Cosmetics.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartController> _logger;

        public CartController(ApplicationDbContext context, ILogger<CartController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var cart = await GetOrCreateCartAsync();
            if (cart == null)
            {
                return View(new List<CartItem>());
            }

            var cartItems = await _context.CartItems
                .Where(ci => ci.CartId == cart.CartId)
                .Include(ci => ci.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                        .ThenInclude(p => p.Brand)
                .Include(ci => ci.ProductVariant)
                    .ThenInclude(pv => pv.ProductVariantImages)
                .OrderByDescending(ci => ci.CreatedAt)
                .ToListAsync();

            return View(cartItems);
        }

        // POST: /Cart/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int variantId, int quantity = 1)
        {
            if (quantity <= 0)
            {
                return Json(new { success = false, message = "Quantity must be greater than 0" });
            }

            var variant = await _context.ProductVariants
                .Include(pv => pv.Product)
                .FirstOrDefaultAsync(pv => pv.VariantId == variantId);

            if (variant == null)
            {
                return Json(new { success = false, message = "Product variant not found" });
            }

            if (variant.Stock < quantity)
            {
                return Json(new { success = false, message = $"Only {variant.Stock} items available in stock" });
            }

            var cart = await GetOrCreateCartAsync();
            if (cart == null)
            {
                return Json(new { success = false, message = "Please login to add items to cart", requiresLogin = true });
            }

            // Check if item already exists in cart
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.VariantId == variantId);

            if (existingItem != null)
            {
                // Update quantity
                existingItem.Quantity += quantity;
                if (existingItem.Quantity > variant.Stock)
                {
                    existingItem.Quantity = variant.Stock;
                }
                existingItem.UpdatedAt = DateTime.UtcNow;
                _context.CartItems.Update(existingItem);
            }
            else
            {
                // Add new item
                var price = variant.Price ?? variant.Product.BasePrice;
                var cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    VariantId = variantId,
                    Quantity = quantity,
                    Price = price,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.CartItems.Add(cartItem);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            _context.ShoppingCarts.Update(cart);

            await _context.SaveChangesAsync();

            var cartCount = await GetCartCountAsync();
            return Json(new { success = true, message = "Item added to cart", cartCount });
        }

        // POST: /Cart/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int cartItemId, int quantity)
        {
            if (quantity <= 0)
            {
                return Json(new { success = false, message = "Quantity must be greater than 0" });
            }

            var cartItem = await _context.CartItems
                .Include(ci => ci.ProductVariant)
                .Include(ci => ci.ShoppingCart)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Cart item not found" });
            }

            // Check if user owns this cart
            var userId = User.Identity?.IsAuthenticated == true 
                ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0")
                : (int?)null;

            if (cartItem.ShoppingCart.UserId != userId)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            // Check stock
            if (quantity > cartItem.ProductVariant.Stock)
            {
                return Json(new { success = false, message = $"Only {cartItem.ProductVariant.Stock} items available in stock" });
            }

            cartItem.Quantity = quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
            cartItem.ShoppingCart.UpdatedAt = DateTime.UtcNow;

            _context.CartItems.Update(cartItem);
            _context.ShoppingCarts.Update(cartItem.ShoppingCart);
            await _context.SaveChangesAsync();

            var subtotal = cartItem.Subtotal;
            var cartTotal = cartItem.ShoppingCart.Total;
            var cartCount = await GetCartCountAsync();

            return Json(new { 
                success = true, 
                message = "Cart updated", 
                subtotal,
                cartTotal,
                cartCount
            });
        }

        // POST: /Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.ShoppingCart)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Cart item not found" });
            }

            // Check if user owns this cart
            var userId = User.Identity?.IsAuthenticated == true 
                ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0")
                : (int?)null;

            if (cartItem.ShoppingCart.UserId != userId)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            _context.CartItems.Remove(cartItem);
            cartItem.ShoppingCart.UpdatedAt = DateTime.UtcNow;
            _context.ShoppingCarts.Update(cartItem.ShoppingCart);
            await _context.SaveChangesAsync();

            var cartCount = await GetCartCountAsync();
            var cartTotal = cartItem.ShoppingCart.Total;

            return Json(new { 
                success = true, 
                message = "Item removed from cart",
                cartTotal,
                cartCount
            });
        }

        // GET: /Cart/Count
        [HttpGet]
        public async Task<IActionResult> GetCount()
        {
            var count = await GetCartCountAsync();
            return Json(new { count });
        }

        // Helper method to get or create cart
        private async Task<ShoppingCart?> GetOrCreateCartAsync()
        {
            var userId = User.Identity?.IsAuthenticated == true 
                ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0")
                : (int?)null;

            if (userId == null)
            {
                return null; // Guest users need to login
            }

            var cart = await _context.ShoppingCarts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.ShoppingCarts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        // Helper method to get cart count
        private async Task<int> GetCartCountAsync()
        {
            var userId = User.Identity?.IsAuthenticated == true 
                ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0")
                : (int?)null;

            if (userId == null)
            {
                return 0;
            }

            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart?.TotalItems ?? 0;
        }
    }
}

