using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pharmaflow7.Data;
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

        public CompanyController(AppDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<CompanyController> logger) : base(userManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
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

                    _logger.LogWarning("Unauthorized access attempt to CompanyDashboard.");
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
                _logger.LogInformation("Received shipment data: ProductId={ProductId}, Destination={Destination}, DistributorId={DistributorId}, StoreId={StoreId}",
                    model.ProductId, model.Destination, model.DistributorId, model.StoreId);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for CreateShipment: {Errors}",
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    model.Products = await _context.Products.Where(p => p.CompanyId == user.Id).ToListAsync();
                    model.Distributors = await _userManager.Users.Where(u => u.RoleType == "distributor").ToListAsync();
                    model.Stores = string.IsNullOrEmpty(model.DistributorId)
                        ? new List<Store>()
                        : await _context.Stores.Where(s => s.DistributorId == model.DistributorId).ToListAsync();
                    TempData["Error"] = "Please correct the errors and try again: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return View(model);
                }

                var shipment = new Shipment
                {
                    ProductId = model.ProductId,
                    Destination = model.Destination,
                    DistributorId = string.IsNullOrEmpty(model.DistributorId) ? null : model.DistributorId,
                    StoreId = model.StoreId.HasValue && model.StoreId != 0 ? model.StoreId : null,
                    CompanyId = user.Id,
                    Status = "Pending"  
                };
                _context.Shipments.Add(shipment);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Shipment {ShipmentId} created by company {CompanyId} with StoreId={StoreId}",
                    shipment.Id, user.Id, shipment.StoreId);

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
                if (user == null || user.RoleType != "company")
                {
                    _logger.LogWarning("Unauthorized access attempt to AddProduct.");
                    return RedirectToAction("Login", "Auth");
                }

                ModelState.Remove("CompanyId");
                ModelState.Remove("Company");

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for AddProduct: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    return BadRequest(ModelState);
                }

                var product = new Product
                {
                    Name = model.Name,
                    ProductionDate = model.ProductionDate,
                    ExpirationDate = model.ExpirationDate,
                    Description = model.Description ?? string.Empty,
                    CompanyId = user.Id
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Product {ProductId} added by company {CompanyId}", product.Id, user.Id);

                return Json(new { productId = product.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product for company {CompanyId}", User?.Identity?.Name);
                return StatusCode(500, new { success = false, message = "An error occurred while adding the product." });
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
    }
}