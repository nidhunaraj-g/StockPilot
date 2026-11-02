
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

const API_URL = 'https://localhost:7294/api';

// Product models
export interface CreateProductRequest {
  sku: string;
  barcode: string | null;
  name: string;
  description: string | null;
  categoryId: number;
  brandId: number | null;
  costPrice: number;
  sellingPrice: number;
  minimumStockLevel: number;
}

export interface UpdateProductRequest extends CreateProductRequest {}

export interface Product {
  id: number;
  sku: string;
  barcode: string | null;
  name: string;
  description: string | null;
  categoryId: number;
  categoryName: string;
  brandId: number | null;
  brandName: string | null;
  costPrice: number;
  sellingPrice: number;
  minimumStockLevel: number;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string | null;
  stockQuantity?: number;
}

// Category models
export interface Category {
  id: number;
  name: string;
  description: string;
  isActive: boolean;
  createdAtUtc?: string;
  updatedAtUtc?: string | null;
}

export interface CreateCategoryRequest {
  name: string;
  description: string;
}

export interface UpdateCategoryRequest {
  name: string;
  description: string;
}

// Brand models
export interface Brand {
  id: number;
  name: string;
  description: string;
  logoUrl: string | null;
  isActive: boolean;
}

export interface CreateBrandRequest {
  name: string;
  description: string;
  logoUrl: string | null;
}

export interface UpdateBrandRequest {
  name: string;
  description: string;
  logoUrl: string | null;
}

export interface ProductPage {
  products: Product[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// Stock models
export interface StockAdjustmentRequest {
  productId: number;
  quantity: number;
  movementType: 'IN' | 'OUT';
  reason: string;
}

export interface StockSummary {
  totalActiveProducts: number;
  totalStockUnits: number;
  lowStockProducts: number;
  outOfStockProducts: number;
}

export interface StockMovement {
  id: number;
  productId: number;
  productName: string;
  quantity: number;
  movementType: 'IN' | 'OUT';
  reason: string;
  createdAtUtc: string;
}

export interface StockMovementPage {
  movements: StockMovement[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// Order models
export interface CreateOrderItemRequest {
  productId: number;
  quantity: number;
}

export interface CreateOrderRequest {
  customerName: string;
  customerEmail: string;
  customerPhone: string | null;
  items: CreateOrderItemRequest[];
}

export interface OrderItem {
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}

export interface Order {
  id: number;
  orderNumber: string;
  customerName: string;
  customerEmail: string;
  customerPhone: string | null;
  orderDateUtc: string;
  status: string;
  subtotal: number;
  taxAmount: number;
  totalAmount: number;
  items: OrderItem[];
}

export interface OrderPage {
  orders: Order[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface CancelOrderResponse {
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private http = inject(HttpClient);

  // Products
  getProducts(
    pageNumber = 1,
    pageSize = 10,
    search = ''
  ): Observable<ProductPage> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    if (search.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http.get<ProductPage>(
      `${API_URL}/Products`,
      { params }
    );
  }

  getProductById(id: number): Observable<Product> {
    return this.http.get<Product>(`${API_URL}/Products/${id}`);
  }

  createProduct(product: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(`${API_URL}/Products`, product);
  }

  updateProduct(
    id: number,
    product: UpdateProductRequest
  ): Observable<Product> {
    return this.http.put<Product>(`${API_URL}/Products/${id}`, product);
  }

  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${API_URL}/Products/${id}`);
  }

  // Categories
  getCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(`${API_URL}/Categories`);
  }

  getCategoryById(id: number): Observable<Category> {
    return this.http.get<Category>(`${API_URL}/Categories/${id}`);
  }

  createCategory(category: CreateCategoryRequest): Observable<Category> {
    return this.http.post<Category>(`${API_URL}/Categories`, category);
  }

  updateCategory(
    id: number,
    category: UpdateCategoryRequest
  ): Observable<Category> {
    return this.http.put<Category>(`${API_URL}/Categories/${id}`, category);
  }

  deleteCategory(id: number): Observable<void> {
    return this.http.delete<void>(`${API_URL}/Categories/${id}`);
  }

  // Brands
  getBrands(): Observable<Brand[]> {
    return this.http.get<Brand[]>(`${API_URL}/Brands`);
  }

  getBrandById(id: number): Observable<Brand> {
    return this.http.get<Brand>(`${API_URL}/Brands/${id}`);
  }

  createBrand(brand: CreateBrandRequest): Observable<Brand> {
    return this.http.post<Brand>(`${API_URL}/Brands`, brand);
  }

  updateBrand(
    id: number,
    brand: UpdateBrandRequest
  ): Observable<Brand> {
    return this.http.put<Brand>(`${API_URL}/Brands/${id}`, brand);
  }

  deleteBrand(id: number): Observable<void> {
    return this.http.delete<void>(`${API_URL}/Brands/${id}`);
  }

  // Stock
  adjustStock(adjustment: StockAdjustmentRequest): Observable<unknown> {
    return this.http.post<unknown>(`${API_URL}/Stock/adjust`, adjustment);
  }

  getStockSummary(): Observable<StockSummary> {
    return this.http.get<StockSummary>(`${API_URL}/Stock/summary`);
  }

  getStockMovements(): Observable<StockMovementPage> {
    return this.http.get<StockMovementPage>(
      `${API_URL}/Stock/movements?pageNumber=1&pageSize=100`
    );
  }

  // Orders
  getOrders(
    pageNumber = 1,
    pageSize = 10,
    search = '',
    status = ''
  ): Observable<OrderPage> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    if (search.trim()) {
      params = params.set('search', search.trim());
    }

    if (status.trim()) {
      params = params.set('status', status.trim());
    }

    return this.http.get<OrderPage>(
      `${API_URL}/Orders`,
      { params }
    );
  }

  getOrderById(id: number): Observable<Order> {
    return this.http.get<Order>(`${API_URL}/Orders/${id}`);
  }

  createOrder(order: CreateOrderRequest): Observable<Order> {
    return this.http.post<Order>(`${API_URL}/Orders`, order);
  }

  cancelOrder(id: number): Observable<CancelOrderResponse> {
    return this.http.put<CancelOrderResponse>(
      `${API_URL}/Orders/${id}/cancel`,
      {}
    );
  }
}
