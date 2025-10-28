using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Website_Cosmetics.Models;
using Website_Cosmetics.Repositories;

namespace Website_Cosmetics.Controllers
{
    public class HomeController : Controller   {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _productRepository;

        public HomeController(ILogger<HomeController> logger, IProductRepository productRepository)
        {
            _logger = logger;
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index()
        {
            // Get Most Loved and On Sale products list
            var mostLovedProducts = await _productRepository.GetMostLovedAsync(8);
            var onSaleProducts = await _productRepository.GetOnSaleAsync(8);

            // Pass data to ViewData so view can use it
            ViewData["MostLovedProducts"] = mostLovedProducts;
            ViewData["OnSaleProducts"] = onSaleProducts;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
