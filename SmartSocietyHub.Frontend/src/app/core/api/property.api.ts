import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { CreatePropertyRequest, Property } from '../models/property.models';

@Injectable({ providedIn: 'root' })
export class PropertyApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/Property`;

  getAll(): Observable<Property[]> {
    return this.http.get<Property[]>(this.baseUrl);
  }

  getById(id: string): Observable<Property> {
    return this.http.get<Property>(`${this.baseUrl}/${id}`);
  }

  create(request: CreatePropertyRequest): Observable<Property> {
    return this.http.post<Property>(this.baseUrl, request);
  }

  update(id: string, request: CreatePropertyRequest): Observable<Property> {
    return this.http.put<Property>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.baseUrl}/${id}`);
  }
}
