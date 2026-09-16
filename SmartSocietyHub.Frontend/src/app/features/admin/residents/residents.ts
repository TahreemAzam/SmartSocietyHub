import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';

import { PropertyApi } from '../../../core/api/property.api';
import { ResidentApi } from '../../../core/api/resident.api';
import { Property } from '../../../core/models/property.models';
import { CreateResidentResponse, Resident } from '../../../core/models/resident.models';
import { ConfirmService } from '../../../shared/ui/confirm-dialog/confirm.service';
import { CredentialsModal } from '../../../shared/ui/credentials-modal/credentials-modal';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { extractErrorMessage } from '../../../shared/utils/api-error';
import { ResidentFormModal, ResidentSaveResult } from './resident-form-modal';

@Component({
  selector: 'app-residents',
  standalone: true,
  imports: [PageHeader, EmptyState, ResidentFormModal, CredentialsModal],
  templateUrl: './residents.html',
  styleUrl: './residents.scss',
})
export class Residents implements OnInit {
  private readonly residentApi = inject(ResidentApi);
  private readonly propertyApi = inject(PropertyApi);
  private readonly toast = inject(ToastService);
  private readonly confirmService = inject(ConfirmService);

  protected readonly residents = signal<Resident[]>([]);
  protected readonly properties = signal<Property[]>([]);
  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);
  protected readonly searchTerm = signal('');

  protected readonly showForm = signal(false);
  protected readonly editingResident = signal<Resident | null>(null);
  protected readonly newCredentials = signal<CreateResidentResponse | null>(null);

  protected readonly filteredResidents = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();

    if (!term) {
      return this.residents();
    }

    return this.residents().filter(
      (r) =>
        r.fullName.toLowerCase().includes(term) ||
        r.cnic.toLowerCase().includes(term) ||
        r.houseNumber.toLowerCase().includes(term) ||
        r.block.toLowerCase().includes(term),
    );
  });

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.loadError.set(null);

    forkJoin({
      residents: this.residentApi.getAll(),
      properties: this.propertyApi.getAll(),
    }).subscribe({
      next: ({ residents, properties }) => {
        this.residents.set(residents);
        this.properties.set(properties);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loadError.set(extractErrorMessage(error, 'Unable to load residents.'));
        this.loading.set(false);
      },
    });
  }

  protected openCreateForm(): void {
    this.editingResident.set(null);
    this.showForm.set(true);
  }

  protected openEditForm(resident: Resident): void {
    this.editingResident.set(resident);
    this.showForm.set(true);
  }

  protected onSaved(result: ResidentSaveResult): void {
    this.showForm.set(false);

    if (result.mode === 'create') {
      this.newCredentials.set(result.response);
      this.toast.success('Resident created successfully.');
    } else {
      this.toast.success('Resident updated successfully.');
    }

    this.load();
  }

  protected async deleteResident(resident: Resident): Promise<void> {
    const confirmed = await this.confirmService.confirm({
      title: 'Delete resident',
      message: `Delete ${resident.fullName}? This will also delete their login account and release House ${resident.houseNumber}, Block ${resident.block}. This cannot be undone.`,
      confirmLabel: 'Delete',
      danger: true,
    });

    if (!confirmed) {
      return;
    }

    this.residentApi.delete(resident.id).subscribe({
      next: () => {
        this.toast.success('Resident deleted successfully.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.toast.error(extractErrorMessage(error, 'Unable to delete this resident.'));
      },
    });
  }
}
