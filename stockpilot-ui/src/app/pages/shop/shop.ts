
import {
  Component,
  OnInit,
  ChangeDetectorRef,
  inject
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import {
  RouterLink,
  RouterLinkActive,
  Router
} from '@angular/router';

import {
  ProductService,
  Product,
  Category
} from '../../services/product';

import { CartService } from '../../services/cart';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-shop',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    RouterLinkActive
  ],
  templateUrl: './shop.html',
  styleUrl: './shop.scss'
})
export class Shop implements OnInit {

  private productService = inject(ProductService);
  private cdr = inject(ChangeDetectorRef);
  private authService = inject(AuthService);
  private router = inject(Router);

  cartService = inject(CartService);

  products: Product[] = [];
  categories: Category[] = [];

  isLoading = false;
  errorMessage = '';

  searchTerm = '';
  selectedCategoryId = '';

  pageNumber = 1;
  pageSize = 12;
  totalCount = 0;
  totalPages = 0;

  selectedProduct: Product | null = null;
  selectedProductImageUrl: string | null = null;

  productSelectedImages: { [productId: number]: string } = {};

  ngOnInit(): void {
    this.loadProducts();
    this.loadCategories();
  }

  get cartCount(): number {
    return this.cartService.count();
  }

  get currentCustomer() {
    return this.authService.getCurrentCustomer();
  }

  get isCustomerLoggedIn(): boolean {
    return this.currentCustomer !== null;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/shop']);
  }

  loadProducts(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.productService.getProducts(
      this.pageNumber,
      this.pageSize,
      this.searchTerm
    ).subscribe({
      next: response => {
        this.products = (response.products ?? []).filter(
          product => product.isActive
        );

        this.totalCount = response.totalCount;
        this.totalPages = response.totalPages;
        this.pageNumber = response.pageNumber;

        this.productSelectedImages = {};

        this.products.forEach(product => {
          const images = this.getProductImages(product);

          if (images.length > 0) {
            this.productSelectedImages[product.id] =
              images[0].imageUrl;
          }
        });

        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: error => {
        console.error('Shop products loading error:', error);

        this.errorMessage =
          error.error?.message ??
          'Unable to load products. Please try again.';

        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadCategories(): void {
    this.productService.getCategories().subscribe({
      next: categories => {
        this.categories = categories.filter(
          category => category.isActive
        );

        this.cdr.detectChanges();
      },
      error: error => {
        console.error('Categories loading error:', error);
      }
    });
  }

  get filteredProducts(): Product[] {
    if (!this.selectedCategoryId) {
      return this.products;
    }

    return this.products.filter(
      product =>
        product.categoryId === Number(this.selectedCategoryId)
    );
  }

  searchProducts(): void {
    this.pageNumber = 1;
    this.loadProducts();
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.pageNumber = 1;
    this.loadProducts();
  }

  onCategoryChange(): void {
    this.cdr.detectChanges();
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

  getProductImage(product: Product): string | null {
    const selectedImage = this.productSelectedImages[product.id];

    if (selectedImage) {
      return selectedImage;
    }

    const images = this.getProductImages(product);

    return images.length > 0 ? images[0].imageUrl : null;
  }

  selectProductCardImage(product: Product, imageUrl: string): void {
    this.productSelectedImages[product.id] = imageUrl;
  }

  openProductDetails(product: Product): void {
    this.selectedProduct = product;

    const images = this.getProductImages(product);

    this.selectedProductImageUrl =
      images.length > 0 ? images[0].imageUrl : null;
  }

  selectModalImage(imageUrl: string): void {
    this.selectedProductImageUrl = imageUrl;
  }

  closeProductDetails(): void {
    this.selectedProduct = null;
    this.selectedProductImageUrl = null;
  }

  isOutOfStock(product: Product): boolean {
    return product.stockQuantity === 0;
  }

  isStockUnknown(product: Product): boolean {
    return product.stockQuantity === undefined ||
           product.stockQuantity === null;
  }

  canAddProduct(product: Product): boolean {
    return this.cartService.canAddProduct(product);
  }

  addToCart(product: Product): void {
    const added = this.cartService.addToCart(product);

    if (!added) {
      return;
    }

    this.cdr.detectChanges();
  }

  previousPage(): void {
    if (this.pageNumber > 1) {
      this.pageNumber--;
      this.loadProducts();
    }
  }

  nextPage(): void {
    if (this.pageNumber < this.totalPages) {
      this.pageNumber++;
      this.loadProducts();
    }
  }
}
