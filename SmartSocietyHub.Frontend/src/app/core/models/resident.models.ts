// Mirrors SmartSocietyHub.Application.Features.ResidentManagement.DTOs exactly.

export interface FamilyMember {
  id: string;
  fullName: string;
  cnic: string;
  phoneNumber: string;
  email?: string | null;
  dateOfBirth?: string | null;
  gender: string;
  relationship: string;
}

export interface FamilyMemberRequest {
  fullName: string;
  cnic: string;
  phoneNumber: string;
  email?: string | null;
  dateOfBirth?: string | null;
  gender: string;
  relationship: string;
}

export type ResidentStatus = 'Active' | 'Inactive';

export interface Resident {
  id: string;
  fullName: string;
  cnic: string;
  phoneNumber: string;
  email?: string | null;
  dateOfBirth?: string | null;
  gender: string;
  propertyId: string;
  houseNumber: string;
  block: string;
  status: string;
  numberOfFamilyMembers: number;
  familyMembers: FamilyMember[];
}

export interface CreateResidentRequest {
  fullName: string;
  cnic: string;
  phoneNumber: string;
  email?: string | null;
  dateOfBirth?: string | null;
  gender: string;
  propertyId: string;
  status: string;
  numberOfFamilyMembers: number;
  familyMembers: FamilyMemberRequest[];
}

export interface CreateResidentResponse {
  resident: Resident;
  loginEmail: string;
  temporaryPassword: string;
}
