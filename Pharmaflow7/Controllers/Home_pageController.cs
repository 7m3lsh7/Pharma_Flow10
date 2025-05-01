using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pharmaflow7.Models;

namespace Pharmaflow7.Controllers
{
    public class Home_pageController : BaseController
    {
        public Home_pageController(UserManager<ApplicationUser> userManager) : base(userManager) { }
        
    }
}
