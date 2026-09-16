using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SmartSocietyHub.Application.Features.ComplaintManagement.DTOs;
using SmartSocietyHub.Application.Features.ComplaintManagement.Interfaces;

namespace SmartSocietyHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComplaintController : ControllerBase
    {
        private readonly IComplaintService _complaintService;

        public ComplaintController(IComplaintService complaintService)
        {
            _complaintService = complaintService;
        }

        // ============================================================
        // RESIDENT - CREATE COMPLAINT
        // ============================================================

        [Authorize(Roles = "Resident")]
        [HttpPost]
        public async Task<IActionResult> Create(
            CreateComplaintRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(userIdClaim, out var applicationUserId))
                {
                    return Unauthorized(new
                    {
                        message = "Invalid user identity."
                    });
                }

                var complaint =
                    await _complaintService.CreateAsync(
                        applicationUserId,
                        request);

                return Ok(complaint);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        // ============================================================
        // RESIDENT - GET MY COMPLAINTS
        // ============================================================

        [Authorize(Roles = "Resident")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyComplaints()
        {
            try
            {
                var userIdClaim = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(userIdClaim, out var applicationUserId))
                {
                    return Unauthorized(new
                    {
                        message = "Invalid user identity."
                    });
                }

                var complaints =
                    await _complaintService.GetMyComplaintsAsync(
                        applicationUserId);

                return Ok(complaints);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
        }

        // ============================================================
        // ADMIN - GET ALL COMPLAINTS
        // ============================================================

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var complaints =
                await _complaintService.GetAllAsync();

            return Ok(complaints);
        }

        // ============================================================
        // MAINTENANCE STAFF - GET COMPLAINTS ASSIGNED TO ME
        // ============================================================

        [Authorize(Roles = "MaintenanceStaff")]
        [HttpGet("assigned")]
        public async Task<IActionResult> GetAssignedToMe()
        {
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdClaim, out var maintenanceStaffUserId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            var complaints =
                await _complaintService.GetAssignedToMeAsync(
                    maintenanceStaffUserId);

            return Ok(complaints);
        }

        // ============================================================
        // ADMIN - GET COMPLAINT BY ID
        // ============================================================

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var complaint =
                await _complaintService.GetByIdAsync(id);

            if (complaint == null)
            {
                return NotFound(new
                {
                    message = "Complaint not found."
                });
            }

            return Ok(complaint);
        }

        // ============================================================
        // ADMIN - ASSIGN MAINTENANCE STAFF
        // ============================================================

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}/assign")]
        public async Task<IActionResult> Assign(
            Guid id,
            AssignComplaintRequest request)
        {
            try
            {
                var complaint =
                    await _complaintService.AssignAsync(
                        id,
                        request);

                if (complaint == null)
                {
                    return NotFound(new
                    {
                        message = "Complaint not found."
                    });
                }

                return Ok(complaint);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        // ============================================================
        // MAINTENANCE STAFF - UPDATE COMPLAINT STATUS
        // ============================================================

        [Authorize(Roles = "MaintenanceStaff")]
        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            UpdateComplaintStatusRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(userIdClaim, out var maintenanceStaffUserId))
                {
                    return Unauthorized(new
                    {
                        message = "Invalid user identity."
                    });
                }

                var complaint =
                    await _complaintService.UpdateStatusAsync(
                        id,
                        maintenanceStaffUserId,
                        request);

                if (complaint == null)
                {
                    return NotFound(new
                    {
                        message = "Complaint not found."
                    });
                }

                return Ok(complaint);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        // ============================================================
        // RESIDENT - ACCEPT RESOLUTION OR REOPEN COMPLAINT
        // ============================================================

        [Authorize(Roles = "Resident")]
        [HttpPut("{id:guid}/resident-action")]
        public async Task<IActionResult> ResidentAction(
            Guid id,
            ResidentComplaintActionRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(userIdClaim, out var applicationUserId))
                {
                    return Unauthorized(new
                    {
                        message = "Invalid user identity."
                    });
                }

                var complaint =
                    await _complaintService.ResidentActionAsync(
                        id,
                        applicationUserId,
                        request);

                if (complaint == null)
                {
                    return NotFound(new
                    {
                        message = "Complaint not found."
                    });
                }

                return Ok(complaint);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }
    }
}