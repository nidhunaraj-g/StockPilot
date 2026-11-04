
import {
  Component,
  OnInit,
  ChangeDetectorRef,
  inject
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import {
  AuthService,
  CustomerProfile,
  CustomerProfileUpdateRequest
} from '../../services/auth';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink
  ],
  templateUrl: './profile.html',
  styleUrl: './profile.scss'
})
export class Profile implements OnInit {

  private authService = inject(AuthService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  profile: CustomerProfile | null = null;

  form: CustomerProfileUpdateRequest = {
    firstName: '',
    lastName: '',
    phone: '',
    address: '',
    city: '',
    state: '',
    country: '',
    postalCode: ''
  };

  isLoading = false;
  isSaving = false;

  errorMessage = '';
  successMessage = '';

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {

    console.log('Loading customer profile...');

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.getProfile()
      .pipe(
        finalize(() => {
          this.isLoading = false;
          this.cdr.detectChanges();

          console.log('Profile loading completed');
        })
      )
      .subscribe({

        next: (response: CustomerProfile) => {

          console.log('Profile received:', response);

          this.profile = response;

          this.form = {
            firstName: response.firstName,
            lastName: response.lastName,
            phone: response.phone,
            address: response.address,
            city: response.city,
            state: response.state,
            country: response.country,
            postalCode: response.postalCode
          };

        },

        error: (error) => {

          console.error('Profile loading error:', error);

          if (error.status === 401) {
            this.authService.logout();
            this.router.navigate(['/login']);
            return;
          }

          this.errorMessage =
            error.error?.message ||
            error.error?.title ||
            'Unable to load your profile. Please try again.';

        }

      });
  }

  updateProfile(): void {

    this.errorMessage = '';
    this.successMessage = '';

    this.isSaving = true;

    console.log('Updating profile:', this.form);

    this.authService.updateProfile(this.form).subscribe({

      next: () => {

        console.log('Profile updated successfully');

        this.isSaving = false;

        this.successMessage =
          'Your profile has been updated successfully.';

        // Fetch the latest profile data from the database.
        this.loadProfile();

      },

      error: (error) => {

        console.error('Profile update error:', error);

        this.isSaving = false;

        this.errorMessage =
          error.error?.message ||
          error.error?.title ||
          'Unable to update your profile. Please try again.';

        this.cdr.detectChanges();

      }

    });
  }
}
