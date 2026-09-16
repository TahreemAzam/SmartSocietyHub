using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;

using SmartSocietyHub.Application.Features.StaffManagement.DTOs;
using SmartSocietyHub.Application.Features.StaffManagement.Interfaces;
using SmartSocietyHub.Infrastructure.Identity;

namespace SmartSocietyHub.Infrastructure.Persistence.Services
{
    public class StaffService : IStaffService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public StaffService(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // ============================================================
        // CREATE MAINTENANCE STAFF
        // ============================================================
        public async Task<CreateMaintenanceStaffResponse>
            CreateMaintenanceStaffAsync(
                CreateMaintenanceStaffRequest request)
        {
            // ========================================================
            // CHECK WHETHER EMAIL ALREADY EXISTS
            // ========================================================

            var existingUser = await _userManager
                .FindByEmailAsync(request.Email);

            if (existingUser != null)
            {
                throw new InvalidOperationException(
                    "A user account with this email already exists.");
            }

            // ========================================================
            // GENERATE TEMPORARY PASSWORD
            // ========================================================

            var temporaryPassword = GenerateTemporaryPassword();

            // ========================================================
            // CREATE APPLICATION USER
            // ========================================================

            var applicationUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),

                FullName = request.FullName,

                UserName = request.Email,

                Email = request.Email,

                PhoneNumber = request.PhoneNumber,

                EmailConfirmed = true
            };

            var userResult = await _userManager.CreateAsync(
                applicationUser,
                temporaryPassword);

            if (!userResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    userResult.Errors
                        .Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Unable to create the maintenance staff account. {errors}");
            }

            // ========================================================
            // ASSIGN MAINTENANCE STAFF ROLE
            // ========================================================

            var roleResult = await _userManager.AddToRoleAsync(
                applicationUser,
                "MaintenanceStaff");

            if (!roleResult.Succeeded)
            {
                // If role assignment fails, remove the user that
                // was just created so we don't leave an incomplete
                // account in the database.

                await _userManager.DeleteAsync(applicationUser);

                var errors = string.Join(
                    "; ",
                    roleResult.Errors
                        .Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Unable to assign the MaintenanceStaff role. {errors}");
            }

            // ========================================================
            // RETURN RESPONSE
            // ========================================================

            return new CreateMaintenanceStaffResponse
            {
                UserId = applicationUser.Id,

                FullName = applicationUser.FullName,

                Email = applicationUser.Email ?? string.Empty,

                PhoneNumber = applicationUser.PhoneNumber ?? string.Empty,

                Role = "MaintenanceStaff",

                TemporaryPassword = temporaryPassword
            };
        }

        // ============================================================
        // GET ALL MAINTENANCE STAFF
        // ============================================================
        // Used by Admin to select a Maintenance Staff member when
        // assigning a complaint.
        // ============================================================
        public async Task<List<MaintenanceStaffResponse>> GetAllMaintenanceStaffAsync()
        {
            var staffUsers = await _userManager.GetUsersInRoleAsync("MaintenanceStaff");

            return staffUsers
                .Select(user => new MaintenanceStaffResponse
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty
                })
                .OrderBy(s => s.FullName)
                .ToList();
        }

        // ============================================================
        // GENERATE TEMPORARY PASSWORD
        // ============================================================
        private static string GenerateTemporaryPassword()
        {
            const string upperCase =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            const string lowerCase =
                "abcdefghijklmnopqrstuvwxyz";

            const string numbers =
                "0123456789";

            const string specialCharacters =
                "!@#$%^&*";

            const string allCharacters =
                upperCase +
                lowerCase +
                numbers +
                specialCharacters;

            var password = new char[12];

            // Guarantee at least:
            // 1 uppercase
            // 1 lowercase
            // 1 number
            // 1 special character

            password[0] =
                GetRandomCharacter(upperCase);

            password[1] =
                GetRandomCharacter(lowerCase);

            password[2] =
                GetRandomCharacter(numbers);

            password[3] =
                GetRandomCharacter(specialCharacters);

            // Fill remaining characters.

            for (int i = 4; i < password.Length; i++)
            {
                password[i] =
                    GetRandomCharacter(allCharacters);
            }

            // Shuffle password characters.

            for (int i = password.Length - 1; i > 0; i--)
            {
                int j =
                    RandomNumberGenerator.GetInt32(i + 1);

                (password[i], password[j]) =
                    (password[j], password[i]);
            }

            return new string(password);
        }

        // ============================================================
        // GET RANDOM CHARACTER
        // ============================================================
        private static char GetRandomCharacter(
            string characters)
        {
            int index =
                RandomNumberGenerator.GetInt32(
                    characters.Length);

            return characters[index];
        }
    }
}