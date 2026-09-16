using Microsoft.AspNetCore.Identity;
using SmartSocietyHub.Application.Features.Authentication.Interfaces;
using SmartSocietyHub.Infrastructure.Identity;
using SmartSocietyHub.Infrastructure.Identity.Services;
using SmartSocietyHub.Infrastructure.Persistence;

namespace SmartSocietyHub.API.Extensions;

public static class IdentityExtensions
{
    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services)
    {
        services
            .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;

                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}