import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { AuthService } from '../../../core/auth/auth.service';
import { getHomePathForRole } from '../../../core/auth/role-routes';
import { BrandMark } from '../../../shared/ui/brand-mark/brand-mark';

interface LoginForm {
  email: FormControl<string>;
  password: FormControl<string>;
  rememberMe: FormControl<boolean>;
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, BrandMark],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly isSubmitting = signal(false);
  protected readonly showPassword = signal(false);
  protected readonly serverError = signal<string | null>(null);
  protected readonly showForgotPasswordNote = signal(false);

  protected readonly form = new FormGroup<LoginForm>({
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    rememberMe: new FormControl(true, { nonNullable: true }),
  });

  protected togglePasswordVisibility(): void {
    this.showPassword.update((visible) => !visible);
  }

  protected toggleForgotPasswordNote(): void {
    this.showForgotPasswordNote.update((visible) => !visible);
  }

  protected submit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.serverError.set(null);
    this.isSubmitting.set(true);

    const { email, password, rememberMe } = this.form.getRawValue();

    this.auth.login({ email, password }, rememberMe).subscribe({
      next: (response) => {
        const homePath = getHomePathForRole(response.role);
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');

        // Only honor returnUrl when it actually belongs to this user's
        // own role branch — a returnUrl pointing at another role's area
        // (e.g. crafted from a blocked /admin visit) must never be used
        // to bounce a resident into an admin route post-login.
        const target = returnUrl && returnUrl.startsWith(homePath) ? returnUrl : homePath;

        this.router.navigateByUrl(target);
      },
      error: (error: HttpErrorResponse) => {
        this.isSubmitting.set(false);

        if (error.status === 401) {
          this.serverError.set('The email or password you entered is incorrect.');
        } else if (error.status === 0) {
          this.serverError.set(
            'Unable to reach the server. Please check your connection and try again.',
          );
        } else {
          this.serverError.set('Something went wrong while signing in. Please try again.');
        }
      },
    });
  }
}
