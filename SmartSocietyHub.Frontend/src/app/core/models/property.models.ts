// Mirrors SmartSocietyHub.Application.Features.PropertyManagement.DTOs exactly.

export type PropertyStatus = 'Vacant' | 'Occupied';

export interface Property {
  id: string;
  houseNumber: string;
  block: string;
  status: PropertyStatus;
}

export interface CreatePropertyRequest {
  houseNumber: string;
  block: string;
  status: PropertyStatus;
}
