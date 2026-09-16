using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;

using SmartSocietyHub.API.Extensions;

using SmartSocietyHub.Application.Features.Authentication.Interfaces;
using SmartSocietyHub.Application.Features.Billing.Interfaces;
using SmartSocietyHub.Application.Features.ComplaintManagement.Interfaces;
using SmartSocietyHub.Application.Features.FacilityBooking.Interfaces;
using SmartSocietyHub.Application.Features.PropertyManagement.Interfaces;
using SmartSocietyHub.Application.Features.ResidentManagement.Interfaces;
using SmartSocietyHub.Application.Features.StaffManagement.Interfaces;
using SmartSocietyHub.Application.Features.Updates.Interfaces;

using SmartSocietyHub.Infrastructure.Identity;
using SmartSocietyHub.Infrastructure.Persistence.Services;
using SmartSocietyHub.Infrastructure.Seed;

namespace SmartSocietyHub.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ========================================================
            // DATABASE
            // ========================================================

            builder.Services.AddDatabase(
                builder.Configuration);

            // ========================================================
            // IDENTITY
            // ========================================================

            builder.Services.AddIdentityServices();

            // ========================================================
            // JWT AUTHENTICATION
            // ========================================================

            builder.Services.AddJwtAuthentication(
                builder.Configuration);

            // ========================================================
            // CONTROLLERS
            // ========================================================

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();

            // ========================================================
            // CORS (Angular development client)
            // ========================================================

            const string AngularDevCorsPolicy = "AngularDevClient";

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(AngularDevCorsPolicy, policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:4200",
                            "https://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // ========================================================
            // PROPERTY MANAGEMENT
            // ========================================================

            builder.Services.AddScoped<
                IPropertyService,
                PropertyService>();

            // ========================================================
            // RESIDENT MANAGEMENT
            // ========================================================

            builder.Services.AddScoped<
                IResidentService,
                ResidentService>();

            // ========================================================
            // COMPLAINT MANAGEMENT
            // ========================================================

            builder.Services.AddScoped<
                IComplaintService,
                ComplaintService>();

            // ========================================================
            // STAFF MANAGEMENT
            // ========================================================

            builder.Services.AddScoped<
                IStaffService,
                StaffService>();

            // ========================================================
            // FACILITY BOOKING
            // ========================================================

            builder.Services.AddScoped<
                IFacilityBookingService,
                FacilityBookingService>();

            // ========================================================
            // BILLING MANAGEMENT
            // ========================================================

            builder.Services.AddScoped<
                IBillingService,
                BillingService>();

            // ========================================================
            // COMMUNITY UPDATES & EVENTS
            // ========================================================

            builder.Services.AddScoped<
                IUpdateService,
                UpdateService>();

            // ========================================================
            // SWAGGER
            // ========================================================

            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition(
                    "Bearer",
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",

                        Type = SecuritySchemeType.Http,

                        Scheme = "Bearer",

                        BearerFormat = "JWT",

                        In = ParameterLocation.Header,

                        Description =
                            "Enter your JWT token."
                    });

                options.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference =
                                    new OpenApiReference
                                    {
                                        Type =
                                            ReferenceType.SecurityScheme,

                                        Id = "Bearer"
                                    }
                            },

                            Array.Empty<string>()
                        }
                    });
            });

            var app = builder.Build();

            // ========================================================
            // SEED ROLES AND DEFAULT ADMIN
            // ========================================================

            using (var scope =
                app.Services.CreateScope())
            {
                var services =
                    scope.ServiceProvider;

                var roleManager =
                    services.GetRequiredService<
                        RoleManager<IdentityRole<Guid>>>();

                var userManager =
                    services.GetRequiredService<
                        UserManager<ApplicationUser>>();

                await IdentitySeeder.SeedAsync(
                    roleManager,
                    userManager);
            }

            // ========================================================
            // SWAGGER
            // ========================================================

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();

                app.UseSwaggerUI();
            }

            // ========================================================
            // HTTPS
            // ========================================================

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            // ========================================================
            // CORS
            // ========================================================

            app.UseCors(AngularDevCorsPolicy);

            // ========================================================
            // AUTHENTICATION
            // ========================================================

            app.UseAuthentication();

            // ========================================================
            // AUTHORIZATION
            // ========================================================

            app.UseAuthorization();

            // ========================================================
            // CONTROLLERS
            // ========================================================

            app.MapControllers();

            await app.RunAsync();
        }
    }
}