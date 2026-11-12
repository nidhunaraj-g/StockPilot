
import {
  Component,
  OnInit,
  ChangeDetectorRef,
  inject
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { ProductService, Product } from '../../services/product';
import { CartService } from '../../services/cart';

@Component({
  selector: 'app-product-details',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink
  ],
  templateUrl: './product-details.html',
  styleUrl: './product-details.scss'
})
export class ProductDetails implements OnInit {

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private productService = inject(ProductService);
  private cdr = inject(ChangeDetectorRef);

  cartService = inject(CartService);

  product: Product | null = null;

  selectedImageUrl: string | null = null;

  isLoading = false;
  errorMessage = '';
  successMessage = '';

  ngOnInit(): void {
    this.loadProduct();
  }

  get cartCount(): number {
    return this.cartService.count();
  }

  loadProduct(): void {

    const productId = Number(
      this.route.snapshot.paramMap.get('id')
    );

    if (!productId || productId <= 0) {
      this.errorMessage = 'Invalid product selected.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.productService.getProductById(productId).subscribe({

      next: (response: Product) => {

        if (!response.isActive) {
          this.errorMessage = 'This product is currently unavailable.';
          this.product = null;
          this.isLoading = false;
          this.cdr.detectChanges();
          return;
        }

        this.product = response;

        const images = this.getProductImages(response);

        this.selectedImageUrl =
          images.length > 0 ? images[0].imageUrl : null;

        this.isLoading = false;
        this.cdr.detectChanges();

      },

      error: (error) => {

        console.error('Product details loading error:', error);

        this.errorMessage =
          error.error?.message ||
          error.error?.title ||
          'Unable to load product details. Please try again.';

        this.isLoading = false;
        this.cdr.detectChanges();

      }

    });
  }

  getProductImages(product: Product) {

    if (!product.images || product.images.length === 0) {
      return [];
    }

    return [...product.images].sort((a, b) => {

      if (a.isPrimary && !b.isPrimary) {
        return -1;
      }

      if (!a.isPrimary && b.isPrimary) {
        return 1;
      }

      return a.sortOrder - b.sortOrder;

    });
  }

  selectImage(imageUrl: string): void {
    this.selectedImageUrl = imageUrl;
  }

  isStockUnknown(product: Product): boolean {
    return product.stockQuantity === undefined ||
           product.stockQuantity === null;
  }

  isOutOfStock(product: Product): boolean {
    return product.stockQuantity === 0;
  }

  canAddProduct(product: Product): boolean {
    return this.cartService.canAddProduct(product);
  }

  addToCart(product: Product): void {

    this.successMessage = '';

    const added = this.cartService.addToCart(product);

    if (!added) {
      return;
    }

    this.successMessage = 'Product added to your cart successfully.';

    this.cdr.detectChanges();

  }

  goToCart(): void {
    this.router.navigate(['/cart']);
  }
}
