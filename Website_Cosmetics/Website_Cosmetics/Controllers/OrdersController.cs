using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;

namespace Website_Cosmetics.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(ApplicationDbContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Orders
        public async Task<IActionResult> Index()
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                            .ThenInclude(p => p.ProductImages)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.ProductVariantImages)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        // GET: /Orders/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                            .ThenInclude(p => p.Brand)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                            .ThenInclude(p => p.ProductImages)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.ProductVariantImages)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: /Orders/Checkout
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                            .ThenInclude(p => p.Brand)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(pv => pv.ProductVariantImages)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            // Get user addresses
            var addresses = await _context.UserAddresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();

            ViewBag.Addresses = addresses;

            // Check stock availability
            var outOfStockItems = new List<string>();
            foreach (var item in cart.CartItems)
            {
                if (item.ProductVariant.Stock < item.Quantity)
                {
                    outOfStockItems.Add($"{item.ProductVariant.Product.Name} - {item.ProductVariant.VariantName}");
                }
            }

            if (outOfStockItems.Any())
            {
                TempData["ErrorMessage"] = $"Some items are out of stock: {string.Join(", ", outOfStockItems)}";
                return RedirectToAction("Index", "Cart");
            }

            // Get user info for shipping address
            var user = await _context.Users.FindAsync(userId);

            ViewBag.Cart = cart;
            ViewBag.User = user;

            return View();
        }

        // POST: /Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? selectedAddressId, string shippingAddress, string shippingMethod, string paymentMethod, string? notes)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return Json(new { success = false, message = "Please login to checkout" });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                return Json(new { success = false, message = "Your cart is empty" });
            }

            // Get shipping address
            string finalShippingAddress = shippingAddress;
            if (selectedAddressId.HasValue)
            {
                var selectedAddress = await _context.UserAddresses
                    .FirstOrDefaultAsync(a => a.AddressId == selectedAddressId.Value && a.UserId == userId);
                
                if (selectedAddress != null)
                {
                    finalShippingAddress = selectedAddress.FullAddress;
                }
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(finalShippingAddress))
            {
                return Json(new { success = false, message = "Shipping address is required" });
            }

            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                return Json(new { success = false, message = "Payment method is required" });
            }

            // Check stock again
            foreach (var item in cart.CartItems)
            {
                if (item.ProductVariant.Stock < item.Quantity)
                {
                    return Json(new { 
                        success = false, 
                        message = $"{item.ProductVariant.Product.Name} - {item.ProductVariant.VariantName} is out of stock" 
                    });
                }
            }

            // Calculate totals
            var subtotal = cart.CartItems.Sum(ci => ci.Subtotal);
            var shippingFee = shippingMethod == "Express" ? 15.00m : shippingMethod == "Same-Day" ? 25.00m : 5.00m;
            var discount = 0m; // Can be calculated from coupons/promotions
            var total = subtotal + shippingFee - discount;

            // Generate order number
            var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";

            // Create order
            var order = new Order
            {
                UserId = userId,
                OrderNumber = orderNumber,
                Status = "Pending",
                SubTotal = subtotal,
                ShippingFee = shippingFee,
                Discount = discount,
                Total = total,
                ShippingAddress = finalShippingAddress,
                ShippingMethod = shippingMethod ?? "Standard",
                PaymentMethod = paymentMethod,
                PaymentStatus = "Pending",
                Notes = notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Save to get OrderId

            // Create order items
            foreach (var cartItem in cart.CartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    VariantId = cartItem.VariantId,
                    ProductName = cartItem.ProductVariant.Product.Name,
                    ColorName = cartItem.ProductVariant.VariantName,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Price,
                    Subtotal = cartItem.Subtotal,
                    CreatedAt = DateTime.UtcNow
                };

                // Update stock
                cartItem.ProductVariant.Stock -= cartItem.Quantity;
                _context.ProductVariants.Update(cartItem.ProductVariant);

                _context.OrderItems.Add(orderItem);
            }

            // Clear cart
            _context.CartItems.RemoveRange(cart.CartItems);
            _context.ShoppingCarts.Remove(cart);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Order created successfully. OrderId: {OrderId}, OrderNumber: {OrderNumber}, UserId: {UserId}", 
                order.OrderId, order.OrderNumber, userId);

            return Json(new { 
                success = true, 
                message = "Order placed successfully", 
                orderId = order.OrderId,
                orderNumber = order.OrderNumber
            });
        }

        // POST: /Orders/Cancel/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return Json(new { success = false, message = "Please login" });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            if (order == null)
            {
                return Json(new { success = false, message = "Order not found" });
            }

            // Only allow cancellation if order is Pending or Processing
            if (order.Status != "Pending" && order.Status != "Processing")
            {
                return Json(new { 
                    success = false, 
                    message = $"Cannot cancel order with status: {order.Status}" 
                });
            }

            // Restore stock
            foreach (var orderItem in order.OrderItems)
            {
                if (orderItem.ProductVariant != null)
                {
                    orderItem.ProductVariant.Stock += orderItem.Quantity;
                    _context.ProductVariants.Update(orderItem.ProductVariant);
                }
            }

            // Update order status
            order.Status = "Cancelled";
            order.UpdatedAt = DateTime.UtcNow;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order cancelled. OrderId: {OrderId}, OrderNumber: {OrderNumber}, UserId: {UserId}", 
                order.OrderId, order.OrderNumber, userId);

            return Json(new { 
                success = true, 
                message = "Order cancelled successfully" 
            });
        }
    }
}

