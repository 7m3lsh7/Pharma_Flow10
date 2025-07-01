using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
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
                    _logger.LogInformation("Middleware: User {UserId} authenticated with RoleType: {RoleType}", user.Id, user.RoleType);
                    context.Items["RoleType"] = user.RoleType?.ToLower();
                    context.Items["UserName"] = user.RoleType == "company" ? user.CompanyName :
                                               user.RoleType == "distributor" ? user.DistributorName :
                                               context.User.Identity.Name ?? "Guest";
                }
                else
                {
                    _logger.LogWarning("Middleware: User is authenticated but not found in database. ClaimsPrincipal: {UserName}", context.User.Identity.Name);
                    context.Items["RoleType"] = null;
                    context.Items["UserName"] = context.User.Identity.Name ?? "Guest";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Middleware: Error retrieving user data for {UserName}", context.User.Identity.Name);
                context.Items["RoleType"] = null;
                context.Items["UserName"] = context.User.Identity.Name ?? "Guest";
            }
        }
        else
        {
            _logger.LogInformation("Middleware: Unauthenticated request to {Path}", context.Request.Path);
            context.Items["RoleType"] = null;
            context.Items["UserName"] = "Guest";
        }

        await _next(context);
    }
}