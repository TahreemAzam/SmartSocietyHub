import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { CommunityUpdate, CreateUpdateRequest, UpdateUpdateRequest } from '../models/update.models';

@Injectable({ providedIn: 'root' })
export class UpdateApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/Update`;

  // Admin
  create(request: CreateUpdateRequest): Observable<CommunityUpdate> {
    return this.http.post<CommunityUpdate>(this.baseUrl, request);
  }

  getAll(): Observable<CommunityUpdate[]> {
    return this.http.get<CommunityUpdate[]>(this.baseUrl);
  }

  update(id: string, request: UpdateUpdateRequest): Observable<CommunityUpdate> {
    return this.http.put<CommunityUpdate>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.baseUrl}/${id}`);
  }

  publish(id: string): Observable<CommunityUpdate> {
    return this.http.patch<CommunityUpdate>(`${this.baseUrl}/${id}/publish`, {});
  }

  unpublish(id: string): Observable<CommunityUpdate> {
    return this.http.patch<CommunityUpdate>(`${this.baseUrl}/${id}/unpublish`, {});
  }

  // Resident / SecurityGuard / MaintenanceStaff
  getPublished(): Observable<CommunityUpdate[]> {
    return this.http.get<CommunityUpdate[]>(`${this.baseUrl}/published`);
  }

  getUpcomingEvents(): Observable<CommunityUpdate[]> {
    return this.http.get<CommunityUpdate[]>(`${this.baseUrl}/events/upcoming`);
  }

  getUnseenCount(): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.baseUrl}/unseen-count`);
  }

  getRecentUnseen(): Observable<CommunityUpdate[]> {
    return this.http.get<CommunityUpdate[]>(`${this.baseUrl}/recent-unseen`);
  }

  markAsSeen(id: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.baseUrl}/${id}/seen`, {});
  }
}
