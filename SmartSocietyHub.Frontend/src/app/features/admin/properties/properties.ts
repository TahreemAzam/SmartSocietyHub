import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { PropertyApi } from '../../../core/api/property.api';
import { CreatePropertyRequest, Property, PropertyStatus } from '../../../core/models/property.models';
import { ConfirmService } from '../../../shared/ui/confirm-dialog/confirm.service';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { Modal } from '../../../shared/ui/modal/modal';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { extractErrorMessage } from '../../../shared/utils/api-error';

interface PropertyForm {
  houseNumber: FormControl<string>;
  block: FormControl<string>;
  status: FormControl<PropertyStatus>;
}

@Component({
  selector: 'app-properties',
  standalone: true,
  imports: [ReactiveFormsModule, PageHeader, EmptyState, Modal],
  templateUrl: './properties.html',
  styleUrl: './properties.scss',
})
export class Properties implements OnInit {
  private readonly propertyApi = inject(PropertyApi);
  private readonly toast = inject(ToastService);
  private readonly confirmService = inject(ConfirmService);

  protected readonly properties = signal<Property[]>([]);
  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);
  protected readonly searchTerm = signal('');

  protected readonly showForm = signal(false);
  protected readonly editingProperty = signal<Property | null>(null);
  protected readonly saving = signal(false);
  protected readonly formError = signal<string | null>(null);

  protected readonly filteredProperties = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();

    if (!term) {
      return this.properties();
    }

    return this.properties().filter(
      (p) => p.houseNumber.toLowerCase().includes(term) || p.block.toLowerCase().includes(term),
    );
  });

  protected readonly vacantCount = computed(
    () => this.properties().filter((p) => p.status === 'Vacant').length,
  );

  protected readonly occupiedCount = computed(
    () => this.properties().filter((p) => p.status === 'Occupied').length,
  );

  protected readonly form = new FormGroup<PropertyForm>({
    houseNumber: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    block: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    status: new FormControl<PropertyStatus>('Vacant', { nonNullable: true, validators: [Validators.required] }),
  });

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.loadError.set(null);

    this.propertyApi.getAll().subscribe({
      next: (properties) => {
        this.properties.set(properties);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loadError.set(extractErrorMessage(error, 'Unable to load properties.'));
        this.loading.set(false);
      },
    });
  }

  protected openCreateForm(): void {
    this.editingProperty.set(null);
    this.formError.set(null);
    this.form.reset({ houseNumber: '', block: '', status: 'Vacant' });
    this.showForm.set(true);
  }

  protected openEditForm(property: Property): void {
    this.editingProperty.set(property);
    this.formError.set(null);
    this.form.reset({
      houseNumber: property.houseNumber,
      block: property.block,
      status: property.status,
    });
    this.showForm.set(true);
  }

  protected closeForm(): void {
    if (this.saving()) {
      return;
    }

    this.showForm.set(false);
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.formError.set(null);
    this.saving.set(true);

    const request: CreatePropertyRequest = this.form.getRawValue();
    const editing = this.editingProperty();

    const call = editing
      ? this.propertyApi.update(editing.id, request)
      : this.propertyApi.create(request);

    call.subscribe({
      next: () => {
        this.saving.set(false);
        this.showForm.set(false);
        this.toast.success(editing ? 'Property updated successfully.' : 'Property created successfully.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.formError.set(extractErrorMessage(error, 'Unable to save this property.'));
      },
    });
  }

  protected async deleteProperty(property: Property): Promise<void> {
    const confirmed = await this.confirmService.confirm({
      title: 'Delete property',
      message: `Delete House ${property.houseNumber}, Block ${property.block}? This cannot be undone.`,
      confirmLabel: 'Delete',
      danger: true,
    });

    if (!confirmed) {
      return;
    }

    this.propertyApi.delete(property.id).subscribe({
      next: () => {
        this.toast.success('Property deleted successfully.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.toast.error(extractErrorMessage(error, 'Unable to delete this property.'));
      },
    });
  }
}
