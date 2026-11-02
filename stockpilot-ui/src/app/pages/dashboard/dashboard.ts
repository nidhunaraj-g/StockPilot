import {
  Component,
  OnInit,
  ChangeDetectorRef,
  inject
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { forkJoin } from 'rxjs';

import {
  ProductService,
  Product,
  StockMovement,
  StockSummary
} from '../../services/product';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit {

  private productService = inject(ProductService);
  private cdr = inject(ChangeDetectorRef);

  products: Product[] = [];
  movements: StockMovement[] = [];
  summary: StockSummary | null = null;

  isLoading = false;
  errorMessage = '';

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.isLoading = true;
    this.errorMessage = '';

    forkJoin({
      productResponse: this.productService.getProducts(),
      stockSummary: this.productService.getStockSummary(),
      movementResponse: this.productService.getStockMovements()
    }).subscribe({
      next: result => {
        this.summary = result.stockSummary;
        this.movements = result.movementResponse.movements ?? [];

        const stockMap = new Map<number, number>();

        for (const movement of this.movements) {
          const currentStock = stockMap.get(movement.productId) ?? 0;

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

        this.products = (result.productResponse.products ?? []).map(
          product => ({
            ...product,
            stockQuantity: stockMap.get(product.id) ?? 0
          })
        );

        this.isLoading = false;
        this.cdr.detectChanges();
      },

      error: error => {
        console.error('Dashboard loading error:', error);

        this.errorMessage =
          'Unable to load dashboard data. Please check whether the API is running.';

        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  getInventoryValue(): number {
    return this.products.reduce(
      (total, product) =>
        total +
        product.costPrice * (product.stockQuantity ?? 0),
      0
    );
  }

  getStockStatus(product: Product): string {
    const quantity = product.stockQuantity ?? 0;

    if (quantity === 0) {
      return 'Out of stock';
    }

    if (quantity <= product.minimumStockLevel) {
      return 'Low stock';
    }

    return 'In stock';
  }

  getMovementIcon(type: 'IN' | 'OUT'): string {
    return type === 'IN' ? 'south_west' : 'north_east';
  }

}