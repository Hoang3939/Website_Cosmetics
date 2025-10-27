using Microsoft.AspNetCore.Mvc;
using Website_Cosmetics.Attributes;

namespace Website_Cosmetics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [RequirePermission("Staff.Dashboard.View")]
    public class StaffDashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
