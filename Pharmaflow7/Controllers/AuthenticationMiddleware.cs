using Microsoft.AspNetCore.Identity;
using Pharmaflow7.Models;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            var user = await userManager.GetUserAsync(context.User);
            context.Items["RoleType"] = user?.RoleType?.ToLower();
            context.Items["UserName"] = context.User.Identity.Name ?? "Guest";
        }
        else
        {
            context.Items["RoleType"] = null;
            context.Items["UserName"] = "Guest";
        }
        await _next(context);
    }
}