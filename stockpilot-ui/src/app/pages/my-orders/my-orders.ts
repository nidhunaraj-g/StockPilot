
import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductService, Order, OrderPage } from '../../services/product';

@Component({
  selector: 'app-my-orders',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-orders.html',
  styleUrl: './my-orders.scss'
})
export class MyOrders implements OnInit {

  private productService = inject(ProductService);

  orders: Order[] = [];

  isLoading = false;
  errorMessage = '';

  pageNumber = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.productService
      .getMyOrders(this.pageNumber, this.pageSize)
      .subscribe({
        next: (response: OrderPage) => {
          this.orders = response.orders;
          this.totalCount = response.totalCount;
          this.pageNumber = response.pageNumber;
          this.pageSize = response.pageSize;
          this.totalPages = response.totalPages;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading customer orders:', error);

          this.errorMessage =
            error.status === 401
              ? 'Please login to view your orders.'
              : 'Unable to load your orders. Please try again.';

          this.isLoading = false;
        }
      });
  }

  previousPage(): void {
    if (this.pageNumber > 1) {
      this.pageNumber--;
      this.loadOrders();
    }
  }

  nextPage(): void {
    if (this.pageNumber < this.totalPages) {
      this.pageNumber++;
      this.loadOrders();
    }
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'confirmed':
        return 'confirmed';

      case 'pending':
        return 'pending';

      case 'shipped':
        return 'shipped';

      case 'delivered':
        return 'delivered';

      case 'cancelled':
        return 'cancelled';

      default:
        return 'default';
    }
  }

}
