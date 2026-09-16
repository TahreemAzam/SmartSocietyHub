using Microsoft.AspNetCore.Identity;
using SmartSocietyHub.Infrastructure.Identity;

namespace SmartSocietyHub.Infrastructure.Seed
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(
            RoleManager<IdentityRole<Guid>> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            string[] roles =
            {
                "Admin",
                "Resident",
                "SecurityGuard",
                "MaintenanceStaff"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole<Guid>
                        {
                            Name = role
                        });
                }
            }

            var adminEmail = "admin@smartsocietyhub.com";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    FullName = "System Administrator",
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}