using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Website_Cosmetics.Attributes;
using Website_Cosmetics.Areas.Admin.ViewModels;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;

namespace Website_Cosmetics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [RequireAnyRoleOrPermission("Admin", "Staff", "Admin.Dashboard.View", "Staff.Dashboard.View")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Dashboard/
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var tomorrow = today.AddDays(1);

            var viewModel = new DashboardViewModel();

            // Revenue Today
            var revenueToday = await _context.Orders
                .Where(o => o.CreatedAt >= today && o.CreatedAt < tomorrow)
                .SumAsync(o => (decimal?)o.Total) ?? 0;
            viewModel.RevenueToday = revenueToday;

            // Revenue Yesterday
            var revenueYesterday = await _context.Orders
                .Where(o => o.CreatedAt >= yesterday && o.CreatedAt < today)
                .SumAsync(o => (decimal?)o.Total) ?? 0;
            viewModel.RevenueYesterday = revenueYesterday;

            // Calculate revenue change percent
            if (revenueYesterday > 0)
            {
                viewModel.RevenueChangePercent = ((revenueToday - revenueYesterday) / revenueYesterday) * 100;
            }
            else if (revenueToday > 0)
            {
                viewModel.RevenueChangePercent = 100; // 100% increase from 0
            }

            // Orders Today
            var ordersToday = await _context.Orders
                .Where(o => o.CreatedAt >= today && o.CreatedAt < tomorrow)
                .CountAsync();
            viewModel.TotalOrders = ordersToday;

            // Orders Yesterday
            var ordersYesterday = await _context.Orders
                .Where(o => o.CreatedAt >= yesterday && o.CreatedAt < today)
                .CountAsync();
            viewModel.OrdersYesterday = ordersYesterday;

            // Calculate orders change percent (comparing today vs yesterday)
            if (ordersYesterday > 0)
            {
                viewModel.OrdersChangePercent = ((ordersToday - ordersYesterday) / (decimal)ordersYesterday) * 100;
            }
            else if (ordersToday > 0)
            {
                viewModel.OrdersChangePercent = 100;
            }

            // New Users Today (with role "User")
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
            if (userRole != null)
            {
                var newUsersToday = await _context.Users
                    .Where(u => u.CreatedAt >= today && u.CreatedAt < tomorrow)
                    .Join(_context.UserRoles,
                        u => u.UserId,
                        ur => ur.UserId,
                        (u, ur) => new { u, ur })
                    .Where(x => x.ur.RoleId == userRole.RoleId)
                    .CountAsync();
                viewModel.NewUsersToday = newUsersToday;

                // New Users Yesterday
                var newUsersYesterday = await _context.Users
                    .Where(u => u.CreatedAt >= yesterday && u.CreatedAt < today)
                    .Join(_context.UserRoles,
                        u => u.UserId,
                        ur => ur.UserId,
                        (u, ur) => new { u, ur })
                    .Where(x => x.ur.RoleId == userRole.RoleId)
                    .CountAsync();
                viewModel.NewUsersYesterday = newUsersYesterday;

                // Calculate new users change percent
                if (newUsersYesterday > 0)
                {
                    viewModel.NewUsersChangePercent = ((newUsersToday - newUsersYesterday) / (decimal)newUsersYesterday) * 100;
                }
                else if (newUsersToday > 0)
                {
                    viewModel.NewUsersChangePercent = 100;
                }
            }

            // Low Stock Count (variants with stock < LowStockThreshold)
            var lowStockCount = await _context.ProductVariants
                .Where(v => (v.IsActive == true || v.IsActive == null) && v.Stock < v.LowStockThreshold)
                .CountAsync();
            viewModel.LowStockCount = lowStockCount;

            // Recent Orders (last 10 orders)
            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .ToListAsync();
            viewModel.RecentOrders = recentOrders;

            // Monthly Revenue Chart Data (last 30 days)
            var startDate = today.AddDays(-29); // Last 30 days including today
            var monthlyRevenue = await _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt < tomorrow)
                .ToListAsync();

            // Group by date and sum revenue (client-side to avoid SQL translation issues)
            var revenueByDate = monthlyRevenue
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.Total)
                })
                .ToDictionary(x => x.Date, x => x.Revenue);

            // Fill in missing days with 0 revenue and format dates
            var allDates = Enumerable.Range(0, 30)
                .Select(i => startDate.AddDays(i))
                .ToList();

            viewModel.MonthlyRevenue = allDates.Select(date => new DailyRevenue
            {
                Date = date.ToString("MM/dd"),
                Revenue = revenueByDate.ContainsKey(date) ? revenueByDate[date] : 0
            }).ToList();

            return View(viewModel);
        }
    }
}
