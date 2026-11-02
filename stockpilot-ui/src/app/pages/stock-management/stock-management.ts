
import {
  Component,
  OnInit,
  ChangeDetectorRef,
  inject
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';

import {
  ProductService,
  Product,
  StockMovement,
  StockAdjustmentRequest
} from '../../services/product';

@Component({
  selector: 'app-stock-management',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule
  ],
  templateUrl: './stock-management.html',
  styleUrl: './stock-management.scss'
})
export class StockManagement implements OnInit {

  private productService = inject(ProductService);
  private cdr = inject(ChangeDetectorRef);

  // Product and stock movement data
  products: Product[] = [];
  movements: StockMovement[] = [];

  // Loading and saving states
  isLoading = false;
  isSaving = false;

  // Messages
  errorMessage = '';
  successMessage = '';

  // Stock adjustment form
  selectedProductId: number | null = null;
  movementType: 'IN' | 'OUT' = 'IN';
  quantity = 1;
  reason = '';

  ngOnInit(): void {
    this.loadStockData();
  }

  // Get selected product
  get selectedProduct(): Product | undefined {
    return this.products.find(
      product => product.id === this.selectedProductId
    );
  }

  // Get current stock of selected product
  get currentStock(): number {
    return this.selectedProduct?.stockQuantity ?? 0;
  }

  // Calculate total stock units
  getTotalStock(): number {
    return this.products.reduce(
      (total, product) =>
        total + (product.stockQuantity ?? 0),
      0
    );
  }

  // Calculate low stock products
  getLowStockCount(): number {
    return this.products.filter(
      product =>
        (product.stockQuantity ?? 0) <= product.minimumStockLevel
    ).length;
  }

  // Load products and stock movements
  loadStockData(): void {

    this.isLoading = true;
    this.errorMessage = '';

    this.productService.getProducts().subscribe({

      next: productResponse => {

        this.productService.getStockMovements().subscribe({

          next: movementResponse => {

            this.movements = movementResponse.movements ?? [];

            const stockMap = new Map<number, number>();

            // Calculate stock from movement history
            for (const movement of this.movements) {

              const currentStock =
                stockMap.get(movement.productId) ?? 0;

              if (movement.movementType === 'IN') {

                stockMap.set(
                  movement.productId,
                  currentStock + movement.quantity
                );

              } else if (movement.movementType === 'OUT') {

                stockMap.set(
                  movement.productId,
                  currentStock - movement.quantity
                );

              }

            }

            // Attach calculated stock to products
            this.products = (productResponse.products ?? []).map(
              product => ({
                ...product,
                stockQuantity: stockMap.get(product.id) ?? 0
              })
            );

            // Select first product by default
            if (
              this.selectedProductId === null &&
              this.products.length > 0
            ) {
              this.selectedProductId = this.products[0].id;
            }

            this.isLoading = false;
            this.cdr.detectChanges();

          },

          error: error => {

            console.error(
              'Stock movements API error:',
              error
            );

            this.errorMessage =
              'Unable to load stock movement history.';

            this.isLoading = false;
            this.cdr.detectChanges();

          }

        });

      },

      error: error => {

        console.error(
          'Products API error:',
          error
        );

        this.errorMessage =
          'Unable to load products.';

        this.isLoading = false;
        this.cdr.detectChanges();

      }

    });

  }

  // Select Stock IN or Stock OUT
  selectMovementType(type: 'IN' | 'OUT'): void {

    this.movementType = type;

    this.errorMessage = '';
    this.successMessage = '';

  }

  // Add or reduce stock
  adjustStock(): void {

    if (this.isSaving) {
      return;
    }

    // Validate product selection
    if (!this.selectedProductId) {

      this.errorMessage =
        'Please select a product.';

      return;
    }

    // Validate quantity
    if (
      !Number.isInteger(this.quantity) ||
      this.quantity <= 0
    ) {

      this.errorMessage =
        'Enter a valid quantity greater than zero.';

      return;
    }

    // Validate reason
    if (!this.reason.trim()) {

      this.errorMessage =
        'Please enter a reason for this adjustment.';

      return;
    }

    // Prevent reducing more stock than available
    if (
      this.movementType === 'OUT' &&
      this.quantity > this.currentStock
    ) {

      this.errorMessage =
        `Only ${this.currentStock} units are available.`;

      return;
    }

    const request: StockAdjustmentRequest = {

      productId: this.selectedProductId,
      quantity: this.quantity,
      movementType: this.movementType,
      reason: this.reason.trim()

    };

    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    // Call backend stock adjustment API
    this.productService.adjustStock(request).subscribe({

      next: () => {

        this.successMessage =
          this.movementType === 'IN'
            ? 'Stock added successfully!'
            : 'Stock reduced successfully!';

        this.quantity = 1;
        this.reason = '';

        this.isSaving = false;

        // Reload stock and movement history
        this.loadStockData();

      },

      error: error => {

        console.error(
          'Stock adjustment API error:',
          error
        );

        this.errorMessage =
          error.error?.message ??
          'Unable to update stock. Please try again.';

        this.isSaving = false;
        this.cdr.detectChanges();

      }

    });

  }

  // Determine product stock status
  getStockStatus(product: Product): string {

    const stock = product.stockQuantity ?? 0;

    if (stock === 0) {
      return 'Out of stock';
    }

    if (stock <= product.minimumStockLevel) {
      return 'Low stock';
    }

    return 'In stock';

  }

}
