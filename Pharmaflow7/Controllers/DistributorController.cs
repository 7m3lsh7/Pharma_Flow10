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
    [Authorize(Roles = "distributor")]
    public class DistributorController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DistributorController> _logger;
        private readonly IHubContext<TrackingHub> _hubContext;

        public DistributorController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<DistributorController> logger,
            IHubContext<TrackingHub> hubContext)
            : base(userManager, logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _hubContext = hubContext;
        }
        [Authorize(Roles = "distributor")]
        public async Task<IActionResult> Track()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Unauthorized access attempt to TrackShipments.");
                return RedirectToAction("Login", "Auth");
            }

            var driverIds = await _context.Drivers
                .Where(d => d.DistributorId == user.Id)
                .Select(d => d.Id) // Id is int, no need for .HasValue
                .ToListAsync();

            var shipments = await _context.Shipments
                .Include(s => s.Driver)
                .Include(s => s.Product)
                .Include(s => s.Store)
                .Where(s => s.DriverId.HasValue && driverIds.Contains(s.DriverId.Value))
                .Select(s => new ShipmentViewModel
                {
                    Id = s.Id,
                    ProductName = s.Product.Name,
                    Destination = s.Destination,
                    Status = s.Status,
                    StoreAddress = s.Store.StoreAddress, // Changed from Address to Name (or update Store entity)
                    DriverFullName = s.Driver != null ? s.Driver.FullName : "غير محدد",
                    Latitude = _context.VehicleLocations
                        .Where(vl => vl.ShipmentId == s.Id)
                        .OrderByDescending(vl => vl.Timestamp)
                        .Select(vl => (double?)vl.Latitude)
                        .FirstOrDefault(),
                    Longitude = _context.VehicleLocations
                        .Where(vl => vl.ShipmentId == s.Id)
                        .OrderByDescending(vl => vl.Timestamp)
                        .Select(vl => (double?)vl.Longitude)
                        .FirstOrDefault(),
                    DestinationLatitude = (double?)s.DestinationLatitude,
                    DestinationLongitude = (double?)s.DestinationLongitude,
                    DistributorId = s.DistributorId,
                    IsAcceptedByDistributor = s.IsAcceptedByDistributor
                })
                .ToListAsync();

            return View(shipments);
        }
        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "distributor")
                {
                    _logger.LogWarning("Unauthorized access attempt to GetDashboardData.");
                    return Unauthorized();
                }

                var shipments = await _context.Shipments
                    .Where(s => s.DistributorId == user.Id)
                    .ToListAsync();

                var inventoryCount = shipments
                    .Where(s => s.Status == "Delivered")
                    .Sum(s => s.Quantity ?? 0);

                var incomingShipments = shipments
                    .Count(s => s.Status == "In Transit" && s.IsAcceptedByDistributor == true);

                var outgoingShipments = shipments
                    .Count(s => s.Status == "In Transit" && s.IsAcceptedByDistributor == false);

                var data = new
                {
                    InventoryCount = inventoryCount,
                    IncomingShipments = incomingShipments,
                    OutgoingShipments = outgoingShipments
                };

                _logger.LogInformation("Dashboard data retrieved for distributor {DistributorId}", user.Id);
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard data for distributor {DistributorId}", User?.Identity?.Name);
                return StatusCode(500, "An error occurred while retrieving dashboard data.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetShipments()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "distributor")
                {
                    _logger.LogWarning("Unauthorized access attempt to GetShipments.");
                    return Unauthorized();
                }

                var shipments = await _context.Shipments
                    .Where(s => s.DistributorId == user.Id)
                    .Include(s => s.Product)
                    .Include(s => s.Store)
                    .Select(s => new
                    {
                        Id = s.Id,
                        Type = s.Product != null ? s.Product.Name : "Unknown",
                        Quantity = s.Quantity ?? 0,
                        Date = s.CreatedAt.HasValue ? s.CreatedAt.Value.ToString("yyyy-MM-dd") : "N/A",
                        Status = s.Status
                    })
                    .ToListAsync();

                _logger.LogInformation("Shipments retrieved for distributor {DistributorId}", user.Id);
                return Json(shipments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shipments for distributor {DistributorId}", User?.Identity?.Name);
                return StatusCode(500, "An error occurred while retrieving shipments.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> TrackShipment()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "distributor")
                {
                    _logger.LogWarning("Unauthorized access attempt to TrackShipment.");
                    return RedirectToAction("Login", "Auth");
                }

                var shipments = await _context.Shipments
                    .Where(s => s.DistributorId == user.Id)
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

                return View(shipments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading TrackShipment for distributor {DistributorId}", User?.Identity?.Name);
                return StatusCode(500, "An error occurred while loading shipments.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptShipment([FromQuery] int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                _logger.LogInformation("AcceptShipment called with ID: {ShipmentId}, User ID: {UserId}", id, user?.Id);
                if (user == null || user.RoleType != "distributor")
                {
                    _logger.LogWarning("Unauthorized access attempt to AcceptShipment.");
                    return Unauthorized();
                }

                var shipment = await _context.Shipments
                    .FirstOrDefaultAsync(s => s.Id == id && s.DistributorId == user.Id);
                if (shipment == null)
                {
                    _logger.LogWarning("Shipment not found for ID: {ShipmentId}, DistributorId: {DistributorId}", id, user.Id);
                    return Json(new { success = false, message = "Shipment not found." });
                }

                shipment.IsAcceptedByDistributor = true;
                shipment.Status = "In Transit";
                _context.Shipments.Update(shipment);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Shipment {ShipmentId} accepted by distributor {DistributorId}", id, user.Id);

                return Json(new { success = true, message = "Shipment accepted successfully! Please assign a driver." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting shipment {ShipmentId}", id);
                return Json(new { success = false, message = "An error occurred while accepting the shipment." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectShipment([FromQuery] int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                _logger.LogInformation("RejectShipment called with ID: {ShipmentId}, User ID: {UserId}", id, user?.Id);
                if (user == null || user.RoleType != "distributor")
                {
                    _logger.LogWarning("Unauthorized access attempt to RejectShipment.");
                    return Unauthorized();
                }

                var shipment = await _context.Shipments
                    .FirstOrDefaultAsync(s => s.Id == id && s.DistributorId == user.Id);
                if (shipment == null)
                {
                    _logger.LogWarning("Shipment not found for ID: {ShipmentId}, DistributorId: {DistributorId}", id, user.Id);
                    return Json(new { success = false, message = "Shipment not found." });
                }

                shipment.IsAcceptedByDistributor = false;
                shipment.Status = "Rejected";
                _context.Shipments.Update(shipment);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Shipment {ShipmentId} rejected by distributor {DistributorId}", id, user.Id);

                return Json(new { success = true, message = "Shipment rejected successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting shipment {ShipmentId}", id);
                return Json(new { success = false, message = "An error occurred while rejecting the shipment." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelivery([FromQuery] int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                _logger.LogInformation("ConfirmDelivery called with ID: {ShipmentId}, User ID: {UserId}", id, user?.Id);
                if (user == null || user.RoleType != "distributor")
                {
                    _logger.LogWarning("Unauthorized access attempt to ConfirmDelivery.");
                    return Unauthorized();
                }

                var shipment = await _context.Shipments
                    .Include(s => s.Driver)
                    .FirstOrDefaultAsync(s => s.Id == id && s.DistributorId == user.Id);
                if (shipment == null)
                {
                    _logger.LogWarning("Shipment not found for ID: {ShipmentId}, DistributorId: {DistributorId}", id, user.Id);
                    return Json(new { success = false, message = "Shipment not found." });
                }

                

                if (shipment.Status != "In Transit" || shipment.IsAcceptedByDistributor != true)
                {
                    _logger.LogWarning("Cannot confirm delivery for ID: {ShipmentId}. Status: {Status}, Accepted: {Accepted}", id, shipment.Status, shipment.IsAcceptedByDistributor);
                    return Json(new { success = false, message = "Cannot confirm delivery for this shipment." });
                }

                shipment.Status = "Delivered";
                _context.Shipments.Update(shipment);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Delivery confirmed for shipment {ShipmentId} by distributor {DistributorId}", id, user.Id);

                if (shipment.DriverId.HasValue)
                {
                    var driver = await _context.Drivers
                        .FirstOrDefaultAsync(d => d.Id == shipment.DriverId.Value);
                    if (driver != null)
                    {
                        var notification = new Notification
                        {
                            UserId = driver.ApplicationUserId, // Use ApplicationUserId
                            Message = $"Shipment {id} delivery confirmed.",
                            ShipmentId = id,
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Notifications.Add(notification);
                        await _context.SaveChangesAsync();
                        await _hubContext.Clients.User(driver.ApplicationUserId).SendAsync("ReceiveNotification", notification.Message, id);
                    }
                }

                return Json(new { success = true, message = "Delivery confirmed successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming delivery for shipment {ShipmentId}", id);
                return Json(new { success = false, message = "An error occurred while confirming delivery." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDriverToShipment(int shipmentId, int driverId) // Changed to int
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "distributor")
                {
                    _logger.LogWarning("Unauthorized access attempt to AssignDriverToShipment.");
                    return Json(new { success = false, message = "Unauthorized access." });
                }

                var shipment = await _context.Shipments
                    .FirstOrDefaultAsync(s => s.Id == shipmentId && s.DistributorId == user.Id);
                if (shipment == null)
                {
                    return Json(new { success = false, message = "Shipment not found." });
                }

                var driver = await _context.Drivers
                    .FirstOrDefaultAsync(d => d.Id == driverId && d.DistributorId == user.Id);
                if (driver == null)
                {
                    return Json(new { success = false, message = "Driver not found or not assigned to your company." });
                }

                shipment.DriverId = driverId;
                shipment.Status = "In Transit";
                _context.Shipments.Update(shipment);

                var notification = new Notification
                {
                    UserId = driver.ApplicationUserId, // Use ApplicationUserId
                    Message = $"You have been assigned to shipment {shipmentId}.",
                    ShipmentId = shipmentId,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.User(driver.ApplicationUserId).SendAsync("ReceiveNotification", notification.Message, shipmentId);

                _logger.LogInformation("Driver {DriverId} assigned to shipment {ShipmentId} by distributor {DistributorId}", driverId, shipmentId, user.Id);
                return Json(new { success = true, message = "Driver assigned successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning driver to shipment {ShipmentId}", shipmentId);
                return Json(new { success = false, message = "An error occurred while assigning the driver." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateShipmentLocation(int id, string address)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "distributor")
                {
                    _logger.LogWarning("Unauthorized access attempt to UpdateShipmentLocation.");
                    return RedirectToAction("Login", "Auth");
                }

                var shipment = await _context.Shipments
                    .Include(s => s.Store)
                    .FirstOrDefaultAsync(s => s.Id == id && s.DistributorId == user.Id);
                if (shipment == null)
                {
                    _logger.LogWarning("Shipment not found for ID: {ShipmentId}, DistributorId: {DistributorId}", id, user.Id);
                    return NotFound();
                }

                if (shipment.Store != null)
                {
                    shipment.Store.StoreAddress = address;
                    _context.Stores.Update(shipment.Store);
                }
                else
                {
                    var newStore = new Store
                    {
                        StoreName = $"Shipment {id} Location",
                        StoreAddress = address,
                        DistributorId = user.Id
                    };
                    _context.Stores.Add(newStore);
                    shipment.StoreId = newStore.Id;
                }

                shipment.Status = "In Transit";
                _context.Shipments.Update(shipment);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Location updated for shipment {ShipmentId} to {Address} by distributor {DistributorId}", id, address, user.Id);

                return Json(new { success = true, message = "Location updated successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location for shipment {ShipmentId}", id);
                return Json(new { success = false, message = "An error occurred while updating the location." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ManageStores()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "distributor")
                {
                    _logger.LogWarning("Unauthorized access attempt to ManageStores.");
                    return RedirectToAction("Login", "Auth");
                }

                var stores = await _context.Stores
                    .Where(s => s.DistributorId == user.Id)
                    .ToListAsync();
                return View(stores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading ManageStores for distributor {DistributorId}", User?.Identity?.Name);
                return StatusCode(500, "An error occurred while loading stores.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddStore()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "distributor")
                {
                    _logger.LogWarning("Unauthorized access attempt to AddStore.");
                    return RedirectToAction("Login", "Auth");
                }

                var stores = await _context.Stores
                    .Where(s => s.DistributorId == user.Id)
                    .ToListAsync();

                var model = new AddStoreViewModel
                {
                    ExistingStores = stores
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading AddStore for distributor {DistributorId}", User?.Identity?.Name);
                return StatusCode(500, "An error occurred while loading the store creation page.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStore(AddStoreViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "distributor")
                {
                    _logger.LogWarning("Unauthorized access attempt to AddStore.");
                    return RedirectToAction("Login", "Auth");
                }

                if (!ModelState.IsValid)
                {
                    model.ExistingStores = await _context.Stores
                        .Where(s => s.DistributorId == user.Id)
                        .ToListAsync();
                    return View(model);
                }

                var store = new Store
                {
                    StoreName = model.NewStore.StoreName,
                    StoreAddress = model.NewStore.StoreAddress,
                    DistributorId = user.Id
                };
                _context.Stores.Add(store);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Store {StoreId} added by distributor {DistributorId}", store.Id, user.Id);

                return RedirectToAction("ManageStores");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding store for distributor {DistributorId}", User?.Identity?.Name ?? "Unknown");
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    model.ExistingStores = await _context.Stores
                        .Where(s => s.DistributorId == user.Id)
                        .ToListAsync();
                }
                else
                {
                    model.ExistingStores = new List<Store>();
                }
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult InventoryManagement()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Report()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetLatestLocation(int shipmentId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "distributor")
                {
                    return Json(new { success = false, message = "Unauthorized access." });
                }

                var location = await _context.VehicleLocations
                    .Where(v => v.ShipmentId == shipmentId && v.Shipment.DistributorId == user.Id)
                    .OrderByDescending(v => v.Timestamp)
                    .Select(v => new { v.Latitude, v.Longitude })
                    .FirstOrDefaultAsync();

                if (location == null)
                {
                    return Json(new { success = false, message = "No location data found." });
                }

                return Json(new { success = true, latitude = location.Latitude, longitude = location.Longitude });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest location for shipment {ShipmentId}", shipmentId);
                return Json(new { success = false, message = "An error occurred while retrieving location." });
            }
        }
    }
}