using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pharmaflow7.Data;
using Pharmaflow7.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pharmaflow7.Controllers
{
    [Authorize(Roles = "distributor")]
    public class DriverController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DriverController> _logger;

        public DriverController(AppDbContext context, UserManager<ApplicationUser> userManager, ILogger<DriverController> logger) : base(userManager)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> ManageDrivers()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "distributor")
                {
                    _logger.LogWarning("Unauthorized access attempt to ManageDrivers.");
                    return RedirectToAction("Login", "Auth");
                }

                var drivers = await _context.Drivers
                    .Where(d => d.DistributorId == user.Id)
                    .Include(d => d.ApplicationUser)
                    .ToListAsync();

                return View(drivers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading ManageDrivers for distributor {DistributorId}", User?.Identity?.Name);
                return StatusCode(500, "An error occurred while loading drivers.");
            }
        }

        [HttpGet]
        public IActionResult AddDriver()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDriver(AddDriverViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "distributor")
                {
                    _logger.LogWarning("Unauthorized access attempt to AddDriver.");
                    return RedirectToAction("Login", "Auth");
                }

                if (ModelState.IsValid)
                {
                    // Check for existing email
                    if (await _userManager.FindByEmailAsync(model.Email) != null)
                    {
                        ModelState.AddModelError("Email", "البريد الإلكتروني مستخدم بالفعل.");
                        return View(model);
                    }

                    // Check for duplicate LicenseNumber or NationalId
                    if (await _context.Drivers.AnyAsync(d => d.LicenseNumber == model.LicenseNumber))
                    {
                        ModelState.AddModelError("LicenseNumber", "رقم الرخصة موجود بالفعل.");
                    }
                    if (await _context.Drivers.AnyAsync(d => d.NationalId == model.NationalId))
                    {
                        ModelState.AddModelError("NationalId", "رقم البطاقة الشخصية موجود بالفعل.");
                    }
                    if (!ModelState.IsValid)
                    {
                        return View(model);
                    }

                    // Create new ApplicationUser
                    var driverUser = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        FullName = model.FullName,
                        ContactNumber = model.ContactNumber,
                        RoleType = "driver"
                    };

                    var result = await _userManager.CreateAsync(driverUser, "TempPassword123!");
                    if (result.Succeeded)
                    {
                        // Create new Driver
                        var driver = new Driver
                        {
                            // Id is auto-incremented by the database
                            ApplicationUserId = driverUser.Id,
                            LicenseNumber = model.LicenseNumber,
                            NationalId = model.NationalId,
                            DistributorId = user.Id, // Ensure DistributorId is set
                            DateHired = DateTime.Now
                        };

                        _context.Drivers.Add(driver);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation("Driver {DriverId} added by distributor {DistributorId}", driver.Id, user.Id);
                        return RedirectToAction("ManageDrivers");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(model);
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding driver for distributor {DistributorId}", User?.Identity?.Name);
                ModelState.AddModelError("", $"An error occurred while adding the driver: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditDriver(int id) // Changed to int
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "distributor")
                {
                    _logger.LogWarning("Unauthorized access attempt to EditDriver.");
                    return RedirectToAction("Login", "Auth");
                }

                var driver = await _context.Drivers
                    .Where(d => d.Id == id && d.DistributorId == user.Id)
                    .Include(d => d.ApplicationUser)
                    .FirstOrDefaultAsync();

                if (driver == null)
                {
                    return NotFound();
                }

                var model = new DriverViewModel
                {
                    Id = driver.Id,
                    FullName = driver.ApplicationUser.FullName,
                    Email = driver.ApplicationUser.Email,
                    ContactNumber = driver.ApplicationUser.ContactNumber,
                    LicenseNumber = driver.LicenseNumber,
                    NationalId = driver.NationalId,
                    DateHired = driver.DateHired
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading EditDriver for driver {DriverId}", id);
                return StatusCode(500, "An error occurred while loading driver data.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDriver(DriverViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "distributor")
                {
                    _logger.LogWarning("Unauthorized access attempt to EditDriver.");
                    return RedirectToAction("Login", "Auth");
                }

                var driver = await _context.Drivers
                    .Where(d => d.Id == model.Id && d.DistributorId == user.Id)
                    .Include(d => d.ApplicationUser)
                    .FirstOrDefaultAsync();

                if (driver == null)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    // Update driver data
                    driver.LicenseNumber = model.LicenseNumber;
                    driver.NationalId = model.NationalId;

                    // Update ApplicationUser data
                    driver.ApplicationUser.FullName = model.FullName;
                    driver.ApplicationUser.ContactNumber = model.ContactNumber;
                    driver.ApplicationUser.Email = model.Email;

                    _context.Drivers.Update(driver);
                    await _userManager.UpdateAsync(driver.ApplicationUser);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Driver {DriverId} updated by distributor {DistributorId}", driver.Id, user.Id);
                    return RedirectToAction("ManageDrivers");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating driver {DriverId}", model.Id);
                ModelState.AddModelError("", "An error occurred while updating the driver.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDriver(int id) // Changed to int
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "distributor")
                {
                    _logger.LogWarning("Unauthorized access attempt to DeleteDriver.");
                    return Json(new { success = false, message = "Unauthorized access." });
                }

                var driver = await _context.Drivers
                    .Where(d => d.Id == id && d.DistributorId == user.Id)
                    .Include(d => d.ApplicationUser)
                    .FirstOrDefaultAsync();

                if (driver == null)
                {
                    return Json(new { success = false, message = "Driver not found." });
                }

                // Check for active shipments
                var hasShipments = await _context.Shipments
                    .AnyAsync(s => s.DriverId == driver.Id);

                if (hasShipments)
                {
                    return Json(new { success = false, message = "Cannot delete driver with active shipments." });
                }

                // Delete driver and associated user
                _context.Drivers.Remove(driver);
                await _userManager.DeleteAsync(driver.ApplicationUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Driver {DriverId} deleted by distributor {DistributorId}", driver.Id, user.Id);
                return Json(new { success = true, message = "Driver deleted successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting driver {DriverId}", id);
                return Json(new { success = false, message = "An error occurred while deleting the driver." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDrivers()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "distributor")
                {
                    return Json(new { success = false, message = "Unauthorized access." });
                }

                var drivers = await _context.Drivers
                    .Where(d => d.DistributorId == user.Id)
                    .Include(d => d.ApplicationUser)
                    .Select(d => new
                    {
                        Id = d.Id, // int
                        fullName = d.ApplicationUser.FullName,
                        licenseNumber = d.LicenseNumber,
                        contactNumber = d.ApplicationUser.ContactNumber
                    })
                    .ToListAsync();

                return Json(drivers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving drivers for distributor {DistributorId}", User?.Identity?.Name);
                return Json(new { success = false, message = "An error occurred while retrieving drivers." });
            }
        }
    }
}