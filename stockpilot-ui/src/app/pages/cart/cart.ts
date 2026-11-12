
import {
  Component,
  ChangeDetectorRef,
  inject
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import {
  ProductService,
  CreateOrderRequest,
  Order
} from '../../services/product';

import { CartService } from '../../services/cart';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink
  ],
  templateUrl: './cart.html',
  styleUrl: './cart.scss'
})
export class Cart {

  private productService = inject(ProductService);
  private cdr = inject(ChangeDetectorRef);

  cartService = inject(CartService);

  customerName = '';
  customerEmail = '';
  customerPhone = '';

  deliveryAddress = '';
  deliveryCity = '';
  deliveryState = '';
  deliveryCountry = 'India';
  deliveryPostalCode = '';

  isSubmittingOrder = false;
  checkoutError = '';
  checkoutSuccess = '';

  placedOrder: Order | null = null;

  placeOrder(): void {
    this.checkoutError = '';
    this.checkoutSuccess = '';

    const cartItems = this.cartService.items();

    if (cartItems.length === 0) {
      this.checkoutError = 'Your cart is empty.';
      return;
    }

    if (!this.customerName.trim()) {
      this.checkoutError = 'Please enter your name.';
      return;
    }

    if (!this.customerEmail.trim()) {
      this.checkoutError = 'Please enter your email address.';
      return;
    }

    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    if (!emailPattern.test(this.customerEmail.trim())) {
      this.checkoutError = 'Please enter a valid email address.';
      return;
    }

    if (this.customerName.trim().length > 150) {
      this.checkoutError = 'Name cannot exceed 150 characters.';
      return;
    }

    if (this.customerEmail.trim().length > 254) {
      this.checkoutError = 'Email cannot exceed 254 characters.';
      return;
    }

    if (this.customerPhone.trim().length > 20) {
      this.checkoutError = 'Phone number cannot exceed 20 characters.';
      return;
    }

    if (!this.deliveryAddress.trim()) {
      this.checkoutError = 'Please enter your delivery address.';
      return;
    }

    if (!this.deliveryCity.trim()) {
      this.checkoutError = 'Please enter your delivery city.';
      return;
    }

    if (!this.deliveryState.trim()) {
      this.checkoutError = 'Please enter your delivery state.';
      return;
    }

    if (!this.deliveryCountry.trim()) {
      this.checkoutError = 'Please enter your delivery country.';
      return;
    }

    if (!this.deliveryPostalCode.trim()) {
      this.checkoutError = 'Please enter your postal code.';
      return;
    }

    if (this.deliveryAddress.trim().length > 300) {
      this.checkoutError = 'Delivery address cannot exceed 300 characters.';
      return;
    }

    if (
      this.deliveryCity.trim().length > 100 ||
      this.deliveryState.trim().length > 100 ||
      this.deliveryCountry.trim().length > 100
    ) {
      this.checkoutError =
        'City, state and country cannot exceed 100 characters.';
      return;
    }

    if (this.deliveryPostalCode.trim().length > 20) {
      this.checkoutError = 'Postal code cannot exceed 20 characters.';
      return;
    }

    for (const item of cartItems) {
      const stock = item.product.stockQuantity;

      if (stock === undefined || stock === null) {
        this.checkoutError =
          `Stock information is unavailable for ${item.product.name}. Please refresh the shop.`;
        return;
      }

      if (stock <= 0 || item.quantity > stock) {
        this.checkoutError =
          `Insufficient stock for ${item.product.name}. Please update your cart.`;
        return;
      }
    }

    const orderRequest: CreateOrderRequest = {
      customerName: this.customerName.trim(),
      customerEmail: this.customerEmail.trim(),
      customerPhone: this.customerPhone.trim() || null,

      deliveryAddress: this.deliveryAddress.trim(),
      deliveryCity: this.deliveryCity.trim(),
      deliveryState: this.deliveryState.trim(),
      deliveryCountry: this.deliveryCountry.trim(),
      deliveryPostalCode: this.deliveryPostalCode.trim(),

      items: cartItems.map(item => ({
        productId: item.product.id,
        quantity: item.quantity
      }))
    };

    this.isSubmittingOrder = true;

    this.productService.createOrder(orderRequest).subscribe({
      next: order => {
        this.placedOrder = order;

        this.checkoutSuccess =
          `Your order has been placed successfully. Order number: ${order.orderNumber}`;

        this.cartService.clearCart();

        this.customerName = '';
        this.customerEmail = '';
        this.customerPhone = '';

        this.deliveryAddress = '';
        this.deliveryCity = '';
        this.deliveryState = '';
        this.deliveryCountry = 'India';
        this.deliveryPostalCode = '';

        this.isSubmittingOrder = false;
        this.cdr.detectChanges();
      },

      error: error => {
        console.error('Order creation error:', error);

        this.checkoutError =
          error.error?.message ??
          error.error?.title ??
          'Unable to place your order. Please try again.';

        if (error.status === 400) {
          this.checkoutError =
            error.error?.message ??
            'Please check your order details and try again.';
        } else if (error.status === 404) {
          this.checkoutError =
            'One or more products could not be found. Please refresh the products.';
        } else if (error.status === 409) {
          this.checkoutError =
            error.error?.message ??
            'The requested stock is no longer available. Please update your cart.';
        } else if (error.status === 0) {
          this.checkoutError =
            'Cannot connect to the server. Please check whether the API is running.';
        }

        this.isSubmittingOrder = false;
        this.cdr.detectChanges();
      }
    });
  }

  increaseQuantity(productId: number): void {
    const increased = this.cartService.increaseQuantity(productId);

    if (!increased) {
      const item = this.cartService.items().find(
        cartItem => cartItem.product.id === productId
      );

      if (item?.product.stockQuantity !== undefined) {
        this.checkoutError =
          `Only ${item.product.stockQuantity} units of ${item.product.name} are available.`;
      } else {
        this.checkoutError =
          'Stock information is unavailable. Please refresh the shop.';
      }
    } else {
      this.checkoutError = '';
    }
  }

  decreaseQuantity(productId: number): void {
    this.cartService.decreaseQuantity(productId);
    this.checkoutError = '';
  }

  removeFromCart(productId: number): void {
    this.cartService.removeFromCart(productId);
    this.checkoutError = '';
  }

  get cartCount(): number {
    return this.cartService.count();
  }

  get cartTotal(): number {
    return this.cartService.total();
  }
}
