using Microsoft.EntityFrameworkCore;
using SmartSocietyHub.Infrastructure.Persistence;

namespace SmartSocietyHub.API.Extensions
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddDatabase(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection")));

            return services;
        }
    }
}
