using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Pharmaflow7.Models; 

public class BaseController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public BaseController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (User.Identity.IsAuthenticated)
        {
            var user = _userManager.GetUserAsync(User).Result;  
            ViewData["RoleType"] = user?.RoleType;
            if (user?.RoleType == "company")
            {
                ViewData["UserName"] = user?.CompanyName;
            }
            else if (user?.RoleType == "distributor")
            {
                ViewData["UserName"] = user?.DistributorName;
            }
            else
            {
                ViewData["UserName"] = user?.UserName;
            }
        
    }
        base.OnActionExecuting(context);
    }
}