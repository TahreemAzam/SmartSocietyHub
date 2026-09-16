using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartSocietyHub.Application.Features.FacilityBooking.DTOs;

namespace SmartSocietyHub.Application.Features.FacilityBooking.Interfaces
{
    public interface IFacilityBookingService
    {
        // ============================================================
        // FACILITY MANAGEMENT - ADMIN
        // ============================================================

        Task<FacilityResponse> CreateFacilityAsync(
            CreateFacilityRequest request);

        Task<List<FacilityResponse>> GetAllFacilitiesAsync();

        Task<FacilityResponse?> GetFacilityByIdAsync(
            Guid facilityId);

        Task<FacilityResponse?> UpdateFacilityAsync(
            Guid facilityId,
            UpdateFacilityRequest request);

        Task<bool> DeleteFacilityAsync(
            Guid facilityId);

        // ============================================================
        // FACILITY BOOKING - RESIDENT
        // ============================================================

        Task<BookingResponse> CreateBookingAsync(
            Guid residentId,
            CreateBookingRequest request);

        Task<List<BookingResponse>> GetMyBookingsAsync(
            Guid residentId);

        Task<BookingResponse?> GetMyBookingByIdAsync(
            Guid residentId,
            Guid bookingId);

        Task<BookingResponse?> UpdateMyBookingAsync(
            Guid residentId,
            Guid bookingId,
            UpdateBookingRequest request);

        Task<bool> CancelMyBookingAsync(
            Guid residentId,
            Guid bookingId);

        // ============================================================
        // BOOKING MANAGEMENT - ADMIN
        // ============================================================

        Task<List<BookingResponse>> GetAllBookingsAsync();

        Task<BookingResponse?> GetBookingByIdAsync(
            Guid bookingId);
    }
}