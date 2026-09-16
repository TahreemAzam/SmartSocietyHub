using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartSocietyHub.Application.Features.FacilityBooking.DTOs;
using SmartSocietyHub.Application.Features.FacilityBooking.Interfaces;
using SmartSocietyHub.Infrastructure.Persistence.Models;

namespace SmartSocietyHub.Infrastructure.Persistence.Services
{
    public class FacilityBookingService : IFacilityBookingService
    {
        private readonly ApplicationDbContext _context;

        public FacilityBookingService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // FACILITY MANAGEMENT - ADMIN
        // ============================================================

        public async Task<FacilityResponse> CreateFacilityAsync(
            CreateFacilityRequest request)
        {
            if (request.OpeningTime >= request.ClosingTime)
            {
                throw new ArgumentException(
                    "Opening time must be earlier than closing time.");
            }

            if (request.BufferMinutes < 0)
            {
                throw new ArgumentException(
                    "Buffer minutes cannot be negative.");
            }

            var existingFacility = await _context.Facilities
                .AnyAsync(f => f.Name.ToLower() == request.Name.ToLower());

            if (existingFacility)
            {
                throw new InvalidOperationException(
                    "A facility with this name already exists.");
            }

            var facility = new Facility
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Description = request.Description,
                Location = request.Location,
                OpeningTime = request.OpeningTime,
                ClosingTime = request.ClosingTime,
                BufferMinutes = request.BufferMinutes,
                IsActive = true
            };

            _context.Facilities.Add(facility);
            await _context.SaveChangesAsync();

            return MapFacilityToResponse(facility);
        }

        public async Task<List<FacilityResponse>> GetAllFacilitiesAsync()
        {
            var facilities = await _context.Facilities
                .OrderBy(f => f.Name)
                .ToListAsync();

            return facilities
                .Select(MapFacilityToResponse)
                .ToList();
        }

        public async Task<FacilityResponse?> GetFacilityByIdAsync(
            Guid facilityId)
        {
            var facility = await _context.Facilities
                .FirstOrDefaultAsync(f => f.Id == facilityId);

            if (facility == null)
            {
                return null;
            }

            return MapFacilityToResponse(facility);
        }

        public async Task<FacilityResponse?> UpdateFacilityAsync(
            Guid facilityId,
            UpdateFacilityRequest request)
        {
            if (request.OpeningTime >= request.ClosingTime)
            {
                throw new ArgumentException(
                    "Opening time must be earlier than closing time.");
            }

            if (request.BufferMinutes < 0)
            {
                throw new ArgumentException(
                    "Buffer minutes cannot be negative.");
            }

            var facility = await _context.Facilities
                .FirstOrDefaultAsync(f => f.Id == facilityId);

            if (facility == null)
            {
                return null;
            }

            var duplicateName = await _context.Facilities
                .AnyAsync(f =>
                    f.Id != facilityId &&
                    f.Name.ToLower() == request.Name.ToLower());

            if (duplicateName)
            {
                throw new InvalidOperationException(
                    "A facility with this name already exists.");
            }

            facility.Name = request.Name.Trim();
            facility.Description = request.Description;
            facility.Location = request.Location;
            facility.OpeningTime = request.OpeningTime;
            facility.ClosingTime = request.ClosingTime;
            facility.BufferMinutes = request.BufferMinutes;
            facility.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return MapFacilityToResponse(facility);
        }

        public async Task<bool> DeleteFacilityAsync(Guid facilityId)
        {
            var facility = await _context.Facilities
                .FirstOrDefaultAsync(f => f.Id == facilityId);

            if (facility == null)
            {
                return false;
            }

            _context.Facilities.Remove(facility);
            await _context.SaveChangesAsync();

            return true;
        }

        // ============================================================
        // FACILITY BOOKING - RESIDENT
        // ============================================================

        public async Task<BookingResponse> CreateBookingAsync(
            Guid applicationUserId,
            CreateBookingRequest request)
        {
            // Find the actual Resident using the logged-in
            // ApplicationUser ID from the JWT token.
            var resident = await _context.Residents
                .FirstOrDefaultAsync(r =>
                    r.ApplicationUserId == applicationUserId &&
                    r.Status == "Active");

            if (resident == null)
            {
                throw new InvalidOperationException(
                    "Active resident was not found for the logged-in user.");
            }

            var facility = await _context.Facilities
                .FirstOrDefaultAsync(f => f.Id == request.FacilityId);

            if (facility == null)
            {
                throw new KeyNotFoundException(
                    "Facility was not found.");
            }

            ValidateBookingDateAndTime(
                facility,
                request.BookingDate,
                request.StartTime,
                request.EndTime);

            var hasConflict = await HasBookingConflictAsync(
                request.FacilityId,
                request.BookingDate,
                request.StartTime,
                request.EndTime);

            if (hasConflict)
            {
                throw new InvalidOperationException(
                    "The selected time slot is not available.");
            }

            var booking = new FacilityBooking
            {
                Id = Guid.NewGuid(),
                FacilityId = request.FacilityId,

                // Use the actual Resident ID here.
                ResidentId = resident.Id,

                BookingDate = request.BookingDate.Date,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Status = "Confirmed",
                CreatedAt = DateTime.UtcNow
            };

            _context.FacilityBookings.Add(booking);
            await _context.SaveChangesAsync();

            return await GetBookingResponseAsync(booking.Id)
                ?? throw new InvalidOperationException(
                    "Booking could not be created.");
        }

        public async Task<List<BookingResponse>> GetMyBookingsAsync(
            Guid applicationUserId)
        {
            // Find the actual Resident using the logged-in
            // ApplicationUser ID.
            var resident = await _context.Residents
                .FirstOrDefaultAsync(r =>
                    r.ApplicationUserId == applicationUserId &&
                    r.Status == "Active");

            if (resident == null)
            {
                throw new InvalidOperationException(
                    "Active resident was not found for the logged-in user.");
            }

            var bookings = await _context.FacilityBookings
                .Include(b => b.Facility)
                .Include(b => b.Resident)
                .Where(b => b.ResidentId == resident.Id)
                .OrderByDescending(b => b.BookingDate)
                .ThenBy(b => b.StartTime)
                .ToListAsync();

            return bookings
                .Select(MapBookingToResponse)
                .ToList();
        }

        public async Task<BookingResponse?> GetMyBookingByIdAsync(
            Guid applicationUserId,
            Guid bookingId)
        {
            // Find the actual Resident using the logged-in
            // ApplicationUser ID.
            var resident = await _context.Residents
                .FirstOrDefaultAsync(r =>
                    r.ApplicationUserId == applicationUserId &&
                    r.Status == "Active");

            if (resident == null)
            {
                throw new InvalidOperationException(
                    "Active resident was not found for the logged-in user.");
            }

            var booking = await _context.FacilityBookings
                .Include(b => b.Facility)
                .Include(b => b.Resident)
                .FirstOrDefaultAsync(b =>
                    b.Id == bookingId &&
                    b.ResidentId == resident.Id);

            if (booking == null)
            {
                return null;
            }

            return MapBookingToResponse(booking);
        }

        public async Task<BookingResponse?> UpdateMyBookingAsync(
            Guid applicationUserId,
            Guid bookingId,
            UpdateBookingRequest request)
        {
            // Find the actual Resident using the logged-in
            // ApplicationUser ID.
            var resident = await _context.Residents
                .FirstOrDefaultAsync(r =>
                    r.ApplicationUserId == applicationUserId &&
                    r.Status == "Active");

            if (resident == null)
            {
                throw new InvalidOperationException(
                    "Active resident was not found for the logged-in user.");
            }

            var booking = await _context.FacilityBookings
                .FirstOrDefaultAsync(b =>
                    b.Id == bookingId &&
                    b.ResidentId == resident.Id);

            if (booking == null)
            {
                return null;
            }

            if (booking.Status != "Confirmed")
            {
                throw new InvalidOperationException(
                    "Only confirmed bookings can be updated.");
            }

            var facility = await _context.Facilities
                .FirstOrDefaultAsync(f => f.Id == request.FacilityId);

            if (facility == null)
            {
                throw new KeyNotFoundException(
                    "Facility was not found.");
            }

            ValidateBookingDateAndTime(
                facility,
                request.BookingDate,
                request.StartTime,
                request.EndTime);

            var hasConflict = await HasBookingConflictAsync(
                request.FacilityId,
                request.BookingDate,
                request.StartTime,
                request.EndTime,
                bookingId);

            if (hasConflict)
            {
                throw new InvalidOperationException(
                    "The selected time slot is not available.");
            }

            booking.FacilityId = request.FacilityId;
            booking.BookingDate = request.BookingDate.Date;
            booking.StartTime = request.StartTime;
            booking.EndTime = request.EndTime;
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetBookingResponseAsync(booking.Id);
        }

        public async Task<bool> CancelMyBookingAsync(
            Guid applicationUserId,
            Guid bookingId)
        {
            // Find the actual Resident using the logged-in
            // ApplicationUser ID.
            var resident = await _context.Residents
                .FirstOrDefaultAsync(r =>
                    r.ApplicationUserId == applicationUserId &&
                    r.Status == "Active");

            if (resident == null)
            {
                throw new InvalidOperationException(
                    "Active resident was not found for the logged-in user.");
            }

            var booking = await _context.FacilityBookings
                .FirstOrDefaultAsync(b =>
                    b.Id == bookingId &&
                    b.ResidentId == resident.Id);

            if (booking == null)
            {
                return false;
            }

            if (booking.Status == "Cancelled")
            {
                throw new InvalidOperationException(
                    "This booking is already cancelled.");
            }

            if (booking.Status == "Completed")
            {
                throw new InvalidOperationException(
                    "Completed bookings cannot be cancelled.");
            }

            booking.Status = "Cancelled";
            booking.CancelledAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // ============================================================
        // BOOKING MANAGEMENT - ADMIN
        // ============================================================

        public async Task<List<BookingResponse>> GetAllBookingsAsync()
        {
            var bookings = await _context.FacilityBookings
                .Include(b => b.Facility)
                .Include(b => b.Resident)
                .OrderByDescending(b => b.BookingDate)
                .ThenBy(b => b.StartTime)
                .ToListAsync();

            return bookings
                .Select(MapBookingToResponse)
                .ToList();
        }

        public async Task<BookingResponse?> GetBookingByIdAsync(
            Guid bookingId)
        {
            return await GetBookingResponseAsync(bookingId);
        }

        // ============================================================
        // VALIDATION & AVAILABILITY
        // ============================================================

        private void ValidateBookingDateAndTime(
            Facility facility,
            DateTime bookingDate,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            if (!facility.IsActive)
            {
                throw new InvalidOperationException(
                    "This facility is currently unavailable.");
            }

            if (bookingDate.Date < DateTime.Today)
            {
                throw new InvalidOperationException(
                    "Bookings cannot be made for a past date.");
            }

            if (startTime >= endTime)
            {
                throw new InvalidOperationException(
                    "Start time must be earlier than end time.");
            }

            if (startTime < facility.OpeningTime ||
                endTime > facility.ClosingTime)
            {
                throw new InvalidOperationException(
                    $"Booking must be within facility opening hours: " +
                    $"{facility.OpeningTime:hh\\:mm} - " +
                    $"{facility.ClosingTime:hh\\:mm}.");
            }

            if (bookingDate.Date == DateTime.Today)
            {
                var currentTime = DateTime.Now.TimeOfDay;

                if (startTime <= currentTime)
                {
                    throw new InvalidOperationException(
                        "Bookings cannot start in the past.");
                }
            }
        }

        private async Task<bool> HasBookingConflictAsync(
            Guid facilityId,
            DateTime bookingDate,
            TimeSpan requestedStart,
            TimeSpan requestedEnd,
            Guid? excludeBookingId = null)
        {
            var bookings = await _context.FacilityBookings
                .Include(b => b.Facility)
                .Where(b =>
                    b.FacilityId == facilityId &&
                    b.BookingDate.Date == bookingDate.Date &&
                    b.Status != "Cancelled" &&
                    (!excludeBookingId.HasValue ||
                     b.Id != excludeBookingId.Value))
                .ToListAsync();

            foreach (var booking in bookings)
            {
                var existingBlockedEnd =
                    booking.EndTime.Add(
                        TimeSpan.FromMinutes(
                            booking.Facility.BufferMinutes));

                // Conflict exists when requested time overlaps
                // the existing booking + its cleaning buffer.
                if (requestedStart < existingBlockedEnd &&
                    requestedEnd > booking.StartTime)
                {
                    return true;
                }
            }

            return false;
        }

        // ============================================================
        // MAPPING
        // ============================================================

        private static FacilityResponse MapFacilityToResponse(
            Facility facility)
        {
            return new FacilityResponse
            {
                Id = facility.Id,
                Name = facility.Name,
                Description = facility.Description,
                Location = facility.Location,
                OpeningTime = facility.OpeningTime,
                ClosingTime = facility.ClosingTime,
                BufferMinutes = facility.BufferMinutes,
                IsActive = facility.IsActive
            };
        }

        private static BookingResponse MapBookingToResponse(
            FacilityBooking booking)
        {
            return new BookingResponse
            {
                Id = booking.Id,
                FacilityId = booking.FacilityId,
                FacilityName = booking.Facility?.Name ?? string.Empty,
                ResidentId = booking.ResidentId,
                ResidentName = booking.Resident?.FullName ?? string.Empty,
                BookingDate = booking.BookingDate,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                Status = booking.Status,
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt,
                CancelledAt = booking.CancelledAt
            };
        }

        private async Task<BookingResponse?> GetBookingResponseAsync(
            Guid bookingId)
        {
            var booking = await _context.FacilityBookings
                .Include(b => b.Facility)
                .Include(b => b.Resident)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return null;
            }

            return MapBookingToResponse(booking);
        }
    }
}