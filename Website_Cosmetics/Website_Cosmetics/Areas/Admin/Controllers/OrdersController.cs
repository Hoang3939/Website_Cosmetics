using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Website_Cosmetics.Attributes;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;
using Website_Cosmetics.Services;

namespace Website_Cosmetics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [RequirePermissionOrAdmin("Order.Manage")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrdersController> _logger;
        private readonly IEmailService _emailService;

        public OrdersController(ApplicationDbContext context, ILogger<OrdersController> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        // GET: /Admin/Orders/
        public async Task<IActionResult> Index(
            string? search = null,
            string? status = null,
            string? paymentStatus = null,
            int page = 1,
            int pageSize = 10)
        {
            // Lấy tất cả orders
            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .AsQueryable();

            // Filter theo search (OrderNumber, Customer name, Customer email)
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                ordersQuery = ordersQuery.Where(o =>
                    o.OrderNumber.Contains(search) ||
                    (o.User != null && o.User.Username.Contains(search)) ||
                    (o.User != null && o.User.Email.Contains(search)) ||
                    (o.User != null && o.User.FirstName != null && o.User.FirstName.Contains(search)) ||
                    (o.User != null && o.User.LastName != null && o.User.LastName.Contains(search)) ||
                    (o.User != null && (o.User.FirstName + " " + o.User.LastName).Contains(search))
                );
            }

            // Filter theo trạng thái đơn hàng
            if (!string.IsNullOrWhiteSpace(status))
            {
                ordersQuery = ordersQuery.Where(o => o.Status == status);
            }

            // Filter theo trạng thái thanh toán
            if (!string.IsNullOrWhiteSpace(paymentStatus))
            {
                ordersQuery = ordersQuery.Where(o => o.PaymentStatus == paymentStatus);
            }

            // Lấy tổng số để phân trang
            var totalCount = await ordersQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Validate page number
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // Phân trang
            var orders = await ordersQuery
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền filter parameters vào ViewBag
            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.PaymentStatus = paymentStatus;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

            // Danh sách các trạng thái có thể chọn
            ViewBag.StatusList = new List<string> { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
            ViewBag.PaymentStatusList = new List<string> { "Pending", "Paid", "Failed", "Refunded" };

            return View(orders);
        }

        // GET: /Admin/Orders/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: /Admin/Orders/UpdateStatus/{id}
        public async Task<IActionResult> UpdateStatus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Danh sách các trạng thái có thể chọn
            ViewBag.StatusList = new List<string> { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
            ViewBag.PaymentStatusList = new List<string> { "Pending", "Paid", "Failed", "Refunded" };

            return View(order);
        }

        // POST: /Admin/Orders/UpdateStatus/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, string? paymentStatus = null)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return NotFound();
                }

                // Validate status
                var validStatuses = new List<string> { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
                if (!validStatuses.Contains(status))
                {
                    ModelState.AddModelError("Status", "Trạng thái không hợp lệ.");
                    ViewBag.StatusList = validStatuses;
                    ViewBag.PaymentStatusList = new List<string> { "Pending", "Paid", "Failed", "Refunded" };
                    return View(order);
                }

                // VALIDATION: Không cho phép chuyển từ "Delivered" về các trạng thái trước đó
                if (order.Status == "Delivered" && status != "Delivered")
                {
                    ModelState.AddModelError("Status", "Cannot change order status from 'Delivered' to another status. Once an order is delivered, it cannot be reverted.");
                    ViewBag.StatusList = validStatuses;
                    ViewBag.PaymentStatusList = new List<string> { "Pending", "Paid", "Failed", "Refunded" };
                    TempData["ErrorMessage"] = "Cannot change order status from 'Delivered' to another status. Once an order is delivered, it cannot be reverted.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }

                // VALIDATION: Không cho phép chuyển từ "Cancelled" về các trạng thái khác (trừ khi có lý do đặc biệt)
                if (order.Status == "Cancelled" && status != "Cancelled")
                {
                    ModelState.AddModelError("Status", "Cannot change order status from 'Cancelled' to another status. A cancelled order cannot be reactivated.");
                    ViewBag.StatusList = validStatuses;
                    ViewBag.PaymentStatusList = new List<string> { "Pending", "Paid", "Failed", "Refunded" };
                    TempData["ErrorMessage"] = "Cannot change order status from 'Cancelled' to another status. A cancelled order cannot be reactivated.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }

                // Nếu hủy đơn hàng, tự động set PaymentStatus = Refunded (nếu đã thanh toán)
                if (status == "Cancelled" && order.PaymentStatus == "Paid")
                {
                    order.PaymentStatus = "Refunded";
                }
                else if (!string.IsNullOrWhiteSpace(paymentStatus))
                {
                    // Validate payment status
                    var validPaymentStatuses = new List<string> { "Pending", "Paid", "Failed", "Refunded" };
                    if (validPaymentStatuses.Contains(paymentStatus))
                    {
                        order.PaymentStatus = paymentStatus;
                    }
                }

                // Store old status to check if it changed
                var oldStatus = order.Status;
                order.Status = status;
                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Send email notification if status changed and customer email exists
                if (oldStatus != status)
                {
                    // Load order with user to get email
                    var orderWithUser = await _context.Orders
                        .Include(o => o.User)
                        .FirstOrDefaultAsync(o => o.OrderId == id);

                    if (orderWithUser?.User != null && !string.IsNullOrWhiteSpace(orderWithUser.User.Email))
                    {
                        try
                        {
                            var customerName = !string.IsNullOrWhiteSpace(orderWithUser.User.FirstName) 
                                ? $"{orderWithUser.User.FirstName} {orderWithUser.User.LastName}".Trim()
                                : orderWithUser.User.Username;

                            await _emailService.SendOrderStatusUpdateAsync(
                                orderWithUser.User.Email,
                                customerName,
                                orderWithUser.OrderNumber,
                                status,
                                orderWithUser.Total,
                                orderWithUser.ShippingAddress
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send order status update email for order {OrderId}", id);
                            // Don't fail the request if email fails, just log it
                        }
                    }
                }

                TempData["SuccessMessage"] = $"Trạng thái đơn hàng '{order.OrderNumber}' đã được cập nhật thành công.";
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status {OrderId}", id);
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật trạng thái đơn hàng: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }

        // GET: /Admin/Orders/Cancel/{id}
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            // Kiểm tra xem đơn hàng đã bị hủy chưa
            if (order.Status == "Cancelled")
            {
                TempData["ErrorMessage"] = "Đơn hàng này đã bị hủy trước đó.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            // Kiểm tra xem đơn hàng đã được giao chưa
            if (order.Status == "Delivered")
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn hàng đã được giao.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            return View(order);
        }

        // POST: /Admin/Orders/Cancel/{id}
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id, string? cancelReason = null)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return NotFound();
                }

                // Kiểm tra xem đơn hàng đã bị hủy chưa
                if (order.Status == "Cancelled")
                {
                    TempData["ErrorMessage"] = "Đơn hàng này đã bị hủy trước đó.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }

                // Kiểm tra xem đơn hàng đã được giao chưa
                if (order.Status == "Delivered")
                {
                    TempData["ErrorMessage"] = "Không thể hủy đơn hàng đã được giao.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }

                // Cập nhật trạng thái
                order.Status = "Cancelled";
                
                // Nếu đã thanh toán, tự động refund
                if (order.PaymentStatus == "Paid")
                {
                    order.PaymentStatus = "Refunded";
                }

                // Thêm lý do hủy vào Notes
                if (!string.IsNullOrWhiteSpace(cancelReason))
                {
                    var existingNotes = string.IsNullOrWhiteSpace(order.Notes) ? "" : order.Notes + "\n";
                    order.Notes = existingNotes + $"[Hủy đơn hàng - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}]: {cancelReason}";
                }
                else
                {
                    var existingNotes = string.IsNullOrWhiteSpace(order.Notes) ? "" : order.Notes + "\n";
                    order.Notes = existingNotes + $"[Hủy đơn hàng - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}]: Đơn hàng đã được hủy bởi admin.";
                }

                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Send email notification to customer
                var orderWithUser = await _context.Orders
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (orderWithUser?.User != null && !string.IsNullOrWhiteSpace(orderWithUser.User.Email))
                {
                    try
                    {
                        var customerName = !string.IsNullOrWhiteSpace(orderWithUser.User.FirstName) 
                            ? $"{orderWithUser.User.FirstName} {orderWithUser.User.LastName}".Trim()
                            : orderWithUser.User.Username;

                        await _emailService.SendOrderStatusUpdateAsync(
                            orderWithUser.User.Email,
                            customerName,
                            orderWithUser.OrderNumber,
                            "Cancelled",
                            orderWithUser.Total,
                            orderWithUser.ShippingAddress
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send order cancellation email for order {OrderId}", id);
                        // Don't fail the request if email fails, just log it
                    }
                }

                TempData["SuccessMessage"] = $"Đơn hàng '{order.OrderNumber}' đã được hủy thành công.";
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", id);
                TempData["ErrorMessage"] = $"Lỗi khi hủy đơn hàng: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }

        // Helper method
        private async Task<bool> OrderExists(int id)
        {
            return await _context.Orders.AnyAsync(e => e.OrderId == id);
        }
    }
}

