using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pharmaflow7.Models;

namespace Pharmaflow7.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(UserManager<ApplicationUser> userManager) : base(userManager) { }



        public IActionResult Index()
        {

            if (User.Identity.IsAuthenticated)
            {
                var roleType = ViewData["RoleType"]?.ToString()?.ToLower();
                return RedirectToAction(roleType switch
                {
                    "company" => "CompanyDashboard",
                    "distributor" => "dashboard",
                    "consumer" => "ConsumerDashboard",
                    _ => "Index"
                }, roleType switch
                {
                    "company" => "Company",
                    "distributor" => "Distributor",
                    "consumer" => "Consumer",
                    _ => "Home"
                });
            }
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
