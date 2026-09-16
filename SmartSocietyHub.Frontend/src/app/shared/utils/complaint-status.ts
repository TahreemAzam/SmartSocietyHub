import { ComplaintStatus } from '../../core/models/complaint.models';

const BADGE_CLASS: Record<ComplaintStatus, string> = {
  Open: 'badge-warning',
  Assigned: 'badge-info',
  InProgress: 'badge-gold',
  Resolved: 'badge-success',
  Closed: 'badge-neutral',
  Reopened: 'badge-danger',
};

export function complaintStatusBadgeClass(status: string): string {
  return BADGE_CLASS[status as ComplaintStatus] ?? 'badge-neutral';
}
