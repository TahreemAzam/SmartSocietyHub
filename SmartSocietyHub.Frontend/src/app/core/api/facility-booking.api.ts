import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  Booking,
  CreateBookingRequest,
  CreateFacilityRequest,
  Facility,
  UpdateBookingRequest,
  UpdateFacilityRequest,
} from '../models/facility.models';

@Injectable({ providedIn: 'root' })
export class FacilityBookingApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/FacilityBooking`;

  // Facilities
  getAllFacilities(): Observable<Facility[]> {
    return this.http.get<Facility[]>(`${this.baseUrl}/facilities`);
  }

  createFacility(request: CreateFacilityRequest): Observable<Facility> {
    return this.http.post<Facility>(`${this.baseUrl}/facilities`, request);
  }

  updateFacility(id: string, request: UpdateFacilityRequest): Observable<Facility> {
    return this.http.put<Facility>(`${this.baseUrl}/facilities/${id}`, request);
  }

  deleteFacility(id: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.baseUrl}/facilities/${id}`);
  }

  // Resident bookings
  createBooking(request: CreateBookingRequest): Observable<Booking> {
    return this.http.post<Booking>(`${this.baseUrl}/bookings`, request);
  }

  getMyBookings(): Observable<Booking[]> {
    return this.http.get<Booking[]>(`${this.baseUrl}/bookings/my`);
  }

  updateMyBooking(id: string, request: UpdateBookingRequest): Observable<Booking> {
    return this.http.put<Booking>(`${this.baseUrl}/bookings/my/${id}`, request);
  }

  cancelMyBooking(id: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.baseUrl}/bookings/my/${id}`);
  }

  // Admin bookings
  getAllBookings(): Observable<Booking[]> {
    return this.http.get<Booking[]>(`${this.baseUrl}/admin/bookings`);
  }
}
