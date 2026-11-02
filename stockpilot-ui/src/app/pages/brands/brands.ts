
import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';

import {
  Brand,
  CreateBrandRequest,
  ProductService,
  UpdateBrandRequest
} from '../../services/product';

@Component({
  selector: 'app-brands',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './brands.html',
  styleUrl: './brands.scss'
})
export class Brands implements OnInit {
  private productService = inject(ProductService);
  private cdr = inject(ChangeDetectorRef);

  brands: Brand[] = [];

  loading = false;
  saving = false;

  searchTerm = '';

  showModal = false;
  showDeleteModal = false;
  isEditMode = false;

  selectedBrandId: number | null = null;
  brandToDelete: Brand | null = null;

  successMessage = '';
  errorMessage = '';

  brandForm: CreateBrandRequest = this.getEmptyForm();

  ngOnInit(): void {
    this.loadBrands();
  }

  get filteredBrands(): Brand[] {
    const search = this.searchTerm.trim().toLowerCase();

    if (!search) {
      return this.brands;
    }

    return this.brands.filter(brand =>
      brand.name.toLowerCase().includes(search) ||
      (brand.description ?? '').toLowerCase().includes(search)
    );
  }

  get activeBrandsCount(): number {
    return this.brands.filter(brand => brand.isActive).length;
  }

  private getEmptyForm(): CreateBrandRequest {
    return {
      name: '',
      description: '',
      logoUrl: null
    };
  }

  loadBrands(): void {
    this.loading = true;
    this.errorMessage = '';

    this.productService.getBrands()
      .pipe(
        finalize(() => {
          this.loading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (response) => {
          this.brands = response ?? [];
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Error loading brands:', error);
          this.errorMessage =
            error.error?.message || 'Unable to load brands. Please try again.';
          this.cdr.detectChanges();
        }
      });
  }

  openAddModal(): void {
    this.isEditMode = false;
    this.selectedBrandId = null;
    this.brandForm = this.getEmptyForm();
    this.successMessage = '';
    this.errorMessage = '';
    this.showModal = true;
  }

  openEditModal(brand: Brand): void {
    this.isEditMode = true;
    this.selectedBrandId = brand.id;

    this.brandForm = {
      name: brand.name,
      description: brand.description ?? '',
      logoUrl: brand.logoUrl
    };

    this.successMessage = '';
    this.errorMessage = '';
    this.showModal = true;
  }

  closeModal(): void {
    if (this.saving) {
      return;
    }

    this.showModal = false;
    this.brandForm = this.getEmptyForm();
    this.selectedBrandId = null;
  }

  saveBrand(): void {
    const name = this.brandForm.name.trim();

    if (!name) {
      this.errorMessage = 'Brand name is required.';
      return;
    }

    this.saving = true;
    this.errorMessage = '';
    this.successMessage = '';

    const request: CreateBrandRequest = {
      name,
      description: this.brandForm.description.trim(),
      logoUrl: this.brandForm.logoUrl?.trim() || null
    };

    const operation = this.isEditMode && this.selectedBrandId !== null
      ? this.productService.updateBrand(
          this.selectedBrandId,
          request as UpdateBrandRequest
        )
      : this.productService.createBrand(request);

    operation
      .pipe(
        finalize(() => {
          this.saving = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: () => {
          this.showModal = false;
          this.successMessage = this.isEditMode
            ? 'Brand updated successfully.'
            : 'Brand created successfully.';

          this.brandForm = this.getEmptyForm();
          this.selectedBrandId = null;
          this.loadBrands();
        },
        error: (error) => {
          console.error('Error saving brand:', error);
          this.errorMessage =
            error.error?.message || 'Unable to save brand. Please try again.';
        }
      });
  }

  openDeleteModal(brand: Brand): void {
    this.brandToDelete = brand;
    this.showDeleteModal = true;
    this.errorMessage = '';
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.brandToDelete = null;
  }

  deleteBrand(): void {
    if (!this.brandToDelete) {
      return;
    }

    const brandId = this.brandToDelete.id;

    this.productService.deleteBrand(brandId)
      .pipe(
        finalize(() => {
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: () => {
          this.showDeleteModal = false;
          this.successMessage = 'Brand deleted successfully.';
          this.brandToDelete = null;
          this.loadBrands();
        },
        error: (error) => {
          console.error('Error deleting brand:', error);
          this.errorMessage =
            error.error?.message || 'Unable to delete brand. Please try again.';
          this.showDeleteModal = false;
          this.brandToDelete = null;
        }
      });
  }

  clearMessages(): void {
    this.successMessage = '';
    this.errorMessage = '';
  }
}
