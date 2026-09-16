using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SmartSocietyHub.Application.Features.Updates.DTOs;
using SmartSocietyHub.Application.Features.Updates.Interfaces;

namespace SmartSocietyHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UpdateController : ControllerBase
    {
        private readonly IUpdateService _updateService;

        public UpdateController(IUpdateService updateService)
        {
            _updateService = updateService;
        }


        // ============================================================
        // ADMIN - UPDATE MANAGEMENT
        // ============================================================

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUpdate(
            [FromBody] CreateUpdateRequest request)
        {
            try
            {
                var result =
                    await _updateService.CreateUpdateAsync(request);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUpdates()
        {
            var result =
                await _updateService.GetAllUpdatesAsync();

            return Ok(result);
        }


        [HttpGet("{updateId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUpdateById(
            Guid updateId)
        {
            var result =
                await _updateService.GetUpdateByIdAsync(updateId);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Update was not found."
                });
            }

            return Ok(result);
        }


        [HttpPut("{updateId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUpdate(
            Guid updateId,
            [FromBody] UpdateUpdateRequest request)
        {
            try
            {
                var result =
                    await _updateService.UpdateUpdateAsync(
                        updateId,
                        request);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Update was not found."
                    });
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }


        [HttpDelete("{updateId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUpdate(
            Guid updateId)
        {
            var deleted =
                await _updateService.DeleteUpdateAsync(updateId);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Update was not found."
                });
            }

            return Ok(new
            {
                message = "Update deleted successfully."
            });
        }


        [HttpPatch("{updateId:guid}/publish")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PublishUpdate(
            Guid updateId)
        {
            var result =
                await _updateService.PublishUpdateAsync(updateId);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Update was not found."
                });
            }

            return Ok(result);
        }


        [HttpPatch("{updateId:guid}/unpublish")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnpublishUpdate(
            Guid updateId)
        {
            var result =
                await _updateService.UnpublishUpdateAsync(updateId);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Update was not found."
                });
            }

            return Ok(result);
        }


        // ============================================================
        // PUBLISHED UPDATES - NON-ADMIN USERS
        // ============================================================

        [HttpGet("published")]
        [Authorize(
            Roles = "Resident,SecurityGuard,MaintenanceStaff")]
        public async Task<IActionResult> GetPublishedUpdates()
        {
            var result =
                await _updateService.GetPublishedUpdatesAsync();

            return Ok(result);
        }


        [HttpGet("published/{updateId:guid}")]
        [Authorize(
            Roles = "Resident,SecurityGuard,MaintenanceStaff")]
        public async Task<IActionResult> GetPublishedUpdateById(
            Guid updateId)
        {
            var result =
                await _updateService.GetPublishedUpdateByIdAsync(
                    updateId);

            if (result == null)
            {
                return NotFound(new
                {
                    message =
                        "Published update was not found."
                });
            }

            return Ok(result);
        }


        [HttpGet("events/upcoming")]
        [Authorize(
            Roles = "Resident,SecurityGuard,MaintenanceStaff")]
        public async Task<IActionResult> GetUpcomingEvents()
        {
            var result =
                await _updateService.GetUpcomingEventsAsync();

            return Ok(result);
        }


        // ============================================================
        // USER-SPECIFIC UNSEEN UPDATES
        // ============================================================

        [HttpGet("unseen-count")]
        [Authorize(
            Roles = "Resident,SecurityGuard,MaintenanceStaff")]
        public async Task<IActionResult> GetUnseenUpdateCount()
        {
            try
            {
                Guid userId = GetCurrentUserId();

                var count =
                    await _updateService.GetUnseenUpdateCountAsync(
                        userId);

                return Ok(new
                {
                    count
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    message = ex.Message
                });
            }
        }


        [HttpGet("recent-unseen")]
        [Authorize(
            Roles = "Resident,SecurityGuard,MaintenanceStaff")]
        public async Task<IActionResult> GetRecentUnseenUpdates()
        {
            try
            {
                Guid userId = GetCurrentUserId();

                var result =
                    await _updateService.GetRecentUnseenUpdatesAsync(
                        userId);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    message = ex.Message
                });
            }
        }


        [HttpPost("{updateId:guid}/seen")]
        [Authorize(
            Roles = "Resident,SecurityGuard,MaintenanceStaff")]
        public async Task<IActionResult> MarkUpdateAsSeen(
            Guid updateId)
        {
            try
            {
                Guid userId = GetCurrentUserId();

                var marked =
                    await _updateService.MarkUpdateAsSeenAsync(
                        userId,
                        updateId);

                if (!marked)
                {
                    return NotFound(new
                    {
                        message =
                            "Published update was not found."
                    });
                }

                return Ok(new
                {
                    message =
                        "Update marked as seen successfully."
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    message = ex.Message
                });
            }
        }


        // ============================================================
        // CURRENT USER
        // ============================================================

        private Guid GetCurrentUserId()
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException(
                    "User ID was not found in the token.");
            }

            return Guid.Parse(userId);
        }
    }
}