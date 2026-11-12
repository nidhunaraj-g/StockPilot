
import { Component, inject } from '@angular/core';
import {
  Router,
  RouterOutlet,
  RouterLink,
  RouterLinkActive,
  NavigationEnd
} from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { filter } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatIconModule
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private router = inject(Router);

  isShopRoute = this.isCustomerRoute(this.router.url);

  constructor() {
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(event => {
        if (event instanceof NavigationEnd) {
          this.isShopRoute = this.isCustomerRoute(
            event.urlAfterRedirects
          );
        }
      });
  }

  private isCustomerRoute(url: string): boolean {
    return (
      url === '/shop' ||
      url.startsWith('/shop/') ||

      url === '/product-details' ||
      url.startsWith('/product-details/') ||

      url === '/cart' ||
      url.startsWith('/cart/') ||

      url === '/login' ||
      url.startsWith('/login/') ||

      url === '/register' ||
      url.startsWith('/register/') ||

      url === '/profile' ||
      url.startsWith('/profile/') ||

      url === '/my-orders' ||
      url.startsWith('/my-orders/')
    );
  }
}
