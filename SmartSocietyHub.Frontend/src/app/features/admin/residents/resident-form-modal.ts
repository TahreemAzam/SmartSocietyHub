import { HttpErrorResponse } from '@angular/common/http';
import { Component, EventEmitter, Input, OnInit, Output, computed, inject, signal } from '@angular/core';
import {
  FormArray,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';

import { ResidentApi } from '../../../core/api/resident.api';
import { Property } from '../../../core/models/property.models';
import {
  CreateResidentRequest,
  CreateResidentResponse,
  Resident,
} from '../../../core/models/resident.models';
import { Modal } from '../../../shared/ui/modal/modal';
import { extractErrorMessage } from '../../../shared/utils/api-error';
import { formatCnic } from '../../../shared/utils/cnic';

interface FamilyMemberForm {
  fullName: FormControl<string>;
  cnic: FormControl<string>;
  phoneNumber: FormControl<string>;
  email: FormControl<string>;
  dateOfBirth: FormControl<string>;
  gender: FormControl<string>;
  relationship: FormControl<string>;
}

interface ResidentForm {
  fullName: FormControl<string>;
  cnic: FormControl<string>;
  phoneNumber: FormControl<string>;
  email: FormControl<string>;
  dateOfBirth: FormControl<string>;
  gender: FormControl<string>;
  propertyId: FormControl<string>;
  status: FormControl<string>;
  familyMembers: FormArray<FormGroup<FamilyMemberForm>>;
}

export type ResidentSaveResult =
  | { mode: 'create'; response: CreateResidentResponse }
  | { mode: 'update'; resident: Resident };

function buildFamilyMemberGroup(): FormGroup<FamilyMemberForm> {
  return new FormGroup<FamilyMemberForm>({
    fullName: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    cnic: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/^\d{5}-\d{7}-\d$/)],
    }),
    phoneNumber: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    email: new FormControl('', { nonNullable: true, validators: [Validators.email] }),
    dateOfBirth: new FormControl('', { nonNullable: true }),
    gender: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    relationship: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });
}

@Component({
  selector: 'app-resident-form-modal',
  standalone: true,
  imports: [ReactiveFormsModule, Modal],
  templateUrl: './resident-form-modal.html',
  styleUrl: './resident-form-modal.scss',
})
export class ResidentFormModal implements OnInit {
  private readonly residentApi = inject(ResidentApi);

  @Input() residentToEdit: Resident | null = null;
  @Input({ required: true }) properties: Property[] = [];
  @Output() saved = new EventEmitter<ResidentSaveResult>();
  @Output() closedModal = new EventEmitter<void>();

  protected readonly saving = signal(false);
  protected readonly formError = signal<string | null>(null);

  protected readonly availableProperties = computed(() => {
    const currentPropertyId = this.residentToEdit?.propertyId;

    return this.properties.filter(
      (p) => p.status === 'Vacant' || p.id === currentPropertyId,
    );
  });

  protected readonly form = new FormGroup<ResidentForm>({
    fullName: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    cnic: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/^\d{5}-\d{7}-\d$/)],
    }),
    phoneNumber: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    email: new FormControl('', { nonNullable: true, validators: [Validators.email] }),
    dateOfBirth: new FormControl('', { nonNullable: true }),
    gender: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    propertyId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    status: new FormControl('Active', { nonNullable: true, validators: [Validators.required] }),
    familyMembers: new FormArray<FormGroup<FamilyMemberForm>>([]),
  });

  ngOnInit(): void {
    const resident = this.residentToEdit;

    if (!resident) {
      return;
    }

    this.form.patchValue({
      fullName: resident.fullName,
      cnic: resident.cnic,
      phoneNumber: resident.phoneNumber,
      email: resident.email ?? '',
      dateOfBirth: resident.dateOfBirth?.slice(0, 10) ?? '',
      gender: resident.gender,
      propertyId: resident.propertyId,
      status: resident.status,
    });

    for (const member of resident.familyMembers) {
      const group = buildFamilyMemberGroup();
      group.patchValue({
        fullName: member.fullName,
        cnic: member.cnic,
        phoneNumber: member.phoneNumber,
        email: member.email ?? '',
        dateOfBirth: member.dateOfBirth?.slice(0, 10) ?? '',
        gender: member.gender,
        relationship: member.relationship,
      });
      this.form.controls.familyMembers.push(group);
    }
  }

  protected addFamilyMember(): void {
    this.form.controls.familyMembers.push(buildFamilyMemberGroup());
  }

  protected removeFamilyMember(index: number): void {
    this.form.controls.familyMembers.removeAt(index);
  }

  protected onCnicInput(event: Event, control: FormControl<string>): void {
    const input = event.target as HTMLInputElement;
    control.setValue(formatCnic(input.value));
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.form.controls.familyMembers.controls.forEach((group) => group.markAllAsTouched());
      return;
    }

    this.formError.set(null);
    this.saving.set(true);

    const raw = this.form.getRawValue();

    const request: CreateResidentRequest = {
      fullName: raw.fullName,
      cnic: raw.cnic,
      phoneNumber: raw.phoneNumber,
      email: raw.email || null,
      dateOfBirth: raw.dateOfBirth || null,
      gender: raw.gender,
      propertyId: raw.propertyId,
      status: raw.status,
      numberOfFamilyMembers: raw.familyMembers.length,
      familyMembers: raw.familyMembers.map((member) => ({
        fullName: member.fullName,
        cnic: member.cnic,
        phoneNumber: member.phoneNumber,
        email: member.email || null,
        dateOfBirth: member.dateOfBirth || null,
        gender: member.gender,
        relationship: member.relationship,
      })),
    };

    const resident = this.residentToEdit;

    if (resident) {
      this.residentApi.update(resident.id, request).subscribe({
        next: (updated) => {
          this.saving.set(false);
          this.saved.emit({ mode: 'update', resident: updated });
        },
        error: (error: HttpErrorResponse) => {
          this.saving.set(false);
          this.formError.set(extractErrorMessage(error, 'Unable to update this resident.'));
        },
      });
    } else {
      this.residentApi.create(request).subscribe({
        next: (response) => {
          this.saving.set(false);
          this.saved.emit({ mode: 'create', response });
        },
        error: (error: HttpErrorResponse) => {
          this.saving.set(false);
          this.formError.set(extractErrorMessage(error, 'Unable to create this resident.'));
        },
      });
    }
  }
}
