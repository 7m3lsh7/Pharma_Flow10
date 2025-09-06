using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pharmaflow7.Models;
using System.Diagnostics;

namespace Pharmaflow7.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(UserManager<ApplicationUser> userManager, ILogger<HomeController> logger) : base(userManager, logger) { }


        [AllowAnonymous]
        public IActionResult Index()
        {
            // ·Ê «·„” Œœ„ „”Ã· œŒÊ·° „„ﬂ‰  ⁄„· redirect
            if (User.Identity.IsAuthenticated)
            {
                var roleType = ViewData["RoleType"]?.ToString()?.ToLower();
                return RedirectToAction(roleType switch
                {
                    "company" => "CompanyDashboard",
                    "distributor" => "dashboard",
                    "driver" => "AddDriver",
                    _ => "Index"
                }, roleType switch
                {
                    "company" => "Company",
                    "distributor" => "Distributor",
                    "driver" => "Driver",
                    _ => "Home"
                });
            }

            // ·Ê „‘ „”Ã· œŒÊ·° «·’›Õ… Â ŸÂ— ⁄«œÌ
            return View();
        }


        public IActionResult About()
        {
            return View();
        }

        public IActionResult HowItWorks()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
        public IActionResult Privacy()
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
