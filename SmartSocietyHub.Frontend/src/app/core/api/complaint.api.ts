import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  AssignComplaintRequest,
  Complaint,
  CreateComplaintRequest,
  ResidentComplaintActionRequest,
  UpdateComplaintStatusRequest,
} from '../models/complaint.models';

@Injectable({ providedIn: 'root' })
export class ComplaintApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/Complaint`;

  // Resident
  create(request: CreateComplaintRequest): Observable<Complaint> {
    return this.http.post<Complaint>(this.baseUrl, request);
  }

  getMy(): Observable<Complaint[]> {
    return this.http.get<Complaint[]>(`${this.baseUrl}/my`);
  }

  residentAction(id: string, request: ResidentComplaintActionRequest): Observable<Complaint> {
    return this.http.put<Complaint>(`${this.baseUrl}/${id}/resident-action`, request);
  }

  // Admin
  getAll(): Observable<Complaint[]> {
    return this.http.get<Complaint[]>(this.baseUrl);
  }

  assign(id: string, request: AssignComplaintRequest): Observable<Complaint> {
    return this.http.put<Complaint>(`${this.baseUrl}/${id}/assign`, request);
  }

  // Maintenance Staff
  getAssignedToMe(): Observable<Complaint[]> {
    return this.http.get<Complaint[]>(`${this.baseUrl}/assigned`);
  }

  updateStatus(id: string, request: UpdateComplaintStatusRequest): Observable<Complaint> {
    return this.http.put<Complaint>(`${this.baseUrl}/${id}/status`, request);
  }
}
