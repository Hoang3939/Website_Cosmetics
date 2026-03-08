using Microsoft.AspNetCore.Mvc;
using Website_Cosmetics.Services;
using System.Security.Claims;

namespace Website_Cosmetics.Areas.Admin.ViewComponents
{
    public class AdminSidebarViewComponent : ViewComponent
    {
        private readonly IAuthorizationService _authService;

        public AdminSidebarViewComponent(IAuthorizationService authService)
        {
            _authService = authService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userIdClaim = UserClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            var userId = userIdClaim != null && int.TryParse(userIdClaim.Value, out var id) ? id : 0;

            if (userId == 0)
            {
                return View(new SidebarViewModel());
            }

            // Check permissions for each menu item
            var isAdmin = await _authService.HasRoleAsync(userId, "Admin");
            
            var viewModel = new SidebarViewModel
            {
                HasDashboardAccess = isAdmin ||
                    await _authService.HasPermissionAsync(userId, "Admin.Dashboard.View") ||
                    await _authService.HasPermissionAsync(userId, "Staff.Dashboard.View") ||
                    await _authService.HasRoleAsync(userId, "Staff"),
                HasProductsAccess = isAdmin ||
                    await _authService.HasPermissionAsync(userId, "Product.Manage"),
                HasCategoriesAccess = isAdmin ||
                    await _authService.HasPermissionAsync(userId, "Category.Manage"),
                HasBrandsAccess = isAdmin ||
                    await _authService.HasPermissionAsync(userId, "Brand.Manage"),
                HasOrdersAccess = isAdmin ||
                    await _authService.HasPermissionAsync(userId, "Order.Manage"),
                HasCustomersAccess = isAdmin ||
                    await _authService.HasPermissionAsync(userId, "User.Manage"),
                HasStaffAccess = isAdmin,
                HasPermissionsAccess = isAdmin
            };

            return View(viewModel);
        }
    }

    public class SidebarViewModel
    {
        public bool HasDashboardAccess { get; set; }
        public bool HasProductsAccess { get; set; }
        public bool HasCategoriesAccess { get; set; }
        public bool HasBrandsAccess { get; set; }
        public bool HasOrdersAccess { get; set; }
        public bool HasCustomersAccess { get; set; }
        public bool HasStaffAccess { get; set; }
        public bool HasPermissionsAccess { get; set; }
    }
}

