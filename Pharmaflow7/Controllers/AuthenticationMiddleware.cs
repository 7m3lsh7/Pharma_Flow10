using Microsoft.AspNetCore.Identity;
using Pharmaflow7.Models;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            try
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user != null)
                {
                    _logger.LogInformation("Middleware: User {UserId} authenticated with RoleType: {RoleType}", user.Id, user.UserType);
                    context.Items["RoleType"] = user.UserType?.ToLower();
                    context.Items["UserName"] = user.UserType == "company" ? user.CompanyName :
                                               user.UserType == "distributor" ? user.DistributorName :
                                               context.User.Identity.Name ?? "Guest";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Middleware error for {UserName}", context.User.Identity.Name);
                context.Items["RoleType"] = null;
                context.Items["UserName"] = context.User.Identity.Name ?? "Guest";
            }
        }
        else
        {
            context.Items["RoleType"] = null;
            context.Items["UserName"] = "Guest";
        }

        await _next(context);
    }
}
