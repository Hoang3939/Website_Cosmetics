using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Website_Cosmetics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Staff,Admin")]
    public class StaffDashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
