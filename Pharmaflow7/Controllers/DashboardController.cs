using Microsoft.AspNetCore.Mvc;

namespace Pharmaflow7.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
