using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Website_Cosmetics.Models;
using Website_Cosmetics.Repositories;

namespace Website_Cosmetics.Controllers
{
    public class CategoriesController : Controller
    {
        // 1. Inject Interface của Category
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        // 2. Lấy tất cả (GET: /Categories)
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories); 
        }

        // 3. Lấy chi tiết (GET: /Categories/Details/5)
        public async Task<IActionResult> Details(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // 4. Tạo mới (GET: /Categories/Create)
        public IActionResult Create()
        {
            return View();
        }

        // 5. Tạo mới (POST: /Categories/Create)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                await _categoryRepository.AddAsync(category);
                return RedirectToAction(nameof(Index)); // Quay về trang danh sách
            }
            return View(category); // Hiển thị lại form nếu có lỗi
        }

        // 6. Chỉnh sửa (GET: /Categories/Edit/5)
        public async Task<IActionResult> Edit(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // 7. Chỉnh sửa (POST: /Categories/Edit/5)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _categoryRepository.UpdateAsync(category);
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // 8. Xóa (GET: /Categories/Delete/5)
        public async Task<IActionResult> Delete(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // 9. Xóa (POST: /Categories/Delete/5)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _categoryRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}

