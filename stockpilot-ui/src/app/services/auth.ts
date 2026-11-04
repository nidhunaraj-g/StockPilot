import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

const API_URL = 'https://localhost:7294/api';

export interface CustomerRegisterRequest {
  firstName: string;
  lastName: string;
  userName: string;
  email: string;
  password: string;
  confirmPassword: string;
  phone: string;
  address: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
}

export interface CustomerLoginRequest {
  userName: string;
  password: string;
}

export interface CustomerLoginResponse {
  token: string;
  expiresAtUtc: string;
  customerId: number;
  userName: string;
  firstName: string;
  lastName: string;
  email: string;
}

export interface CustomerProfile {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  address: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export interface CustomerProfileUpdateRequest {
  firstName: string;
  lastName: string;
  phone: string;
  address: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);

  private readonly storageKey = 'stockpilot_customer';

  register(request: CustomerRegisterRequest): Observable<unknown> {
    return this.http.post<unknown>(
      `${API_URL}/Customers/register`,
      request
    );
  }

  login(request: CustomerLoginRequest): Observable<CustomerLoginResponse> {
    return this.http
      .post<CustomerLoginResponse>(
        `${API_URL}/Customers/login`,
        request
      )
      .pipe(
        tap(response => {
          localStorage.setItem(
            this.storageKey,
            JSON.stringify(response)
          );
        })
      );
  }

  getProfile(): Observable<CustomerProfile> {
    return this.http.get<CustomerProfile>(
      `${API_URL}/Customers/me`
    );
  }

  updateProfile(
    request: CustomerProfileUpdateRequest
  ): Observable<CustomerProfile> {
    return this.http.put<CustomerProfile>(
      `${API_URL}/Customers/me`,
      request
    );
  }

  getCurrentCustomer(): CustomerLoginResponse | null {
    const storedCustomer = localStorage.getItem(this.storageKey);

    if (!storedCustomer) {
      return null;
    }

    try {
      const customer = JSON.parse(
        storedCustomer
      ) as CustomerLoginResponse;

      if (
        new Date(customer.expiresAtUtc).getTime() <= Date.now()
      ) {
        this.logout();
        return null;
      }

      return customer;
    } catch {
      this.logout();
      return null;
    }
  }

  isLoggedIn(): boolean {
    return this.getCurrentCustomer() !== null;
  }

  logout(): void {
    localStorage.removeItem(this.storageKey);
  }

  getToken(): string | null {
    return this.getCurrentCustomer()?.token ?? null;
  }
}