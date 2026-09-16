using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SmartSocietyHub.Application.Features.ResidentManagement.DTOs;
using SmartSocietyHub.Application.Features.ResidentManagement.Interfaces;

namespace SmartSocietyHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ResidentController : ControllerBase
    {
        private readonly IResidentService _residentService;

        public ResidentController(IResidentService residentService)
        {
            _residentService = residentService;
        }

        // ============================================================
        // RESIDENT - GET OWN PROFILE
        // ============================================================

        [Authorize(Roles = "Resident")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdClaim, out var applicationUserId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            var resident = await _residentService.GetByApplicationUserIdAsync(
                applicationUserId);

            if (resident == null)
            {
                return NotFound(new
                {
                    message = "No resident profile is linked to this account."
                });
            }

            return Ok(resident);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(
            CreateResidentRequest request)
        {
            try
            {
                var resident = await _residentService.CreateAsync(request);

                return Ok(resident);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var residents = await _residentService.GetAllAsync();

            return Ok(residents);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var resident = await _residentService.GetByIdAsync(id);

            if (resident == null)
            {
                return NotFound(new
                {
                    message = "Resident not found."
                });
            }

            return Ok(resident);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            CreateResidentRequest request)
        {
            try
            {
                var resident =
                    await _residentService.UpdateAsync(id, request);

                if (resident == null)
                {
                    return NotFound(new
                    {
                        message = "Resident not found."
                    });
                }

                return Ok(resident);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            bool deleted;

            try
            {
                deleted = await _residentService.DeleteAsync(id);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Resident not found."
                });
            }

            return Ok(new
            {
                message = "Resident deleted successfully."
            });
        }
    }
}