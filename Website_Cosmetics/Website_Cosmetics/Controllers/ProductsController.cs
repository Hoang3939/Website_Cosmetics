using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Website_Cosmetics.Repositories;
using Website_Cosmetics.Models;
using Microsoft.EntityFrameworkCore;

namespace Website_Cosmetics.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductRepository _productRepository;

        public ProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // GET: /Products
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products); 
        }

        // GET: /Products/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(); 
            }
            return View(product); 
        }
        // 1. CREATE (TẠO MỚI)
        // GET: /Products/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                await _productRepository.AddAsync(product);

                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // 2. EDIT (CHỈNH SỬA)
        // GET: /Products/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: /Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _productRepository.UpdateAsync(product);
                }
                catch (DbUpdateConcurrencyException)
                {

                    if (await _productRepository.GetByIdAsync(id) == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // 3. DELETE (XÓA)
        // GET: /Products/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: /Products/Delete/5
        [HttpPost, ActionName("Delete")] 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _productRepository.DeleteAsync(id);

            return RedirectToAction(nameof(Index));
        }
    }
}
   