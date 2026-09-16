import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { CreateResidentRequest, CreateResidentResponse, Resident } from '../models/resident.models';

@Injectable({ providedIn: 'root' })
export class ResidentApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/Resident`;

  getMe(): Observable<Resident> {
    return this.http.get<Resident>(`${this.baseUrl}/me`);
  }

  getAll(): Observable<Resident[]> {
    return this.http.get<Resident[]>(this.baseUrl);
  }

  getById(id: string): Observable<Resident> {
    return this.http.get<Resident>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateResidentRequest): Observable<CreateResidentResponse> {
    return this.http.post<CreateResidentResponse>(this.baseUrl, request);
  }

  update(id: string, request: CreateResidentRequest): Observable<Resident> {
    return this.http.put<Resident>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.baseUrl}/${id}`);
  }
}
