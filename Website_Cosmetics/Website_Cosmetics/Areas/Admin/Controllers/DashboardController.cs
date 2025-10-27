using Microsoft.AspNetCore.Mvc;

namespace YourProjectNamespace.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        // GET: /Admin/Dashboard/
        public IActionResult Index()
        {
            return View();
        }
    }
}
