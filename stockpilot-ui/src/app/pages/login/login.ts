
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  private authService = inject(AuthService);
  private router = inject(Router);

  userName = '';
  password = '';

  showPassword = false;
  isLoading = false;
  errorMessage = '';

  login(): void {
    this.errorMessage = '';

    if (!this.userName.trim() || !this.password) {
      this.errorMessage = 'Please enter your username and password.';
      return;
    }

    this.isLoading = true;

    this.authService.login({
      userName: this.userName.trim(),
      password: this.password
    }).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigate(['/shop']);
      },
      error: error => {
        this.isLoading = false;
        this.errorMessage =
          error.error?.message ?? 'Login failed. Please check your details.';
      }
    });
  }
}
