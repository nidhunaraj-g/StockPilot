
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth';

export const authInterceptor: HttpInterceptorFn = (req, next) => {

  const authService = inject(AuthService);

  const token = authService.getToken();

  // Do not attach a token to public authentication endpoints.
  const isPublicAuthRequest =
    req.url.includes('/Customers/login') ||
    req.url.includes('/Customers/register');

  if (!token || isPublicAuthRequest) {
    return next(req);
  }

  // Attach JWT token to the request.
  const authReq = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });

  return next(authReq);
};
