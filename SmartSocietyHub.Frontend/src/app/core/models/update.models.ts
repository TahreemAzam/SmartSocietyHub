// Mirrors SmartSocietyHub.Application.Features.Updates.DTOs exactly.

export type UpdateType = 'Announcement' | 'Event';

export interface CommunityUpdate {
  id: string;
  title: string;
  description: string;
  type: UpdateType;
  isPublished: boolean;
  publishedAt?: string | null;
  eventDate?: string | null;
  startTime?: string | null;
  endTime?: string | null;
  location?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateUpdateRequest {
  title: string;
  description: string;
  type: UpdateType;
  eventDate?: string | null;
  startTime?: string | null;
  endTime?: string | null;
  location?: string | null;
}

export type UpdateUpdateRequest = CreateUpdateRequest;
