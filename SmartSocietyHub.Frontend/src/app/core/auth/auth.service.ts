import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AuthenticatedUser, LoginRequest, LoginResponse } from '../models/auth.models';
import { isJwtExpired } from './jwt.util';

const STORAGE_KEY = 'hsh.auth';

interface StoredSession {
  token: string;
  user: AuthenticatedUser;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly currentUserSignal = signal<AuthenticatedUser | null>(null);
  private readonly tokenSignal = signal<string | null>(null);

  readonly currentUser = this.currentUserSignal.asReadonly();
  readonly isAuthenticated = computed(() => this.currentUserSignal() !== null);

  constructor(
    private readonly http: HttpClient,
    private readonly router: Router,
  ) {
    this.restoreSession();
  }

  login(request: LoginRequest, rememberMe: boolean): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiBaseUrl}/Auth/login`, request).pipe(
      tap((response) => {
        const user: AuthenticatedUser = {
          userId: response.userId,
          fullName: response.fullName,
          email: response.email,
          role: response.role,
        };

        this.persistSession({ token: response.token, user }, rememberMe);
        this.tokenSignal.set(response.token);
        this.currentUserSignal.set(user);
      }),
    );
  }

  logout(navigateToLogin = true): void {
    localStorage.removeItem(STORAGE_KEY);
    sessionStorage.removeItem(STORAGE_KEY);
    this.tokenSignal.set(null);
    this.currentUserSignal.set(null);

    if (navigateToLogin) {
      this.router.navigate(['/login']);
    }
  }

  getToken(): string | null {
    return this.tokenSignal();
  }

  private restoreSession(): void {
    const raw = localStorage.getItem(STORAGE_KEY) ?? sessionStorage.getItem(STORAGE_KEY);

    if (!raw) {
      return;
    }

    try {
      const session = JSON.parse(raw) as StoredSession;

      if (!session.token || isJwtExpired(session.token)) {
        localStorage.removeItem(STORAGE_KEY);
        sessionStorage.removeItem(STORAGE_KEY);
        return;
      }

      this.tokenSignal.set(session.token);
      this.currentUserSignal.set(session.user);
    } catch {
      localStorage.removeItem(STORAGE_KEY);
      sessionStorage.removeItem(STORAGE_KEY);
    }
  }

  private persistSession(session: StoredSession, rememberMe: boolean): void {
    // Remember me controls whether the session survives closing the
    // browser (localStorage) or ends with the tab/browser session
    // (sessionStorage). The token's own 60-minute expiry applies either
    // way — the backend issues no refresh token.
    const serialized = JSON.stringify(session);

    if (rememberMe) {
      localStorage.setItem(STORAGE_KEY, serialized);
      sessionStorage.removeItem(STORAGE_KEY);
    } else {
      sessionStorage.setItem(STORAGE_KEY, serialized);
      localStorage.removeItem(STORAGE_KEY);
    }
  }
}
