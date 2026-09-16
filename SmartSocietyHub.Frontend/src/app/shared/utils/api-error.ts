import { HttpErrorResponse } from '@angular/common/http';

// Every controller in the backend consistently returns { message: "..." }
// on 4xx/409 responses (see e.g. PropertyController, ComplaintController).
// This reads that shape, falling back to a sensible generic message for
// anything else (network failure, unexpected 500, validation payloads).
export function extractErrorMessage(error: unknown, fallback = 'Something went wrong. Please try again.'): string {
  if (error instanceof HttpErrorResponse) {
    if (error.status === 0) {
      return 'Unable to reach the server. Please check your connection.';
    }

    const body = error.error;

    if (body && typeof body === 'object' && typeof body.message === 'string') {
      return body.message;
    }

    // ASP.NET model-validation errors come back as { errors: { Field: [msgs] } }.
    if (body && typeof body === 'object' && body.errors && typeof body.errors === 'object') {
      const firstError = Object.values(body.errors as Record<string, string[]>)[0];
      if (Array.isArray(firstError) && firstError.length > 0) {
        return firstError[0];
      }
    }
  }

  return fallback;
}
