
import {
  Component,
  OnInit,
  ChangeDetectorRef,
  inject
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';

import {
  ProductService,
  Order
} from '../../services/product';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './orders.html',
  styleUrl: './orders.scss'
})
export class Orders implements OnInit {
  private productService = inject(ProductService);
  private cdr = inject(ChangeDetectorRef);

  orders: Order[] = [];

  loading = false;
  cancelling = false;

  search = '';
  selectedStatus = '';

  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  selectedOrder: Order | null = null;
  orderToCancel: Order | null = null;

  successMessage = '';
  errorMessage = '';

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading = true;
    this.errorMessage = '';

    this.productService.getOrders(
      this.currentPage,
      this.pageSize,
      this.search,
      this.selectedStatus
    )
    .pipe(
      finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
      })
    )
    .subscribe({
      next: (response) => {
        this.orders = response.orders ?? [];
        this.totalCount = response.totalCount;
        this.currentPage = response.pageNumber;
        this.pageSize = response.pageSize;
        this.totalPages = response.totalPages;
      },
      error: (error) => {
        console.error('Orders loading error:', error);
        this.errorMessage = 'Unable to load orders. Please check whether the API is running.';
      }
    });
  }

  searchOrders(): void {
    this.currentPage = 1;
    this.loadOrders();
  }

  filterByStatus(): void {
    this.currentPage = 1;
    this.loadOrders();
  }

  clearFilters(): void {
    this.search = '';
    this.selectedStatus = '';
    this.currentPage = 1;
    this.loadOrders();
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadOrders();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadOrders();
    }
  }

  viewOrder(order: Order): void {
    this.selectedOrder = order;
  }

  closeDetails(): void {
    this.selectedOrder = null;
  }

  confirmCancel(order: Order): void {
    this.orderToCancel = order;
  }

  closeCancelConfirmation(): void {
    if (!this.cancelling) {
      this.orderToCancel = null;
    }
  }

  cancelOrder(): void {
    if (!this.orderToCancel || this.cancelling) {
      return;
    }

    const orderId = this.orderToCancel.id;

    this.cancelling = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.productService.cancelOrder(orderId)
      .pipe(
        finalize(() => {
          this.cancelling = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (response) => {
          this.successMessage = response.message || 'Order cancelled successfully.';
          this.orderToCancel = null;
          this.selectedOrder = null;
          this.loadOrders();
        },
        error: (error) => {
          console.error('Order cancellation error:', error);

          this.errorMessage =
            error.error?.message || 'Unable to cancel this order. Please try again.';

          this.orderToCancel = null;
        }
      });
  }

  get confirmedCount(): number {
    return this.orders.filter(
      order => order.status.toLowerCase() === 'confirmed'
    ).length;
  }

  get cancelledCount(): number {
    return this.orders.filter(
      order => order.status.toLowerCase() === 'cancelled'
    ).length;
  }

  get totalRevenue(): number {
    return this.orders
      .filter(order => order.status.toLowerCase() !== 'cancelled')
      .reduce((total, order) => total + order.totalAmount, 0);
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'confirmed':
        return 'confirmed';
      case 'cancelled':
        return 'cancelled';
      case 'pending':
        return 'pending';
      case 'completed':
        return 'completed';
      default:
        return 'default';
    }
  }
}
