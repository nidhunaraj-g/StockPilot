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
  Category,
  Brand,
  CreateProductRequest,
  UpdateProductRequest
} from '../../services/product';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule
  ],
  templateUrl: './products.html',
  styleUrl: './products.scss'
})
export class Products implements OnInit {

  private productService = inject(ProductService);
  private cdr = inject(ChangeDetectorRef);

  products: Product[] = [];
  categories: Category[] = [];
  brands: Brand[] = [];

  isLoading = false;
  isSaving = false;

  errorMessage = '';
  successMessage = '';

  searchTerm = '';
  selectedCategoryId = '';
  selectedBrandId = '';

  pageNumber = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  showForm = false;
  isEditMode = false;
  editingProductId: number | null = null;

  showDeleteConfirmation = false;
  deletingProduct: Product | null = null;

  form: CreateProductRequest = this.emptyForm();

  ngOnInit(): void {
    this.loadProducts();
    this.loadDropdownData();
  }

  private emptyForm(): CreateProductRequest {
    return {
      sku: '',
      barcode: null,
      name: '',
      description: null,
      categoryId: 0,
      brandId: null,
      costPrice: 0,
      sellingPrice: 0,
      minimumStockLevel: 0
    };
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
        this.products = response.products ?? [];
        this.totalCount = response.totalCount;
        this.totalPages = response.totalPages;
        this.pageNumber = response.pageNumber;

        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: error => {
        console.error('Products loading error:', error);

        this.errorMessage =
          error.error?.message ?? 'Unable to load products.';

        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadDropdownData(): void {
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

    this.productService.getBrands().subscribe({
      next: brands => {
        this.brands = brands.filter(
          brand => brand.isActive
        );
        this.cdr.detectChanges();
      },
      error: error => {
        console.error('Brands loading error:', error);
      }
    });
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

  get filteredProducts(): Product[] {
    return this.products.filter(product => {
      const categoryMatches =
        !this.selectedCategoryId ||
        product.categoryId === Number(this.selectedCategoryId);

      const brandMatches =
        !this.selectedBrandId ||
        product.brandId === Number(this.selectedBrandId);

      return categoryMatches && brandMatches;
    });
  }

  openAddForm(): void {
    this.form = this.emptyForm();
    this.isEditMode = false;
    this.editingProductId = null;
    this.showForm = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  openEditForm(product: Product): void {
    this.isEditMode = true;
    this.editingProductId = product.id;

    this.form = {
      sku: product.sku,
      barcode: product.barcode,
      name: product.name,
      description: product.description,
      categoryId: product.categoryId,
      brandId: product.brandId,
      costPrice: product.costPrice,
      sellingPrice: product.sellingPrice,
      minimumStockLevel: product.minimumStockLevel
    };

    this.showForm = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  closeForm(): void {
    if (this.isSaving) {
      return;
    }

    this.showForm = false;
    this.form = this.emptyForm();
    this.errorMessage = '';
  }

  saveProduct(): void {
    if (this.isSaving) {
      return;
    }

    if (
      !this.form.sku.trim() ||
      !this.form.name.trim() ||
      !this.form.categoryId ||
      this.form.costPrice < 0 ||
      this.form.sellingPrice < 0 ||
      this.form.minimumStockLevel < 0
    ) {
      this.errorMessage =
        'Please complete all required fields with valid values.';
      return;
    }

    if (this.form.sellingPrice < this.form.costPrice) {
      this.errorMessage =
        'Selling price should not be less than cost price.';
      return;
    }

    const request: CreateProductRequest = {
      sku: this.form.sku.trim(),
      barcode: this.form.barcode?.trim() || null,
      name: this.form.name.trim(),
      description: this.form.description?.trim() || null,
      categoryId: Number(this.form.categoryId),
      brandId: this.form.brandId
        ? Number(this.form.brandId)
        : null,
      costPrice: Number(this.form.costPrice),
      sellingPrice: Number(this.form.sellingPrice),
      minimumStockLevel: Number(this.form.minimumStockLevel)
    };

    this.isSaving = true;
    this.errorMessage = '';

    if (this.isEditMode && this.editingProductId !== null) {
      const updateRequest: UpdateProductRequest = request;

      this.productService.updateProduct(
        this.editingProductId,
        updateRequest
      ).subscribe({
        next: () => {
          this.successMessage = 'Product updated successfully!';
          this.showForm = false;
          this.isSaving = false;
          this.loadProducts();
          this.cdr.detectChanges();
        },
        error: error => {
          this.handleSaveError(error);
        }
      });
    } else {
      this.productService.createProduct(request).subscribe({
        next: () => {
          this.successMessage = 'Product created successfully!';
          this.showForm = false;
          this.pageNumber = 1;
          this.isSaving = false;
          this.loadProducts();
          this.cdr.detectChanges();
        },
        error: error => {
          this.handleSaveError(error);
        }
      });
    }
  }

  private handleSaveError(error: any): void {
    console.error('Product save error:', error);

    this.errorMessage =
      error.error?.message ??
      'Unable to save product. Check the entered information.';

    this.isSaving = false;
    this.cdr.detectChanges();
  }

  confirmDelete(product: Product): void {
    this.deletingProduct = product;
    this.showDeleteConfirmation = true;
  }

  cancelDelete(): void {
    this.showDeleteConfirmation = false;
    this.deletingProduct = null;
  }

  deleteProduct(): void {
    if (!this.deletingProduct) {
      return;
    }

    const id = this.deletingProduct.id;

    this.productService.deleteProduct(id).subscribe({
      next: () => {
        this.successMessage = 'Product deleted successfully.';
        this.showDeleteConfirmation = false;
        this.deletingProduct = null;

        if (this.products.length === 1 && this.pageNumber > 1) {
          this.pageNumber--;
        }

        this.loadProducts();
      },
      error: error => {
        console.error('Product delete error:', error);

        this.errorMessage =
          error.error?.message ?? 'Unable to delete product.';

        this.showDeleteConfirmation = false;
        this.deletingProduct = null;
        this.cdr.detectChanges();
      }
    });
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

  onPageSizeChange(): void {
    this.pageNumber = 1;
    this.loadProducts();
  }

  clearFilters(): void {
    this.selectedCategoryId = '';
    this.selectedBrandId = '';
    this.searchTerm = '';
    this.pageNumber = 1;
    this.loadProducts();
  }

}