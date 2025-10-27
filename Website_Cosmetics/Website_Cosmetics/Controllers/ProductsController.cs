using Microsoft.AspNetCore.Mvc;

namespace Website_Cosmetics.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            return View();
        }

        public IActionResult Category(string category)
        {
            return View();
        }
    }
}
