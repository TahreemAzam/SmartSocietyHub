using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSocietyHub.Application.Features.FacilityBooking.DTOs;
using SmartSocietyHub.Application.Features.FacilityBooking.Interfaces;
using System.Security.Claims;

namespace SmartSocietyHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FacilityBookingController : ControllerBase
    {
        private readonly IFacilityBookingService _facilityBookingService;

        public FacilityBookingController(
            IFacilityBookingService facilityBookingService)
        {
            _facilityBookingService = facilityBookingService;
        }

        // ============================================================
        // HELPER - GET LOGGED-IN RESIDENT ID
        // ============================================================

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException(
                    "User ID was not found in the token.");
            }

            return Guid.Parse(userId);
        }

        // ============================================================
        // FACILITY MANAGEMENT - ADMIN
        // ============================================================

        [HttpPost("facilities")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateFacility(
            [FromBody] CreateFacilityRequest request)
        {
            try
            {
                var result =
                    await _facilityBookingService
                        .CreateFacilityAsync(request);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet("facilities")]
        public async Task<IActionResult> GetAllFacilities()
        {
            var result =
                await _facilityBookingService
                    .GetAllFacilitiesAsync();

            return Ok(result);
        }

        [HttpGet("facilities/{facilityId:guid}")]
        public async Task<IActionResult> GetFacilityById(
            Guid facilityId)
        {
            var result =
                await _facilityBookingService
                    .GetFacilityByIdAsync(facilityId);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Facility was not found."
                });
            }

            return Ok(result);
        }

        [HttpPut("facilities/{facilityId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateFacility(
            Guid facilityId,
            [FromBody] UpdateFacilityRequest request)
        {
            try
            {
                var result =
                    await _facilityBookingService
                        .UpdateFacilityAsync(
                            facilityId,
                            request);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Facility was not found."
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
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpDelete("facilities/{facilityId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFacility(
            Guid facilityId)
        {
            var result =
                await _facilityBookingService
                    .DeleteFacilityAsync(facilityId);

            if (!result)
            {
                return NotFound(new
                {
                    message = "Facility was not found."
                });
            }

            return Ok(new
            {
                message = "Facility deleted successfully."
            });
        }

        // ============================================================
        // BOOKING MANAGEMENT - RESIDENT
        // ============================================================

        [HttpPost("bookings")]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> CreateBooking(
            [FromBody] CreateBookingRequest request)
        {
            try
            {
                var residentId = GetCurrentUserId();

                var result =
                    await _facilityBookingService
                        .CreateBookingAsync(
                            residentId,
                            request);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet("bookings/my")]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> GetMyBookings()
        {
            var residentId = GetCurrentUserId();

            var result =
                await _facilityBookingService
                    .GetMyBookingsAsync(residentId);

            return Ok(result);
        }

        [HttpGet("bookings/my/{bookingId:guid}")]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> GetMyBookingById(
            Guid bookingId)
        {
            var residentId = GetCurrentUserId();

            var result =
                await _facilityBookingService
                    .GetMyBookingByIdAsync(
                        residentId,
                        bookingId);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Booking was not found."
                });
            }

            return Ok(result);
        }

        [HttpPut("bookings/my/{bookingId:guid}")]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> UpdateMyBooking(
            Guid bookingId,
            [FromBody] UpdateBookingRequest request)
        {
            try
            {
                var residentId = GetCurrentUserId();

                var result =
                    await _facilityBookingService
                        .UpdateMyBookingAsync(
                            residentId,
                            bookingId,
                            request);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Booking was not found."
                    });
                }

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpDelete("bookings/my/{bookingId:guid}")]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> CancelMyBooking(
            Guid bookingId)
        {
            try
            {
                var residentId = GetCurrentUserId();

                var result =
                    await _facilityBookingService
                        .CancelMyBookingAsync(
                            residentId,
                            bookingId);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Booking was not found."
                    });
                }

                return Ok(new
                {
                    message = "Booking cancelled successfully."
                });
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
        // BOOKING MANAGEMENT - ADMIN
        // ============================================================

        [HttpGet("admin/bookings")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllBookings()
        {
            var result =
                await _facilityBookingService
                    .GetAllBookingsAsync();

            return Ok(result);
        }

        [HttpGet("admin/bookings/{bookingId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBookingById(
            Guid bookingId)
        {
            var result =
                await _facilityBookingService
                    .GetBookingByIdAsync(bookingId);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Booking was not found."
                });
            }

            return Ok(result);
        }
    }
}