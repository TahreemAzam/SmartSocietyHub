// Mirrors SmartSocietyHub.Application.Features.StaffManagement.DTOs exactly.

export interface MaintenanceStaff {
  userId: string;
  fullName: string;
  email: string;
  phoneNumber: string;
}

export interface CreateMaintenanceStaffRequest {
  fullName: string;
  email: string;
  phoneNumber: string;
}

export interface CreateMaintenanceStaffResponse {
  userId: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  role: string;
  temporaryPassword: string;
}
