using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Pharmaflow7.Models;
using Microsoft.Extensions.Logging;

public class BaseController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<BaseController> _logger;

    public BaseController(UserManager<ApplicationUser> userManager, ILogger<BaseController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (User.Identity.IsAuthenticated)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // Use RoleType consistently (stored in ApplicationUser.RoleType)
                    ViewData["RoleType"] = user.RoleType?.ToLowerInvariant();
                    ViewData["UserName"] = user.RoleType?.ToLowerInvariant() == "company" ? user.CompanyName :
                                           user.RoleType?.ToLowerInvariant() == "distributor" ? user.DistributorName :
                                           user.UserName;
                }
            }
            catch
            {
                ViewData["RoleType"] = null;
                ViewData["UserName"] = User.Identity.Name ?? "Guest";
            }
        }
        else
        {
            ViewData["RoleType"] = null;
            ViewData["UserName"] = "Guest";
        }

        await next();
    }
}
