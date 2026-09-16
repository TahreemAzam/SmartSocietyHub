// Mirrors SmartSocietyHub.Application.Features.ComplaintManagement.DTOs exactly.

export type ComplaintStatus =
  | 'Open'
  | 'Assigned'
  | 'InProgress'
  | 'Resolved'
  | 'Closed'
  | 'Reopened';

export interface Complaint {
  id: string;
  residentId: string;
  residentName: string;
  propertyId: string;
  houseNumber: string;
  block: string;
  title: string;
  description: string;
  category: string;
  status: ComplaintStatus;
  assignedToUserId?: string | null;
  assignedToUserName?: string | null;
  createdAt: string;
  resolvedAt?: string | null;
  closedAt?: string | null;
  reopenedAt?: string | null;
}

export interface CreateComplaintRequest {
  title: string;
  description: string;
  category: string;
}

export interface AssignComplaintRequest {
  maintenanceStaffUserId: string;
}

export interface UpdateComplaintStatusRequest {
  status: 'InProgress' | 'Resolved';
}

export interface ResidentComplaintActionRequest {
  isSatisfied: boolean;
}
