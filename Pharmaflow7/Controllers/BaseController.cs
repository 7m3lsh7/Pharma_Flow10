using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Pharmaflow7.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

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
                    ViewData["RoleType"] = user.RoleType;
                    ViewData["UserName"] = user.RoleType switch
                    {
                        "company" => user.CompanyName,
                        "distributor" => user.DistributorName,
                        _ => user.UserName
                    };
                }
                else
                {
                    ViewData["RoleType"] = null;
                    ViewData["UserName"] = User.Identity.Name ?? "Guest";
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
