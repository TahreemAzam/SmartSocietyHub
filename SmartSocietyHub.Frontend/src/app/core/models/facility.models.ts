// Mirrors SmartSocietyHub.Application.Features.FacilityBooking.DTOs exactly.
// Time fields are "HH:mm:ss" strings (backend TimeSpan serialization).

export interface Facility {
  id: string;
  name: string;
  description?: string | null;
  location?: string | null;
  openingTime: string;
  closingTime: string;
  bufferMinutes: number;
  isActive: boolean;
}

export interface CreateFacilityRequest {
  name: string;
  description?: string | null;
  location?: string | null;
  openingTime: string;
  closingTime: string;
  bufferMinutes: number;
}

export interface UpdateFacilityRequest extends CreateFacilityRequest {
  isActive: boolean;
}

export type BookingStatus = 'Confirmed' | 'Cancelled' | 'Completed';

export interface Booking {
  id: string;
  facilityId: string;
  facilityName: string;
  residentId: string;
  residentName: string;
  bookingDate: string;
  startTime: string;
  endTime: string;
  status: BookingStatus;
  createdAt: string;
  updatedAt?: string | null;
  cancelledAt?: string | null;
}

export interface CreateBookingRequest {
  facilityId: string;
  bookingDate: string;
  startTime: string;
  endTime: string;
}

export type UpdateBookingRequest = CreateBookingRequest;
