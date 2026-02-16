using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Pharmaflow7.Data;
using Pharmaflow7.Helpers;
using Pharmaflow7.Hubs;
using Pharmaflow7.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Pharmaflow7.Controllers
{
    [Authorize(Roles = "company")]
    public class CompanyController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<CompanyController> _logger;
        private readonly IHubContext<TrackingHub> _hubContext;

        public CompanyController(AppDbContext context, UserManager<ApplicationUser> userManager,
            IHubContext<TrackingHub> hubContext, SignInManager<ApplicationUser> signInManager, ILogger<CompanyController> logger) : base(userManager, logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _hubContext = hubContext;
        }
      
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CompanyDashboard()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType.ToLower() != "company")
                {
                    _logger.LogWarning("Unauthorized access attempt to Reports.");
                    return RedirectToAction("Login", "Auth");
                }
                var totalProducts = await _context.Products.CountAsync(p => p.CompanyId == user.Id);
                var activeShipments = await _context.Shipments.CountAsync(s => s.CompanyId == user.Id && s.Status != "Delivered");
                var deliveredShipments = await _context.Shipments.CountAsync(s => s.CompanyId == user.Id && s.Status == "Delivered");
                var totalShipments = await _context.Shipments.CountAsync(s => s.CompanyId == user.Id);

                int performanceScore = totalShipments > 0 ? (int)((deliveredShipments / (double)totalShipments) * 100) : 0;

                var shipments = await _context.Shipments
                    .Where(s => s.CompanyId == user.Id && s.Status != "Delivered")
                    .Join(_context.Products, s => s.ProductId, p => p.Id, (s, p) => new { s.Id, ProductName = p.Name, s.Destination, s.Status })
                    .ToListAsync();

                var salesData = await _context.Shipments
                    .Where(s => s.CompanyId == user.Id)
                    .GroupBy(s => s.CreatedDate.Month)
                    .Select(g => new { Month = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(g.Key), Total = g.Count() })
                    .ToListAsync();

                var distributionData = await _context.Shipments
                    .Where(s => s.CompanyId == user.Id)
                    .GroupBy(s => s.Destination)
                    .Select(g => new { Destination = g.Key, Count = g.Count() })
                    .ToListAsync();

                var viewModel = new CompanyDashboardViewModel
                {
                    TotalProducts = totalProducts,
                    ActiveShipments = activeShipments,
                    DeliveredShipments = deliveredShipments,
                    PerformanceScore = performanceScore,
                    Shipments = shipments.Cast<object>().ToList(),
                    SalesData = salesData.Cast<object>().ToList(),
                    DistributionData = distributionData.Cast<object>().ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading CompanyDashboard for user {UserId}", User?.Identity?.Name);
                return StatusCode(500, "An error occurred while loading the dashboard.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ManageProducts()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "company")
                {
                    _logger.LogWarning("Unauthorized access attempt to ManageProducts.");
                    return RedirectToAction("Login", "Auth");
                }

                var products = await _context.Products.Where(p => p.CompanyId == user.Id).ToListAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products for user {UserId}", User?.Identity?.Name);
                return StatusCode(500, "An error occurred while fetching products.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "company")
                {
                    _logger.LogWarning("Unauthorized access attempt to EditProduct.");
                    return RedirectToAction("Login", "Auth");
                }

                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == user.Id);
                if (product == null)
                {
                    return NotFound();
                }

                var model = new ProductViewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    ProductionDate = product.ProductionDate,
                    ExpirationDate = product.ExpirationDate,
                    Description = product.Description
                };

                return PartialView("_EditProductModal", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading EditProduct for product {ProductId}", id);
                return StatusCode(500, "An error occurred while loading the product.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(ProductViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "company")
                {
                    _logger.LogWarning("Unauthorized access attempt to EditProduct.");
                    return RedirectToAction("Login", "Auth");
                }

                if (!ModelState.IsValid)
                {
                    return PartialView("_EditProductModal", model);
                }

                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == model.Id && p.CompanyId == user.Id);
                if (product == null)
                {
                    return NotFound();
                }

                if (model.ProductionDate >= model.ExpirationDate)
                {
                    ModelState.AddModelError("ExpirationDate", "Expiration date must be after production date.");
                    return PartialView("_EditProductModal", model);
                }

                product.Name = model.Name;
                product.ProductionDate = model.ProductionDate;
                product.ExpirationDate = model.ExpirationDate;
                product.Description = model.Description;

                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Product {ProductId} updated by company {CompanyId}", product.Id, user.Id);

                return Json(new { success = true, message = "Product updated successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", model.Id);
                return StatusCode(500, new { success = false, message = "An error occurred while updating the product." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "company")
                {
                    _logger.LogWarning("Unauthorized access attempt to DeleteProduct.");
                    return RedirectToAction("Login", "Auth");
                }

                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == user.Id);
                if (product == null)
                {
                    return Json(new { success = false, message = "Product not found." });
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Product {ProductId} deleted by company {CompanyId}", id, user.Id);

                return Json(new { success = true, productId = id, message = "Product deleted successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return Json(new { success = false, message = "An error occurred while deleting the product." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateShipment()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "company")
                {
                    _logger.LogWarning("Unauthorized access attempt to CreateShipment.");
                    return RedirectToAction("Login", "Auth");
                }

                var model = new ShipmentViewModel
                {
                    Products = await _context.Products.Where(p => p.CompanyId == user.Id).ToListAsync(),
                    Distributors = await _userManager.Users.Where(u => u.RoleType == "distributor").ToListAsync(),
                    Stores = new List<Store>()
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading CreateShipment for user {UserId}", User?.Identity?.Name);
                return StatusCode(500, "An error occurred while loading the shipment creation page.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateShipment(ShipmentViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.RoleType != "company")
            {
                _logger.LogWarning("Unauthorized access attempt to CreateShipment.");
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                _logger.LogInformation("Received shipment data: ProductId={ProductId}, Destination={Destination}, DistributorId={DistributorId}, StoreId={StoreId}, DriverId={DriverId}",
                    model.ProductId, model.Destination, model.DistributorId, model.StoreId, model.DriverId);

                // Remove DriverName from validation since it's not sent from the form
                ModelState.Remove("DriverName");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _logger.LogWarning("Invalid model state for CreateShipment: {Errors}", string.Join(", ", errors));
                    model.Products = await _context.Products.Where(p => p.CompanyId == user.Id).ToListAsync();
                    model.Distributors = await _userManager.Users.Where(u => u.RoleType == "distributor").ToListAsync();
                    model.Stores = string.IsNullOrEmpty(model.DistributorId)
                        ? new List<Store>()
                        : await _context.Stores.Where(s => s.DistributorId == model.DistributorId).ToListAsync();
                    TempData["Error"] = "Please correct the errors and try again: " + string.Join(", ", errors);
                    return View(model);
                }

                // Validate DriverId
                if (model.DriverId.HasValue)
                {
                    var driver = await _context.Drivers
                        .Include(d => d.ApplicationUser)
                        .FirstOrDefaultAsync(d => d.Id == model.DriverId.Value);
                    if (driver == null)
                    {
                        ModelState.AddModelError("DriverId", "Selected driver does not exist.");
                        _logger.LogWarning("Invalid DriverId: {DriverId}", model.DriverId);
                        model.Products = await _context.Products.Where(p => p.CompanyId == user.Id).ToListAsync();
                        model.Distributors = await _userManager.Users.Where(u => u.RoleType == "distributor").ToListAsync();
                        model.Stores = string.IsNullOrEmpty(model.DistributorId)
                            ? new List<Store>()
                            : await _context.Stores.Where(s => s.DistributorId == model.DistributorId).ToListAsync();
                        TempData["Error"] = "Invalid driver selected.";
                        return View(model);
                    }
                    // Optionally set DriverName for display purposes
                    model.DriverName = driver.ApplicationUser?.FullName;
                }

                var shipment = new Shipment
                {
                    ProductId = model.ProductId,
                    Destination = model.Destination,
                    DistributorId = model.DistributorId,
                    StoreId = model.StoreId.HasValue && model.StoreId != 0 ? model.StoreId : null,
                    DriverId = model.DriverId,
                    CompanyId = user.Id,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Shipments.Add(shipment);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Shipment {ShipmentId} created by company {CompanyId} with StoreId={StoreId} and DriverId={DriverId}",
                    shipment.Id, user.Id, shipment.StoreId, shipment.DriverId);

                // Send notification to distributor if assigned
                if (!string.IsNullOrEmpty(model.DistributorId))
                {
                    var notification = new Notification
                    {
                        UserId = model.DistributorId,
                        ShipmentId = shipment.Id,
                        Message = $"New shipment #{shipment.Id} assigned to you.",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    };
                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                    await _hubContext.Clients.User(model.DistributorId).SendAsync("ReceiveNotification", notification.Message, shipment.Id);
                }

                // Send notification to driver if assigned
                if (model.DriverId.HasValue)
                {
                    var driver = await _context.Drivers
                        .Include(d => d.ApplicationUser)
                        .FirstOrDefaultAsync(d => d.Id == model.DriverId.Value);
                    if (driver != null)
                    {
                        var driverNotification = new Notification
                        {
                            UserId = driver.ApplicationUserId,
                            ShipmentId = shipment.Id,
                            Message = $"New shipment #{shipment.Id} assigned to you.",
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false
                        };
                        _context.Notifications.Add(driverNotification);
                        await _context.SaveChangesAsync();
                        await _hubContext.Clients.User(driver.ApplicationUserId).SendAsync("ReceiveNotification", driverNotification.Message, shipment.Id);
                    }
                }

                return RedirectToAction("CompanyDashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating shipment for company {CompanyId}", user?.Id ?? "Unknown");
                model.Products = await _context.Products.Where(p => p.CompanyId == user.Id).ToListAsync();
                model.Distributors = await _userManager.Users.Where(u => u.RoleType == "distributor").ToListAsync();
                model.Stores = string.IsNullOrEmpty(model.DistributorId)
                    ? new List<Store>()
                    : await _context.Stores.Where(s => s.DistributorId == model.DistributorId).ToListAsync();
                TempData["Error"] = "An error occurred while creating the shipment: " + ex.Message;
                return View(model);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetStores(string distributorId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "company")
                {
                    _logger.LogWarning("Unauthorized access attempt to GetStores.");
                    return Unauthorized();
                }

                if (string.IsNullOrEmpty(distributorId))
                {
                    return BadRequest(new { message = "Distributor ID is required." });
                }

                var stores = await _context.Stores
                    .Where(s => s.DistributorId == distributorId)
                    .Select(s => new { s.Id, s.StoreName, s.StoreAddress })
                    .ToListAsync();
                return Json(stores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stores for distributor {DistributorId}", distributorId);
                return StatusCode(500, new { message = "An error occurred while fetching stores." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TrackShipments()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "company")
                {
                    _logger.LogWarning("Unauthorized access attempt to TrackShipments.");
                    return RedirectToAction("Login", "Auth");
                }

                var shipments = await _context.Shipments
                    .Where(s => s.CompanyId == user.Id)
                    .Include(s => s.Product)
                    .Include(s => s.Distributor)
                    .Include(s => s.Store)
                    .Select(s => new ShipmentViewModel
                    {
                        Id = s.Id,
                        ProductName = s.Product.Name,
                        Destination = s.Destination,
                        Status = s.Status,
                        StoreAddress = s.Store != null ? s.Store.StoreAddress : "Not Assigned",
                        DistributorName = s.Distributor != null ? s.Distributor.UserName : "Not Assigned",
                        IsAcceptedByDistributor = s.IsAcceptedByDistributor
                    })
                    .ToListAsync();

                return View(shipments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading TrackShipments for user {UserId}", User?.Identity?.Name);
                return StatusCode(500, "An error occurred while loading shipments.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType != "company")
                {
                    _logger.LogWarning("Unauthorized access attempt to AddProduct.");
                    return RedirectToAction("Login", "Auth");
                }
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading AddProduct for user {UserId}", User?.Identity?.Name);
                return StatusCode(500, "An error occurred while loading the product creation page.");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct([FromBody] Product model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType.ToLower() != "company")
                {
                    _logger.LogWarning("Unauthorized access attempt to Reports.");
                    return RedirectToAction("Login", "Auth");
                }

                var product = new Product
                {
                    Name = model.Name,
                    ProductionDate = model.ProductionDate,
                    ExpirationDate = model.ExpirationDate,
                    Description = model.Description ?? string.Empty,
                    CompanyId = user.Id
                };

                var qrPayload = new QrPayload
                {
                    Id = product.Id,
                    Name = product.Name,
                    ProductionDate = product.ProductionDate,
                    ExpirationDate = product.ExpirationDate,
                    Description = product.Description
                };

                product.Signature = QrHelper.GenerateQrData(qrPayload);
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return Json(new { success = true, productId = product.Id, qrData = product.Signature });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product");
                return StatusCode(500, new { success = false, message = "Error adding product" });
            }
        }



        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType.ToLower() != "company")
                {
                    _logger.LogWarning("Unauthorized access attempt to Reports.");
                    return RedirectToAction("Login", "Auth");
                }

                var salesData = await _context.Shipments
                    .Where(s => s.CompanyId == user.Id)
                    .GroupBy(s => s.CreatedDate.Month)
                    .Select(g => new ReportsViewModel.SalesData
                    {
                        Month = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(g.Key),
                        Total = g.Count()
                    })
                    .ToListAsync();

                var issues = await _context.Issues
                    .Where(i => i.CompanyId == user.Id)
                    .Select(i => new ReportsViewModel.IssueData
                    {
                        Id = i.Id,
                        ProductName = i.Product.Name,
                        IssueType = i.IssueType,
                        ReportedBy = i.ReportedBy.UserName,
                        Date = i.ReportedDate,
                        Status = i.Status
                    })
                    .ToListAsync();

                var distributionData = await _context.Shipments
                    .Where(s => s.CompanyId == user.Id)
                    .GroupBy(s => s.Destination)
                    .Select(g => new ReportsViewModel.DistributionData
                    {
                        Destination = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                var topProducts = await _context.Shipments
                    .Where(s => s.CompanyId == user.Id && s.Status == "Delivered")
                    .GroupBy(s => s.ProductId)
                    .Select(g => new ReportsViewModel.ProductSalesData
                    {
                        ProductName = g.First().Product.Name,
                        SalesCount = g.Count()
                    })
                    .OrderByDescending(p => p.SalesCount)
                    .Take(5)
                    .ToListAsync();

                var viewModel = new ReportsViewModel
                {
                    SalesPerformance = salesData,
                    Issues = issues,
                    DistributionPerformance = distributionData,
                    TopProducts = topProducts
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Reports for user {UserId}", User?.Identity?.Name);
                return StatusCode(500, "An error occurred while loading the reports.");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetDrivers(string distributorId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.RoleType.ToLower() != "company")
                {
                    _logger.LogWarning("Unauthorized access attempt to Reports.");
                    return RedirectToAction("Login", "Auth");
                }

                if (string.IsNullOrEmpty(distributorId))
                {
                    _logger.LogWarning("GetDrivers called with empty distributorId.");
                    return BadRequest(new { message = "Distributor ID is required." });
                }

                _logger.LogInformation("Fetching drivers for distributorId: {DistributorId}", distributorId);

                var drivers = await _context.Drivers
                    .Where(d => d.DistributorId == distributorId)
                    .Include(d => d.ApplicationUser)
                    .Select(d => new
                    {
                        Id = d.Id, // int
                        fullName = d.ApplicationUser != null ? d.ApplicationUser.FullName : "Unknown",
                        licenseNumber = d.LicenseNumber ?? "N/A",
                        contactNumber = d.ApplicationUser != null ? d.ApplicationUser.ContactNumber : "N/A"
                    })
                    .ToListAsync();

                _logger.LogInformation("Found {DriverCount} drivers for distributorId: {DistributorId}", drivers.Count, distributorId);

                return Json(drivers); // Returns empty array if no drivers found
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching drivers for distributorId: {DistributorId}", distributorId);
                return Json(new { success = false, message = "An error occurred while fetching drivers." });
            }
        }
    }
    

}