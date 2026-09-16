using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using SmartSocietyHub.Application.Features.ResidentManagement.DTOs;
using SmartSocietyHub.Application.Features.ResidentManagement.Interfaces;
using SmartSocietyHub.Infrastructure.Identity;
using SmartSocietyHub.Infrastructure.Persistence.Models;

namespace SmartSocietyHub.Infrastructure.Persistence.Services
{
    public class ResidentService : IResidentService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ResidentService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ============================================================
        // CREATE RESIDENT / OWNER + LOGIN + FAMILY MEMBERS
        // ============================================================
        public async Task<CreateResidentResponse> CreateAsync(
            CreateResidentRequest request)
        {
            // ========================================================
            // VALIDATE FAMILY MEMBERS
            // ========================================================

            if (request.NumberOfFamilyMembers != request.FamilyMembers.Count)
            {
                throw new InvalidOperationException(
                    $"Number of family members ({request.NumberOfFamilyMembers}) " +
                    $"does not match the number of family member records provided " +
                    $"({request.FamilyMembers.Count}).");
            }

            // ========================================================
            // CHECK PROPERTY
            // ========================================================

            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == request.PropertyId);

            if (property == null)
            {
                throw new InvalidOperationException(
                    "The selected property does not exist.");
            }

            // ========================================================
            // PROPERTY OCCUPANCY RULE
            // ========================================================
            // A resident can only be assigned to a Vacant property.
            // If the property is already Occupied, reject the request.

            if (string.Equals(
                    property.Status,
                    "Occupied",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "The selected property is already occupied.");
            }

            // ========================================================
            // CHECK RESIDENT CNIC
            // ========================================================

            var cnicExists = await _context.Residents
                .AnyAsync(r => r.CNIC == request.CNIC);

            if (cnicExists)
            {
                throw new InvalidOperationException(
                    "A resident with this CNIC already exists.");
            }

            // ========================================================
            // GENERATE LOGIN EMAIL
            // ========================================================

            var loginEmail = request.Email;

            if (string.IsNullOrWhiteSpace(loginEmail))
            {
                var cleanCnic = request.CNIC
                    .Replace("-", "")
                    .Trim();

                loginEmail =
                    $"resident.{cleanCnic}@smartsocietyhub.com";
            }

            // ========================================================
            // CHECK LOGIN EMAIL
            // ========================================================

            var existingUser = await _userManager
                .FindByEmailAsync(loginEmail);

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
            // DATABASE TRANSACTION
            // ========================================================

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // ====================================================
                // CREATE APPLICATION USER / LOGIN ACCOUNT
                // ====================================================

                var applicationUser = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    FullName = request.FullName,
                    UserName = loginEmail,
                    Email = loginEmail,
                    EmailConfirmed = true
                };

                var userResult = await _userManager.CreateAsync(
                    applicationUser,
                    temporaryPassword);

                if (!userResult.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        userResult.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(
                        $"Unable to create the resident login account. {errors}");
                }

                // ====================================================
                // ASSIGN RESIDENT ROLE
                // ====================================================

                var roleResult = await _userManager.AddToRoleAsync(
                    applicationUser,
                    "Resident");

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        roleResult.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(
                        $"Unable to assign the Resident role. {errors}");
                }

                // ====================================================
                // CREATE RESIDENT / OWNER
                // ====================================================

                var resident = new Resident
                {
                    Id = Guid.NewGuid(),
                    FullName = request.FullName,
                    CNIC = request.CNIC,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender,
                    PropertyId = request.PropertyId,
                    Status = request.Status,

                    ApplicationUserId = applicationUser.Id
                };

                _context.Residents.Add(resident);

                // ====================================================
                // CREATE FAMILY MEMBERS
                // ====================================================

                foreach (var familyMemberRequest in request.FamilyMembers)
                {
                    var familyMember = new FamilyMember
                    {
                        Id = Guid.NewGuid(),

                        FullName = familyMemberRequest.FullName,

                        CNIC = familyMemberRequest.CNIC,

                        PhoneNumber = familyMemberRequest.PhoneNumber,

                        Email = familyMemberRequest.Email,

                        DateOfBirth = familyMemberRequest.DateOfBirth,

                        Gender = familyMemberRequest.Gender,

                        Relationship = familyMemberRequest.Relationship,

                        ResidentId = resident.Id
                    };

                    _context.FamilyMembers.Add(familyMember);
                }

                // ====================================================
                // MARK PROPERTY AS OCCUPIED
                // ====================================================

                property.Status = "Occupied";

                // ====================================================
                // SAVE EVERYTHING
                // ====================================================

                await _context.SaveChangesAsync();

                // ====================================================
                // COMMIT TRANSACTION
                // ====================================================

                await transaction.CommitAsync();

                // ====================================================
                // PREPARE RESPONSE
                // ====================================================

                var residentResponse = new ResidentResponse
                {
                    Id = resident.Id,

                    FullName = resident.FullName,

                    CNIC = resident.CNIC,

                    PhoneNumber = resident.PhoneNumber,

                    Email = resident.Email,

                    DateOfBirth = resident.DateOfBirth,

                    Gender = resident.Gender,

                    PropertyId = resident.PropertyId,

                    HouseNumber = property.HouseNumber,

                    Block = property.Block,

                    Status = resident.Status,

                    FamilyMembers = request.FamilyMembers
                        .Select(f => new FamilyMemberResponse
                        {
                            FullName = f.FullName,

                            CNIC = f.CNIC,

                            PhoneNumber = f.PhoneNumber,

                            Email = f.Email,

                            DateOfBirth = f.DateOfBirth,

                            Gender = f.Gender,

                            Relationship = f.Relationship
                        })
                        .ToList()
                };

                return new CreateResidentResponse
                {
                    Resident = residentResponse,

                    LoginEmail = loginEmail,

                    TemporaryPassword = temporaryPassword
                };
            }
            catch
            {
                await transaction.RollbackAsync();

                throw;
            }
        }

        // ============================================================
        // GET ALL RESIDENTS / OWNERS
        // ============================================================
        public async Task<List<ResidentResponse>> GetAllAsync()
        {
            return await _context.Residents
                .Include(r => r.FamilyMembers)
                .Include(r => r.Property)
                .Select(resident => new ResidentResponse
                {
                    Id = resident.Id,

                    FullName = resident.FullName,

                    CNIC = resident.CNIC,

                    PhoneNumber = resident.PhoneNumber,

                    Email = resident.Email,

                    DateOfBirth = resident.DateOfBirth,

                    Gender = resident.Gender,

                    PropertyId = resident.PropertyId,

                    HouseNumber = resident.Property != null ? resident.Property.HouseNumber : string.Empty,

                    Block = resident.Property != null ? resident.Property.Block : string.Empty,

                    Status = resident.Status,

                    FamilyMembers = resident.FamilyMembers
                        .Select(f => new FamilyMemberResponse
                        {
                            Id = f.Id,

                            FullName = f.FullName,

                            CNIC = f.CNIC,

                            PhoneNumber = f.PhoneNumber,

                            Email = f.Email,

                            DateOfBirth = f.DateOfBirth,

                            Gender = f.Gender,

                            Relationship = f.Relationship
                        })
                        .ToList()
                })
                .ToListAsync();
        }

        // ============================================================
        // GET RESIDENT / OWNER BY ID
        // ============================================================
        public async Task<ResidentResponse?> GetByIdAsync(Guid id)
        {
            var resident = await _context.Residents
                .Include(r => r.FamilyMembers)
                .Include(r => r.Property)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resident == null)
            {
                return null;
            }

            return new ResidentResponse
            {
                Id = resident.Id,

                FullName = resident.FullName,

                CNIC = resident.CNIC,

                PhoneNumber = resident.PhoneNumber,

                Email = resident.Email,

                DateOfBirth = resident.DateOfBirth,

                Gender = resident.Gender,

                PropertyId = resident.PropertyId,

                HouseNumber = resident.Property != null ? resident.Property.HouseNumber : string.Empty,

                Block = resident.Property != null ? resident.Property.Block : string.Empty,

                Status = resident.Status,

                FamilyMembers = resident.FamilyMembers
                    .Select(f => new FamilyMemberResponse
                    {
                        Id = f.Id,

                        FullName = f.FullName,

                        CNIC = f.CNIC,

                        PhoneNumber = f.PhoneNumber,

                        Email = f.Email,

                        DateOfBirth = f.DateOfBirth,

                        Gender = f.Gender,

                        Relationship = f.Relationship
                    })
                    .ToList()
            };
        }

        // ============================================================
        // GET RESIDENT / OWNER BY APPLICATION USER ID
        // ============================================================
        public async Task<ResidentResponse?> GetByApplicationUserIdAsync(
            Guid applicationUserId)
        {
            var resident = await _context.Residents
                .Include(r => r.FamilyMembers)
                .Include(r => r.Property)
                .FirstOrDefaultAsync(
                    r => r.ApplicationUserId == applicationUserId);

            if (resident == null)
            {
                return null;
            }

            return new ResidentResponse
            {
                Id = resident.Id,

                FullName = resident.FullName,

                CNIC = resident.CNIC,

                PhoneNumber = resident.PhoneNumber,

                Email = resident.Email,

                DateOfBirth = resident.DateOfBirth,

                Gender = resident.Gender,

                PropertyId = resident.PropertyId,

                HouseNumber = resident.Property != null ? resident.Property.HouseNumber : string.Empty,

                Block = resident.Property != null ? resident.Property.Block : string.Empty,

                Status = resident.Status,

                FamilyMembers = resident.FamilyMembers
                    .Select(f => new FamilyMemberResponse
                    {
                        Id = f.Id,

                        FullName = f.FullName,

                        CNIC = f.CNIC,

                        PhoneNumber = f.PhoneNumber,

                        Email = f.Email,

                        DateOfBirth = f.DateOfBirth,

                        Gender = f.Gender,

                        Relationship = f.Relationship
                    })
                    .ToList()
            };
        }

        // ============================================================
        // UPDATE RESIDENT / OWNER + FAMILY MEMBERS
        // ============================================================
        public async Task<ResidentResponse?> UpdateAsync(
            Guid id,
            CreateResidentRequest request)
        {
            // ========================================================
            // VALIDATE FAMILY MEMBERS
            // ========================================================

            if (request.NumberOfFamilyMembers != request.FamilyMembers.Count)
            {
                throw new InvalidOperationException(
                    $"Number of family members ({request.NumberOfFamilyMembers}) " +
                    $"does not match the number of family member records provided " +
                    $"({request.FamilyMembers.Count}).");
            }

            // ========================================================
            // FIND RESIDENT
            // ========================================================

            var resident = await _context.Residents
                .Include(r => r.FamilyMembers)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resident == null)
            {
                return null;
            }

            // ========================================================
            // CHECK NEW PROPERTY
            // ========================================================

            var newProperty = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == request.PropertyId);

            if (newProperty == null)
            {
                throw new InvalidOperationException(
                    "The selected property does not exist.");
            }

            // ========================================================
            // PROPERTY CHANGE CHECK
            // ========================================================

            var propertyChanged =
                resident.PropertyId != request.PropertyId;

            // If resident is moving to another property,
            // the new property must be vacant.
            if (propertyChanged &&
                string.Equals(
                    newProperty.Status,
                    "Occupied",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "The selected property is already occupied.");
            }

            // ========================================================
            // CHECK CNIC
            // ========================================================

            var cnicExists = await _context.Residents
                .AnyAsync(r =>
                    r.CNIC == request.CNIC &&
                    r.Id != id);

            if (cnicExists)
            {
                throw new InvalidOperationException(
                    "A resident with this CNIC already exists.");
            }

            // ========================================================
            // UPDATE RESIDENT / OWNER
            // ========================================================

            resident.FullName = request.FullName;

            resident.CNIC = request.CNIC;

            resident.PhoneNumber = request.PhoneNumber;

            resident.Email = request.Email;

            resident.DateOfBirth = request.DateOfBirth;

            resident.Gender = request.Gender;

            resident.PropertyId = request.PropertyId;

            resident.Status = request.Status;

            // ========================================================
            // UPDATE LOGIN ACCOUNT
            // ========================================================

            if (resident.ApplicationUserId.HasValue)
            {
                var applicationUser = await _userManager.FindByIdAsync(
                    resident.ApplicationUserId.Value.ToString());

                if (applicationUser != null)
                {
                    applicationUser.FullName = request.FullName;

                    if (!string.IsNullOrWhiteSpace(request.Email) &&
                        !string.Equals(
                            applicationUser.Email,
                            request.Email,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        var emailExists = await _userManager
                            .FindByEmailAsync(request.Email);

                        if (emailExists != null &&
                            emailExists.Id != applicationUser.Id)
                        {
                            throw new InvalidOperationException(
                                "A user account with this email already exists.");
                        }

                        applicationUser.Email = request.Email;
                        applicationUser.UserName = request.Email;
                    }

                    var userUpdateResult =
                        await _userManager.UpdateAsync(applicationUser);

                    if (!userUpdateResult.Succeeded)
                    {
                        var errors = string.Join(
                            "; ",
                            userUpdateResult.Errors
                                .Select(e => e.Description));

                        throw new InvalidOperationException(
                            $"Unable to update the resident login account. {errors}");
                    }
                }
            }

            // ========================================================
            // PROPERTY STATUS UPDATE
            // ========================================================

            if (propertyChanged)
            {
                // The resident is leaving the old property.
                var oldProperty = await _context.Properties
                    .FirstOrDefaultAsync(
                        p => p.Id == resident.PropertyId);

                if (oldProperty != null)
                {
                    oldProperty.Status = "Vacant";
                }

                // The resident is moving into the new property.
                newProperty.Status = "Occupied";
            }

            // ========================================================
            // REMOVE OLD FAMILY MEMBERS
            // ========================================================

            _context.FamilyMembers.RemoveRange(
                resident.FamilyMembers);

            // ========================================================
            // ADD UPDATED FAMILY MEMBERS
            // ========================================================

            foreach (var familyMemberRequest in request.FamilyMembers)
            {
                var familyMember = new FamilyMember
                {
                    Id = Guid.NewGuid(),

                    FullName = familyMemberRequest.FullName,

                    CNIC = familyMemberRequest.CNIC,

                    PhoneNumber = familyMemberRequest.PhoneNumber,

                    Email = familyMemberRequest.Email,

                    DateOfBirth = familyMemberRequest.DateOfBirth,

                    Gender = familyMemberRequest.Gender,

                    Relationship = familyMemberRequest.Relationship,

                    ResidentId = resident.Id
                };

                _context.FamilyMembers.Add(familyMember);
            }

            // ========================================================
            // SAVE CHANGES
            // ========================================================

            await _context.SaveChangesAsync();

            // ========================================================
            // RETURN UPDATED RESIDENT
            // ========================================================

            return new ResidentResponse
            {
                Id = resident.Id,

                FullName = resident.FullName,

                CNIC = resident.CNIC,

                PhoneNumber = resident.PhoneNumber,

                Email = resident.Email,

                DateOfBirth = resident.DateOfBirth,

                Gender = resident.Gender,

                PropertyId = resident.PropertyId,

                HouseNumber = newProperty.HouseNumber,

                Block = newProperty.Block,

                Status = resident.Status,

                FamilyMembers = request.FamilyMembers
                    .Select(f => new FamilyMemberResponse
                    {
                        FullName = f.FullName,

                        CNIC = f.CNIC,

                        PhoneNumber = f.PhoneNumber,

                        Email = f.Email,

                        DateOfBirth = f.DateOfBirth,

                        Gender = f.Gender,

                        Relationship = f.Relationship
                    })
                    .ToList()
            };
        }

        // ============================================================
        // DELETE RESIDENT / OWNER
        // ============================================================
        public async Task<bool> DeleteAsync(Guid id)
        {
            var resident = await _context.Residents
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resident == null)
            {
                return false;
            }

            // ========================================================
            // BLOCK DELETION IF RECORDS REFERENCE THIS RESIDENT
            // ========================================================
            // Complaints, facility bookings, and bills all reference
            // Resident with a Restrict delete behavior. Checking here
            // (before any mutation, including the login-account
            // deletion below) avoids leaving the account partially
            // deleted if the resident row itself cannot be removed.

            var hasDependentRecords =
                await _context.Complaints.AnyAsync(c => c.ResidentId == resident.Id) ||
                await _context.FacilityBookings.AnyAsync(b => b.ResidentId == resident.Id) ||
                await _context.Bills.AnyAsync(b => b.ResidentId == resident.Id);

            if (hasDependentRecords)
            {
                throw new InvalidOperationException(
                    "This resident cannot be deleted because they have complaint, booking, or billing records on file.");
            }

            // ========================================================
            // MAKE PROPERTY VACANT
            // ========================================================

            var property = await _context.Properties
                .FirstOrDefaultAsync(
                    p => p.Id == resident.PropertyId);

            if (property != null)
            {
                property.Status = "Vacant";
            }

            // ========================================================
            // DELETE RESIDENT
            // ========================================================

            _context.Residents.Remove(resident);

            // ========================================================
            // DELETE LOGIN ACCOUNT
            // ========================================================

            if (resident.ApplicationUserId.HasValue)
            {
                var applicationUser = await _userManager.FindByIdAsync(
                    resident.ApplicationUserId.Value.ToString());

                if (applicationUser != null)
                {
                    var userResult = await _userManager
                        .DeleteAsync(applicationUser);

                    if (!userResult.Succeeded)
                    {
                        var errors = string.Join(
                            "; ",
                            userResult.Errors
                                .Select(e => e.Description));

                        throw new InvalidOperationException(
                            $"Unable to delete the resident login account. {errors}");
                    }
                }
            }

            // ========================================================
            // SAVE CHANGES
            // ========================================================

            await _context.SaveChangesAsync();

            return true;
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

            password[0] =
                GetRandomCharacter(upperCase);

            password[1] =
                GetRandomCharacter(lowerCase);

            password[2] =
                GetRandomCharacter(numbers);

            password[3] =
                GetRandomCharacter(specialCharacters);

            for (int i = 4; i < password.Length; i++)
            {
                password[i] =
                    GetRandomCharacter(allCharacters);
            }

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