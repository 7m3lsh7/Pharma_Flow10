using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pharmaflow7.Models;

namespace Pharmaflow7.Controllers
{
    public class Home_pageController : BaseController
    {
        private readonly ILogger<Home_pageController> _logger;
        public Home_pageController(UserManager<ApplicationUser> userManager,ILogger<Home_pageController> logger) : base(userManager, logger) { }
        
    }
}
