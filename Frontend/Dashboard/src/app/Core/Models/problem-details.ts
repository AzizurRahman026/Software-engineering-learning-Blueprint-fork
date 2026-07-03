import { HttpErrorResponse } from '@angular/common/http';

/**
 * RFC 7807 problem+json body, as produced by the backend's
 * GlobalExceptionMiddleware (plus our custom `correlationId` extension).
 */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  /** Day 9/10 backend work: middleware copies X-Correlation-Id into the body. */
  correlationId?: string;
  /** ValidationProblemDetails: field name -> error messages. */
  errors?: Record<string, string[]>;
}

/**
 * Safely extract a ProblemDetails body from an HttpErrorResponse.
 * Returns null when the error body is not problem+json (network failure,
 * HTML error page from a proxy, empty body, ...).
 */
export function parseProblemDetails(err: HttpErrorResponse): ProblemDetails | null {
  const body = err.error;
  if (!body || typeof body !== 'object') return null;
  // Minimal shape check — a problem body always carries title or status.
  if (!('title' in body) && !('status' in body)) return null;
  return body as ProblemDetails;
}
