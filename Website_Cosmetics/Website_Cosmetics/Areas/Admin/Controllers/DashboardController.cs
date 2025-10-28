using Microsoft.AspNetCore.Mvc;
using Website_Cosmetics.Attributes;

namespace Website_Cosmetics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [RequirePermission("Admin.Dashboard.View")]
    public class DashboardController : Controller
    {
        // GET: /Admin/Dashboard/
        public IActionResult Index()
        {
            return View();
        }
    }
}
