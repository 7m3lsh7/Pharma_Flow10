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

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (User.Identity.IsAuthenticated)
        {
            try
            {
                var user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
                if (user != null)
                {
                    _logger.LogInformation("User {UserId} authenticated with RoleType: {RoleType}", user.Id, user.RoleType);
                    ViewData["RoleType"] = user.RoleType;
                    if (user.RoleType == "company")
                    {
                        ViewData["UserName"] = user.CompanyName;
                    }
                    else if (user.RoleType == "distributor")
                    {
                        ViewData["UserName"] = user.DistributorName;
                    }
                    else
                    {
                        ViewData["UserName"] = user.UserName;
                    }
                }
                else
                {
                    _logger.LogWarning("User is authenticated but not found in database. ClaimsPrincipal: {UserName}", User.Identity.Name);
                    ViewData["RoleType"] = null;
                    ViewData["UserName"] = User.Identity.Name ?? "Guest";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user data in BaseController for {UserName}", User.Identity.Name);
                ViewData["RoleType"] = null;
                ViewData["UserName"] = User.Identity.Name ?? "Guest";
            }
        }
        else
        {
            _logger.LogWarning("Unauthenticated access attempt to {Action} in {Controller}", context.ActionDescriptor.DisplayName, context.Controller);
            ViewData["RoleType"] = null;
            ViewData["UserName"] = "Guest";
        }

        base.OnActionExecuting(context);
    }
}