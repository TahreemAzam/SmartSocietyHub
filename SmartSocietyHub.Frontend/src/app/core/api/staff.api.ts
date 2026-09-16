import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  CreateMaintenanceStaffRequest,
  CreateMaintenanceStaffResponse,
  MaintenanceStaff,
} from '../models/staff.models';

@Injectable({ providedIn: 'root' })
export class StaffApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/Staff`;

  getAllMaintenanceStaff(): Observable<MaintenanceStaff[]> {
    return this.http.get<MaintenanceStaff[]>(`${this.baseUrl}/maintenance`);
  }

  createMaintenanceStaff(
    request: CreateMaintenanceStaffRequest,
  ): Observable<CreateMaintenanceStaffResponse> {
    return this.http.post<CreateMaintenanceStaffResponse>(`${this.baseUrl}/maintenance`, request);
  }
}
