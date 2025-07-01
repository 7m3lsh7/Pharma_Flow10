using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Pharmaflow7.Data;
using Pharmaflow7.Hubs;
using Pharmaflow7.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pharmaflow7.Controllers
{
    public class DriverController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DriverController> _logger;
        private readonly IHubContext<TrackingHub> _hubContext;

        public DriverController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<DriverController> logger,
            IHubContext<TrackingHub> hubContext)
            : base(userManager, logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _hubContext = hubContext;
        }

        [Authorize(Roles = "driver")]
        [HttpGet]
        public async Task<IActionResult> DriverShipments()
        {
            try
            {
                _logger.LogInformation("Attempting to access DriverShipments. IsAuthenticated: {IsAuthenticated}, User: {UserName}", User.Identity.IsAuthenticated, User.Identity.Name);
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("Unauthorized access attempt to DriverShipments. User is null. ClaimsPrincipal: {UserName}", User.Identity.Name);
                    return Unauthorized();
                }

                _logger.LogInformation("User {UserId} authenticated with RoleType: {RoleType}", user.Id, user.RoleType);
                var isInRole = await _userManager.IsInRoleAsync(user, "driver");
                if (!isInRole)
                {
                    _logger.LogWarning("User {UserId} does not have the 'driver' role.", user.Id);
                    return Unauthorized();
                }

                var driver = await _context.Drivers
                    .FirstOrDefaultAsync(d => d.ApplicationUserId == user.Id);
                if (driver == null)
                {
                    _logger.LogWarning("No driver record found for user {UserId}", user.Id);
                    // رجّع فيو بدل 401
                    return View("NoDriverProfile", new { Message = "Your driver profile is not set up. Please contact your distributor or complete your profile." });
                }

                var shipments = await _context.Shipments
                    .Where(s => s.DriverId == driver.Id)
                    .Include(s => s.Product)
                    .Include(s => s.Store)
                    .Include(s => s.Driver)
                    .Select(s => new ShipmentViewModel
                    {
                        Id = s.Id,
                        ProductName = s.Product != null ? s.Product.Name : "Unknown",
                        Destination = s.Destination,
                        Status = s.Status,
                        StoreAddress = s.Store != null ? s.Store.StoreAddress : "Not Assigned",
                        IsAcceptedByDistributor = s.IsAcceptedByDistributor,
                        DriverName = s.Driver != null ? s.Driver.FullName : "Not Assigned",
                        DriverId = s.DriverId,
                        Latitude = _context.VehicleLocations
      .Where(vl => vl.ShipmentId == s.Id)
      .OrderByDescending(vl => vl.Timestamp)
      .Select(vl => (double?)vl.Latitude)
      .FirstOrDefault(),
                        Longitude = _context.VehicleLocations
      .Where(vl => vl.ShipmentId == s.Id)
      .OrderByDescending(vl => vl.Timestamp)
      .Select(vl => (double?)vl.Longitude)
      .FirstOrDefault()
                    })
                    .ToListAsync();

                var notifications = await _context.Notifications
                    .Where(n => n.UserId == user.Id && !n.IsRead)
                    .ToListAsync();
                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                }
                await _context.SaveChangesAsync();

                return View(shipments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading DriverShipments for driver {DriverId}", User?.Identity?.Name);
                return StatusCode(500, "An error occurred while loading shipments.");
            }
        }

        // باقي الأكشنات (UpdateVehicleLocation, ManageDrivers, إلخ) تبقى زي ما هي
        [Authorize(Roles = "driver")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateVehicleLocation(int shipmentId, decimal latitude, decimal longitude)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _logger.LogWarning("Invalid model state for UpdateVehicleLocation: {Errors}", string.Join(", ", errors));
                    return Json(new { success = false, message = "Invalid input data: " + string.Join(", ", errors) });
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("Unauthorized access attempt to UpdateVehicleLocation by unauthenticated user.");
                    return Json(new { success = false, message = "Unauthorized access." });
                }

                var driver = await _context.Drivers
                    .FirstOrDefaultAsync(d => d.ApplicationUserId == user.Id);
                if (driver == null)
                {
                    _logger.LogWarning("No driver record found for user {UserId}", user.Id);
                    return Json(new { success = false, message = "Driver not found." });
                }

                var shipment = await _context.Shipments
                    .FirstOrDefaultAsync(s => s.Id == shipmentId && s.DriverId == driver.Id);
                if (shipment == null)
                {
                    _logger.LogWarning("Shipment not found for ID: {ShipmentId}, DriverId: {DriverId}", shipmentId, driver.Id);
                    return Json(new { success = false, message = "Shipment not found." });
                }

                if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
                {
                    _logger.LogWarning("Invalid coordinates for shipment {ShipmentId}: Latitude={Latitude}, Longitude={Longitude}", shipmentId, latitude, longitude);
                    return Json(new { success = false, message = "Invalid latitude or longitude values." });
                }

                var location = new VehicleLocation
                {
                    ShipmentId = shipmentId,
                    Latitude = latitude,
                    Longitude = longitude,
                    Timestamp = DateTime.UtcNow
                };
                _context.VehicleLocations.Add(location);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Location updated for shipment {ShipmentId} to ({Latitude}, {Longitude}) by driver {DriverId}", shipmentId, latitude, longitude, driver.Id);

                await _hubContext.Clients.Group($"shipment_{shipmentId}").SendAsync("ReceiveLocationUpdate", shipmentId, (double)latitude, (double)longitude, shipment.Status);

                return Json(new { success = true, message = "Location updated successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location for shipment {ShipmentId}. Details: {Message}, InnerException: {InnerException}", shipmentId, ex.Message, ex.InnerException?.Message);
                return Json(new { success = false, message = $"An error occurred while updating the location: {ex.Message}" });
            }
        }

        // باقي الأكشنات (ManageDrivers, AddDriver, EditDriver, DeleteDriver, GetDrivers) تبقى زي ما هي
        [Authorize(Roles = "distributor")]
        [HttpGet]
        public async Task<IActionResult> ManageDrivers()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("Unauthorized access attempt to ManageDrivers.");
                    return Unauthorized();
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

        [Authorize(Roles = "distributor")]
        [HttpGet]
        public IActionResult AddDriver()
        {
            return View();
        }

        [Authorize(Roles = "distributor")]
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
                    if (await _userManager.FindByEmailAsync(model.Email) != null)
                    {
                        ModelState.AddModelError("Email", "البريد الإلكتروني مستخدم بالفعل.");
                        return View(model);
                    }

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

                    var driverUser = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        FullName = model.FullName,
                        ContactNumber = model.ContactNumber,
                        RoleType = "driver"
                    };

                    var result = await _userManager.CreateAsync(driverUser, model.Password); // استخدام كلمة المرور من الفورم
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(driverUser, "driver");

                        var driver = new Driver
                        {
                            ApplicationUserId = driverUser.Id,
                            LicenseNumber = model.LicenseNumber,
                            NationalId = model.NationalId,
                            FullName = model.FullName,
                            ContactNumber = model.ContactNumber,
                            DistributorId = user.Id,
                            DateHired = DateTime.Now
                        };

                        _context.Drivers.Add(driver);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation("Driver {DriverId} added by distributor {DistributorId}", driver.Id, user.Id);
                        // إضافة رسالة نجاح
                        TempData["SuccessMessage"] = $"تم إضافة السائق {model.FullName} بنجاح. الإيميل: {model.Email}, كلمة المرور: {model.Password}";
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
                ModelState.AddModelError("", $"حدث خطأ أثناء إضافة السائق: {ex.Message}");
                return View(model);
            }
        }

        [Authorize(Roles = "distributor")]
        [HttpGet]
        public async Task<IActionResult> EditDriver(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("Unauthorized access attempt to EditDriver.");
                    return Unauthorized();
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
                    FullName = driver.FullName,
                    Email = driver.ApplicationUser.Email,
                    ContactNumber = driver.ContactNumber,
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

        [Authorize(Roles = "distributor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDriver(DriverViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("Unauthorized access attempt to EditDriver.");
                    return Unauthorized();
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
                    driver.LicenseNumber = model.LicenseNumber;
                    driver.NationalId = model.NationalId;
                    driver.FullName = model.FullName;
                    driver.ContactNumber = model.ContactNumber;

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

        [Authorize(Roles = "distributor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDriver(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
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

                var hasShipments = await _context.Shipments
                    .AnyAsync(s => s.DriverId == driver.Id);

                if (hasShipments)
                {
                    return Json(new { success = false, message = "Cannot delete driver with active shipments." });
                }

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

        [Authorize(Roles = "distributor")]
        [HttpGet]
        public async Task<IActionResult> GetDrivers()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Unauthorized access." });
                }

                var drivers = await _context.Drivers
                    .Where(d => d.DistributorId == user.Id)
                    .Include(d => d.ApplicationUser)
                    .Select(d => new
                    {
                        Id = d.Id,
                        fullName = d.FullName,
                        licenseNumber = d.LicenseNumber,
                        contactNumber = d.ContactNumber
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