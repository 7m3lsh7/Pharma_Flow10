using Microsoft.AspNetCore.Identity;
using Pharmaflow7.Models;

namespace Pharmaflow7.Data
{
    public static class DbInitializer
    {
        // Use canonical lowercase role names for consistency across the app
        public static readonly string[] Roles = { "admin", "company", "distributor", "consumer", "driver" };

        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Seed Roles
            foreach (var roleName in Roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed Admin
            var adminEmail = "admin@pharmaflow.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "System Administrator",
                    IsVerified = true,
                    IsActive = true,
                        UserType = "admin",
                        RoleType = "admin",
                    CreatedDate = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(admin, "Admin@123456");
                if (result.Succeeded)
                {
                        await userManager.AddToRoleAsync(admin, "admin");
                }
            }
        }
    }
}
