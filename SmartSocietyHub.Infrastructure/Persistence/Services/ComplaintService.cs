using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using SmartSocietyHub.Application.Features.ComplaintManagement.DTOs;
using SmartSocietyHub.Application.Features.ComplaintManagement.Interfaces;
using SmartSocietyHub.Infrastructure.Identity;
using SmartSocietyHub.Infrastructure.Persistence.Models;

namespace SmartSocietyHub.Infrastructure.Persistence.Services
{
    public class ComplaintService : IComplaintService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ComplaintService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ============================================================
        // CREATE COMPLAINT
        // ============================================================
        // Resident creates a complaint.
        // Property is automatically taken from the logged-in resident.
        // ============================================================
        public async Task<ComplaintResponse> CreateAsync(
            Guid applicationUserId,
            CreateComplaintRequest request)
        {
            // Find the resident linked to the logged-in user.
            var resident = await _context.Residents
                .FirstOrDefaultAsync(
                    r => r.ApplicationUserId == applicationUserId);

            if (resident == null)
            {
                throw new InvalidOperationException(
                    "The logged-in user is not linked to a resident.");
            }

            // Check that the resident's property still exists.
            var property = await _context.Properties
                .FirstOrDefaultAsync(
                    p => p.Id == resident.PropertyId);

            if (property == null)
            {
                throw new InvalidOperationException(
                    "The resident's property does not exist.");
            }

            var complaint = new Complaint
            {
                Id = Guid.NewGuid(),

                ResidentId = resident.Id,

                // Property is automatically taken from the resident.
                PropertyId = resident.PropertyId,

                Title = request.Title,

                Description = request.Description,

                Category = request.Category,

                Status = "Open",

                CreatedAt = DateTime.UtcNow
            };

            _context.Complaints.Add(complaint);

            await _context.SaveChangesAsync();

            return await BuildComplaintResponseAsync(
                complaint.Id);
        }

        // ============================================================
        // GET MY COMPLAINTS
        // ============================================================
        // Resident gets their own complaints.
        // ============================================================
        public async Task<List<ComplaintResponse>> GetMyComplaintsAsync(
            Guid applicationUserId)
        {
            var resident = await _context.Residents
                .FirstOrDefaultAsync(
                    r => r.ApplicationUserId == applicationUserId);

            if (resident == null)
            {
                throw new InvalidOperationException(
                    "The logged-in user is not linked to a resident.");
            }

            return await _context.Complaints
                .Include(c => c.Resident)
                .Include(c => c.Property)
                .Where(c => c.ResidentId == resident.Id)
                .Select(c => new ComplaintResponse
                {
                    Id = c.Id,

                    ResidentId = c.ResidentId,

                    ResidentName = c.Resident != null
                        ? c.Resident.FullName
                        : string.Empty,

                    PropertyId = c.PropertyId,

                    HouseNumber = c.Property != null
                        ? c.Property.HouseNumber
                        : string.Empty,

                    Block = c.Property != null
                        ? c.Property.Block
                        : string.Empty,

                    Title = c.Title,

                    Description = c.Description,

                    Category = c.Category,

                    Status = c.Status,

                    AssignedToUserId = c.AssignedToUserId,

                    CreatedAt = c.CreatedAt,

                    ResolvedAt = c.ResolvedAt,

                    ClosedAt = c.ClosedAt,

                    ReopenedAt = c.ReopenedAt
                })
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // ============================================================
        // GET ALL COMPLAINTS
        // ============================================================
        // Admin gets all complaints.
        // ============================================================
        public async Task<List<ComplaintResponse>> GetAllAsync()
        {
            return await _context.Complaints
                .Include(c => c.Resident)
                .Include(c => c.Property)
                .Select(c => new ComplaintResponse
                {
                    Id = c.Id,

                    ResidentId = c.ResidentId,

                    ResidentName = c.Resident != null
                        ? c.Resident.FullName
                        : string.Empty,

                    PropertyId = c.PropertyId,

                    HouseNumber = c.Property != null
                        ? c.Property.HouseNumber
                        : string.Empty,

                    Block = c.Property != null
                        ? c.Property.Block
                        : string.Empty,

                    Title = c.Title,

                    Description = c.Description,

                    Category = c.Category,

                    Status = c.Status,

                    AssignedToUserId = c.AssignedToUserId,

                    CreatedAt = c.CreatedAt,

                    ResolvedAt = c.ResolvedAt,

                    ClosedAt = c.ClosedAt,

                    ReopenedAt = c.ReopenedAt
                })
                .ToListAsync();
        }

        // ============================================================
        // GET COMPLAINTS ASSIGNED TO A MAINTENANCE STAFF MEMBER
        // ============================================================
        public async Task<List<ComplaintResponse>> GetAssignedToMeAsync(
            Guid maintenanceStaffUserId)
        {
            return await _context.Complaints
                .Include(c => c.Resident)
                .Include(c => c.Property)
                .Where(c => c.AssignedToUserId == maintenanceStaffUserId)
                .Select(c => new ComplaintResponse
                {
                    Id = c.Id,

                    ResidentId = c.ResidentId,

                    ResidentName = c.Resident != null
                        ? c.Resident.FullName
                        : string.Empty,

                    PropertyId = c.PropertyId,

                    HouseNumber = c.Property != null
                        ? c.Property.HouseNumber
                        : string.Empty,

                    Block = c.Property != null
                        ? c.Property.Block
                        : string.Empty,

                    Title = c.Title,

                    Description = c.Description,

                    Category = c.Category,

                    Status = c.Status,

                    AssignedToUserId = c.AssignedToUserId,

                    CreatedAt = c.CreatedAt,

                    ResolvedAt = c.ResolvedAt,

                    ClosedAt = c.ClosedAt,

                    ReopenedAt = c.ReopenedAt
                })
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // ============================================================
        // GET COMPLAINT BY ID
        // ============================================================
        public async Task<ComplaintResponse?> GetByIdAsync(Guid id)
        {
            return await BuildComplaintResponseAsync(id);
        }

        // ============================================================
        // ASSIGN COMPLAINT
        // ============================================================
        // Admin assigns the complaint to Maintenance Staff.
        // ============================================================
        public async Task<ComplaintResponse?> AssignAsync(
            Guid complaintId,
            AssignComplaintRequest request)
        {
            var complaint = await _context.Complaints
                .FirstOrDefaultAsync(c => c.Id == complaintId);

            if (complaint == null)
            {
                return null;
            }

            // Check that the selected user exists.
            var maintenanceStaff = await _userManager
                .FindByIdAsync(
                    request.MaintenanceStaffUserId.ToString());

            if (maintenanceStaff == null)
            {
                throw new InvalidOperationException(
                    "The selected maintenance staff member does not exist.");
            }

            // Check that the selected user actually has
            // the MaintenanceStaff role.
            var isMaintenanceStaff =
                await _userManager.IsInRoleAsync(
                    maintenanceStaff,
                    "MaintenanceStaff");

            if (!isMaintenanceStaff)
            {
                throw new InvalidOperationException(
                    "The selected user is not a Maintenance Staff member.");
            }

            // Assign the staff member.
            complaint.AssignedToUserId =
                request.MaintenanceStaffUserId;

            // Change status to Assigned.
            complaint.Status = "Assigned";

            await _context.SaveChangesAsync();

            return await BuildComplaintResponseAsync(
                complaint.Id);
        }

        // ============================================================
        // MAINTENANCE STAFF UPDATES STATUS
        // ============================================================
        public async Task<ComplaintResponse?> UpdateStatusAsync(
            Guid complaintId,
            Guid maintenanceStaffUserId,
            UpdateComplaintStatusRequest request)
        {
            var complaint = await _context.Complaints
                .FirstOrDefaultAsync(c => c.Id == complaintId);

            if (complaint == null)
            {
                return null;
            }

            // Make sure this complaint is assigned to this staff member.
            if (!complaint.AssignedToUserId.HasValue ||
                complaint.AssignedToUserId.Value !=
                maintenanceStaffUserId)
            {
                throw new InvalidOperationException(
                    "This complaint is not assigned to you.");
            }

            var newStatus = request.Status.Trim();

            // Maintenance Staff can only update the complaint
            // to InProgress or Resolved.
            if (!string.Equals(
                    newStatus,
                    "InProgress",
                    StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(
                    newStatus,
                    "Resolved",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Maintenance Staff can only set the complaint status to InProgress or Resolved.");
            }

            // If complaint is already closed, it cannot be changed
            // by Maintenance Staff.
            if (complaint.Status == "Closed")
            {
                throw new InvalidOperationException(
                    "A closed complaint cannot be updated.");
            }

            complaint.Status = newStatus;

            // Record resolution time when staff marks it Resolved.
            if (string.Equals(
                    newStatus,
                    "Resolved",
                    StringComparison.OrdinalIgnoreCase))
            {
                complaint.ResolvedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return await BuildComplaintResponseAsync(
                complaint.Id);
        }

        // ============================================================
        // RESIDENT ACTION
        // ============================================================
        // true  -> Resident is satisfied -> Closed
        // false -> Resident is not satisfied -> Reopened
        // ============================================================
        public async Task<ComplaintResponse?> ResidentActionAsync(
            Guid complaintId,
            Guid applicationUserId,
            ResidentComplaintActionRequest request)
        {
            var complaint = await _context.Complaints
                .FirstOrDefaultAsync(c => c.Id == complaintId);

            if (complaint == null)
            {
                return null;
            }

            // Find the resident linked to the logged-in user.
            var resident = await _context.Residents
                .FirstOrDefaultAsync(
                    r => r.ApplicationUserId == applicationUserId);

            if (resident == null)
            {
                throw new InvalidOperationException(
                    "The logged-in user is not linked to a resident.");
            }

            // Make sure the complaint belongs to this resident.
            if (complaint.ResidentId != resident.Id)
            {
                throw new InvalidOperationException(
                    "You are not authorized to take action on this complaint.");
            }

            // Resident action should only happen after resolution.
            if (complaint.Status != "Resolved")
            {
                throw new InvalidOperationException(
                    "Resident can only take action after the complaint has been resolved.");
            }

            // ========================================================
            // RESIDENT ACCEPTS RESOLUTION
            // ========================================================
            if (request.IsSatisfied)
            {
                complaint.Status = "Closed";

                complaint.ClosedAt = DateTime.UtcNow;
            }

            // ========================================================
            // RESIDENT REOPENS COMPLAINT
            // ========================================================
            else
            {
                complaint.Status = "Reopened";

                complaint.ReopenedAt = DateTime.UtcNow;

                // Clear previous resolution time because the issue
                // is no longer considered resolved.
                complaint.ResolvedAt = null;
            }

            await _context.SaveChangesAsync();

            return await BuildComplaintResponseAsync(
                complaint.Id);
        }

        // ============================================================
        // BUILD COMPLAINT RESPONSE
        // ============================================================
        private async Task<ComplaintResponse?> BuildComplaintResponseAsync(
            Guid complaintId)
        {
            var complaint = await _context.Complaints
                .Include(c => c.Resident)
                .Include(c => c.Property)
                .FirstOrDefaultAsync(
                    c => c.Id == complaintId);

            if (complaint == null)
            {
                return null;
            }

            string? assignedToUserName = null;

            if (complaint.AssignedToUserId.HasValue)
            {
                var assignedUser = await _userManager.FindByIdAsync(
                    complaint.AssignedToUserId.Value.ToString());

                if (assignedUser != null)
                {
                    assignedToUserName =
                        assignedUser.FullName;
                }
            }

            return new ComplaintResponse
            {
                Id = complaint.Id,

                ResidentId = complaint.ResidentId,

                ResidentName = complaint.Resident != null
                    ? complaint.Resident.FullName
                    : string.Empty,

                PropertyId = complaint.PropertyId,

                HouseNumber = complaint.Property != null
                    ? complaint.Property.HouseNumber
                    : string.Empty,

                Block = complaint.Property != null
                    ? complaint.Property.Block
                    : string.Empty,

                Title = complaint.Title,

                Description = complaint.Description,

                Category = complaint.Category,

                Status = complaint.Status,

                AssignedToUserId =
                    complaint.AssignedToUserId,

                AssignedToUserName =
                    assignedToUserName,

                CreatedAt = complaint.CreatedAt,

                ResolvedAt =
                    complaint.ResolvedAt,

                ClosedAt =
                    complaint.ClosedAt,

                ReopenedAt =
                    complaint.ReopenedAt
            };
        }
    }
}
