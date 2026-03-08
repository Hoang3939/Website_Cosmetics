using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;

namespace Website_Cosmetics.Controllers
{
    public class AddressController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AddressController> _logger;

        public AddressController(ApplicationDbContext context, ILogger<AddressController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Address
        public async Task<IActionResult> Index()
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var addresses = await _context.UserAddresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(addresses);
        }

        // GET: /Address/Create
        [HttpGet]
        public IActionResult Create()
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        // POST: /Address/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserAddress address)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            // Set UserId before validation (it's not in the form)
            address.UserId = userId;
            
            // Remove fields that shouldn't be validated from form
            // Remove both "UserId" and "User" (navigation property)
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("AddressId");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("UpdatedAt");

            // Set default country if empty
            if (string.IsNullOrWhiteSpace(address.Country))
            {
                address.Country = "United States";
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // UserId already set above
                    address.CreatedAt = DateTime.UtcNow;
                    address.UpdatedAt = DateTime.UtcNow;

                    // If this is set as default, unset other defaults
                    if (address.IsDefault)
                    {
                        var existingDefaults = await _context.UserAddresses
                            .Where(a => a.UserId == userId && a.IsDefault)
                            .ToListAsync();
                        
                        foreach (var existing in existingDefaults)
                        {
                            existing.IsDefault = false;
                            existing.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                    // If no default exists, set this as default
                    else
                    {
                        var hasDefault = await _context.UserAddresses
                            .AnyAsync(a => a.UserId == userId && a.IsDefault);
                        
                        if (!hasDefault)
                        {
                            address.IsDefault = true;
                        }
                    }

                    _context.UserAddresses.Add(address);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Address added successfully.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating address for user {UserId}", userId);
                    TempData["ErrorMessage"] = "An error occurred while saving the address. Please try again.";
                    return View(address);
                }
            }

            // Log validation errors for debugging
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("Validation errors when creating address: {Errors}", string.Join(", ", errors));
            }

            return View(address);
        }

        // GET: /Address/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var address = await _context.UserAddresses
                .FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == userId);

            if (address == null)
            {
                return NotFound();
            }

            return View(address);
        }

        // POST: /Address/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserAddress address)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (id != address.AddressId)
            {
                return NotFound();
            }

            var existingAddress = await _context.UserAddresses
                .FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == userId);

            if (existingAddress == null)
            {
                return NotFound();
            }

            // Remove fields that shouldn't be validated from form
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("AddressId");
            ModelState.Remove("CreatedAt");

            if (ModelState.IsValid)
            {
                // If this is set as default, unset other defaults
                if (address.IsDefault && !existingAddress.IsDefault)
                {
                    var otherDefaults = await _context.UserAddresses
                        .Where(a => a.UserId == userId && a.AddressId != id && a.IsDefault)
                        .ToListAsync();
                    
                    foreach (var other in otherDefaults)
                    {
                        other.IsDefault = false;
                        other.UpdatedAt = DateTime.UtcNow;
                    }
                }

                existingAddress.FullName = address.FullName;
                existingAddress.PhoneNumber = address.PhoneNumber;
                existingAddress.AddressLine1 = address.AddressLine1;
                existingAddress.AddressLine2 = address.AddressLine2;
                existingAddress.City = address.City;
                existingAddress.State = address.State;
                existingAddress.PostalCode = address.PostalCode;
                existingAddress.Country = address.Country;
                existingAddress.IsDefault = address.IsDefault;
                existingAddress.UpdatedAt = DateTime.UtcNow;

                _context.UserAddresses.Update(existingAddress);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Address updated successfully.";
                return RedirectToAction("Index");
            }

            return View(address);
        }

        // POST: /Address/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var address = await _context.UserAddresses
                .FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == userId);

            if (address == null)
            {
                return NotFound();
            }

            _context.UserAddresses.Remove(address);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Address deleted successfully.";
            return RedirectToAction("Index");
        }

        // POST: /Address/SetDefault/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDefault(int id)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return Json(new { success = false, message = "Please login" });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var address = await _context.UserAddresses
                .FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == userId);

            if (address == null)
            {
                return Json(new { success = false, message = "Address not found" });
            }

            // Unset other defaults
            var otherDefaults = await _context.UserAddresses
                .Where(a => a.UserId == userId && a.AddressId != id && a.IsDefault)
                .ToListAsync();
            
            foreach (var other in otherDefaults)
            {
                other.IsDefault = false;
                other.UpdatedAt = DateTime.UtcNow;
            }

            address.IsDefault = true;
            address.UpdatedAt = DateTime.UtcNow;

            _context.UserAddresses.Update(address);
            if (otherDefaults.Any())
            {
                _context.UserAddresses.UpdateRange(otherDefaults);
            }
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Default address updated" });
        }
    }
}

