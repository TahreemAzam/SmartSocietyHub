using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SmartSocietyHub.Application.Features.StaffManagement.DTOs;
using SmartSocietyHub.Application.Features.StaffManagement.Interfaces;

namespace SmartSocietyHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;

        public StaffController(
            IStaffService staffService)
        {
            _staffService = staffService;
        }

        // ============================================================
        // CREATE MAINTENANCE STAFF
        // ============================================================

        [HttpPost("maintenance")]
        public async Task<IActionResult> CreateMaintenanceStaff(
            CreateMaintenanceStaffRequest request)
        {
            try
            {
                var staff =
                    await _staffService.CreateMaintenanceStaffAsync(
                        request);

                return Ok(staff);
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
        // GET ALL MAINTENANCE STAFF
        // ============================================================

        [HttpGet("maintenance")]
        public async Task<IActionResult> GetAllMaintenanceStaff()
        {
            var staff = await _staffService.GetAllMaintenanceStaffAsync();

            return Ok(staff);
        }
    }
}