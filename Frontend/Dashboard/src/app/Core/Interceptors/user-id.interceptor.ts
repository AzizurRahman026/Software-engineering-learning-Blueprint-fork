import { HttpInterceptorFn } from '@angular/common/http';

const STORAGE_KEY = 'auth_user';

/**
 * Attaches the logged-in user's id as an `X-User-Id` header on every outgoing
 * request, read from the `auth_user` entry persisted by AuthService. This is how
 * the backend scopes chat history to the current user.
 */
export const userIdInterceptor: HttpInterceptorFn = (req, next) => {
  if (typeof localStorage === 'undefined') {
    return next(req);
  }

  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) {
    return next(req);
  }

  try {
    const user = JSON.parse(raw) as { userId?: string };
    if (user?.userId) {
      return next(req.clone({ setHeaders: { 'X-User-Id': user.userId } }));
    }
  } catch {
    // ignore malformed storage — fall through without the header
  }

  return next(req);
};
