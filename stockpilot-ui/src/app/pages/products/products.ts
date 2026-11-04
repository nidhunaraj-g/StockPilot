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
  UpdateProductRequest,
  ProductImageRequest
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

  // Image URL fields
  newImageUrl = '';
  newImageAltText = '';

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
      minimumStockLevel: 0,
      images: []
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

    this.newImageUrl = '';
    this.newImageAltText = '';

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
      minimumStockLevel: product.minimumStockLevel,
      images: (product.images ?? []).map(image => ({
        imageUrl: image.imageUrl,
        altText: image.altText,
        isPrimary: image.isPrimary,
        sortOrder: image.sortOrder
      }))
    };

    this.newImageUrl = '';
    this.newImageAltText = '';

    this.showForm = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  // Add a product image using its URL
  addImageUrl(): void {
    const imageUrl = this.newImageUrl.trim();

    if (!imageUrl) {
      this.errorMessage = 'Please enter an image URL.';
      return;
    }

    try {
      const parsedUrl = new URL(imageUrl);

      if (
        parsedUrl.protocol !== 'http:' &&
        parsedUrl.protocol !== 'https:'
      ) {
        this.errorMessage = 'Please enter a valid HTTP or HTTPS image URL.';
        return;
      }
    } catch {
      this.errorMessage = 'Please enter a valid image URL.';
      return;
    }

    const alreadyExists = this.form.images.some(
      image => image.imageUrl.toLowerCase() === imageUrl.toLowerCase()
    );

    if (alreadyExists) {
      this.errorMessage = 'This image URL has already been added.';
      return;
    }

    const isFirstImage = this.form.images.length === 0;

    const image: ProductImageRequest = {
      imageUrl,
      altText: this.newImageAltText.trim() || null,
      isPrimary: isFirstImage,
      sortOrder: this.form.images.length
    };

    this.form.images = [
      ...this.form.images,
      image
    ];

    this.newImageUrl = '';
    this.newImageAltText = '';
    this.errorMessage = '';

    this.cdr.detectChanges();
  }

  // Remove an image
  removeImage(index: number): void {
    this.form.images.splice(index, 1);

    this.form.images = this.form.images.map((image, imageIndex) => ({
      ...image,
      sortOrder: imageIndex,
      isPrimary: imageIndex === 0
    }));

    this.cdr.detectChanges();
  }

  // Select the primary image
  setPrimaryImage(index: number): void {
    this.form.images = this.form.images.map((image, imageIndex) => ({
      ...image,
      isPrimary: imageIndex === index
    }));

    this.cdr.detectChanges();
  }

  // Handle an image URL that fails to load in the preview
  onImagePreviewError(event: Event): void {
    const imageElement = event.target as HTMLImageElement;
    imageElement.style.display = 'none';
  }

  closeForm(): void {
    if (this.isSaving) {
      return;
    }

    this.showForm = false;
    this.form = this.emptyForm();

    this.newImageUrl = '';
    this.newImageAltText = '';

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
      minimumStockLevel: Number(this.form.minimumStockLevel),

      images: (this.form.images ?? []).map((image, index) => ({
        imageUrl: image.imageUrl.trim(),
        altText: image.altText?.trim() || null,
        isPrimary: image.isPrimary,
        sortOrder: index
      }))
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