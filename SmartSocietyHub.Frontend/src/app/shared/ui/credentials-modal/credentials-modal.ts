import { Component, EventEmitter, Input, Output, signal } from '@angular/core';

import { Modal } from '../modal/modal';

// Shown exactly once, right after the backend generates a login account
// with a random temporary password (Resident or Maintenance Staff
// creation). The password is never retrievable again after this, so the
// modal forces an explicit acknowledgement before it can be dismissed.
@Component({
  selector: 'app-credentials-modal',
  standalone: true,
  imports: [Modal],
  templateUrl: './credentials-modal.html',
  styleUrl: './credentials-modal.scss',
})
export class CredentialsModal {
  @Input() personName = '';
  @Input() loginEmail = '';
  @Input() temporaryPassword = '';
  @Output() acknowledged = new EventEmitter<void>();

  protected readonly copied = signal<'email' | 'password' | null>(null);

  protected copy(value: string, field: 'email' | 'password'): void {
    navigator.clipboard?.writeText(value).then(() => {
      this.copied.set(field);
      setTimeout(() => this.copied.set(null), 1500);
    });
  }
}
