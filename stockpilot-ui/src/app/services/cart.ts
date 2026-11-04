
import { Injectable, computed, signal } from '@angular/core';
import { Product } from './product';

export interface CartItem {
  product: Product;
  quantity: number;
}

@Injectable({
  providedIn: 'root'
})
export class CartService {

  private readonly storageKey = 'stockpilot_cart';

  // Restore cart items when the service is initialized.
  private cartItemsSignal = signal<CartItem[]>(this.loadCart());

  readonly items = this.cartItemsSignal.asReadonly();

  readonly count = computed(() =>
    this.cartItemsSignal().reduce(
      (total, item) => total + item.quantity,
      0
    )
  );

  readonly total = computed(() =>
    this.cartItemsSignal().reduce(
      (total, item) =>
        total + item.product.sellingPrice * item.quantity,
      0
    )
  );

  // Load previously saved cart items from browser storage.
  private loadCart(): CartItem[] {
    try {
      const savedCart = localStorage.getItem(this.storageKey);

      if (!savedCart) {
        return [];
      }

      const parsedCart: unknown = JSON.parse(savedCart);

      if (!Array.isArray(parsedCart)) {
        return [];
      }

      return parsedCart.filter((item: any) =>
        item &&
        item.product &&
        typeof item.product.id === 'number' &&
        typeof item.quantity === 'number' &&
        Number.isInteger(item.quantity) &&
        item.quantity > 0
      ) as CartItem[];

    } catch (error) {
      console.error('Unable to restore cart:', error);
      return [];
    }
  }

  // Save the current cart to browser storage.
  private saveCart(): void {
    try {
      localStorage.setItem(
        this.storageKey,
        JSON.stringify(this.cartItemsSignal())
      );
    } catch (error) {
      console.error('Unable to save cart:', error);
    }
  }

  addToCart(product: Product): boolean {

    // Do not allow products whose stock is unknown or zero.
    if (
      product.stockQuantity === undefined ||
      product.stockQuantity <= 0
    ) {
      return false;
    }

    const currentItems = this.cartItemsSignal();

    const existingItem = currentItems.find(
      item => item.product.id === product.id
    );

    const currentQuantity = existingItem?.quantity ?? 0;

    if (currentQuantity >= product.stockQuantity) {
      return false;
    }

    if (existingItem) {
      this.cartItemsSignal.update(items =>
        items.map(item =>
          item.product.id === product.id
            ? {
                ...item,
                product,
                quantity: item.quantity + 1
              }
            : item
        )
      );
    } else {
      this.cartItemsSignal.update(items => [
        ...items,
        {
          product,
          quantity: 1
        }
      ]);
    }

    this.saveCart();

    return true;
  }

  removeFromCart(productId: number): void {
    this.cartItemsSignal.update(items =>
      items.filter(item => item.product.id !== productId)
    );

    this.saveCart();
  }

  increaseQuantity(productId: number): boolean {
    const item = this.cartItemsSignal().find(
      cartItem => cartItem.product.id === productId
    );

    if (!item) {
      return false;
    }

    if (
      item.product.stockQuantity === undefined ||
      item.product.stockQuantity <= 0 ||
      item.quantity >= item.product.stockQuantity
    ) {
      return false;
    }

    this.cartItemsSignal.update(items =>
      items.map(cartItem =>
        cartItem.product.id === productId
          ? {
              ...cartItem,
              quantity: cartItem.quantity + 1
            }
          : cartItem
      )
    );

    this.saveCart();

    return true;
  }

  decreaseQuantity(productId: number): void {
    const item = this.cartItemsSignal().find(
      cartItem => cartItem.product.id === productId
    );

    if (!item) {
      return;
    }

    if (item.quantity <= 1) {
      this.removeFromCart(productId);
      return;
    }

    this.cartItemsSignal.update(items =>
      items.map(cartItem =>
        cartItem.product.id === productId
          ? {
              ...cartItem,
              quantity: cartItem.quantity - 1
            }
          : cartItem
      )
    );

    this.saveCart();
  }

  clearCart(): void {
    this.cartItemsSignal.set([]);

    this.saveCart();
  }

  getQuantity(productId: number): number {
    return this.cartItemsSignal().find(
      item => item.product.id === productId
    )?.quantity ?? 0;
  }

  canAddProduct(product: Product): boolean {
    if (
      product.stockQuantity === undefined ||
      product.stockQuantity <= 0
    ) {
      return false;
    }

    return this.getQuantity(product.id) < product.stockQuantity;
  }
}
