
import { CommonModule } from '@angular/common';
import {
  Component,
  OnInit,
  inject,
  ChangeDetectorRef
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { finalize } from 'rxjs';

import {
  Category,
  CreateCategoryRequest,
  ProductService
} from '../../services/product';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  templateUrl: './categories.html',
  styleUrl: './categories.scss'
})
export class Categories implements OnInit {

  private productService = inject(ProductService);
  private cdr = inject(ChangeDetectorRef);

  categories: Category[] = [];
  loading = false;
  saving = false;
  deleting = false;

  searchTerm = '';

  showForm = false;
  isEditMode = false;
  editingCategoryId: number | null = null;

  showDeleteConfirmation = false;
  categoryToDelete: Category | null = null;

  categoryForm: CreateCategoryRequest = {
    name: '',
    description: ''
  };

  errorMessage = '';
  successMessage = '';

  ngOnInit(): void {
    this.loadCategories();
  }

  get filteredCategories(): Category[] {
    const search = this.searchTerm.trim().toLowerCase();

    if (!search) return this.categories;

    return this.categories.filter(category =>
      category.name.toLowerCase().includes(search) ||
      (category.description ?? '').toLowerCase().includes(search)
    );
  }

  get activeCategoryCount(): number {
    return this.categories.filter(category => category.isActive).length;
  }

  loadCategories(): void {
    this.loading = true;
    this.errorMessage = '';
    this.cdr.detectChanges();

    console.log('Loading categories...');

    this.productService.getCategories()
      .pipe(
        finalize(() => {
          this.loading = false;
          console.log('Category loading finished.');
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (response) => {
          console.log('Categories API response:', response);

          this.categories = Array.isArray(response) ? response : [];

          console.log('Categories loaded:', this.categories.length);
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Error loading categories:', error);

          this.categories = [];
          this.errorMessage =
            error.error?.message ??
            'Unable to load categories. Please try again.';

          this.cdr.detectChanges();
        }
      });
  }

  openAddForm(): void {
    this.resetForm();
    this.isEditMode = false;
    this.editingCategoryId = null;
    this.showForm = true;
  }

  openEditForm(category: Category): void {
    this.resetForm();

    this.isEditMode = true;
    this.editingCategoryId = category.id;

    this.categoryForm = {
      name: category.name,
      description: category.description ?? ''
    };

    this.showForm = true;
  }

  closeForm(): void {
    if (this.saving) return;

    this.showForm = false;
    this.resetForm();
  }

  saveCategory(): void {
    const name = this.categoryForm.name.trim();
    const description = this.categoryForm.description.trim();

    if (!name) {
      this.errorMessage = 'Category name is required.';
      return;
    }

    const request: CreateCategoryRequest = { name, description };

    this.saving = true;
    this.errorMessage = '';

    const operation =
      this.isEditMode && this.editingCategoryId !== null
        ? this.productService.updateCategory(this.editingCategoryId, request)
        : this.productService.createCategory(request);

    operation.pipe(
      finalize(() => {
        this.saving = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: () => {
        this.showForm = false;
        this.successMessage = this.isEditMode
          ? 'Category updated successfully.'
          : 'Category created successfully.';

        this.resetForm();
        this.loadCategories();
      },
      error: (error) => {
        console.error('Error saving category:', error);
        this.errorMessage =
          error.error?.message ?? 'Unable to save category.';
      }
    });
  }

  openDeleteConfirmation(category: Category): void {
    this.categoryToDelete = category;
    this.showDeleteConfirmation = true;
    this.errorMessage = '';
  }

  closeDeleteConfirmation(): void {
    if (this.deleting) return;

    this.showDeleteConfirmation = false;
    this.categoryToDelete = null;
  }

  confirmDelete(): void {
    if (!this.categoryToDelete) return;

    const id = this.categoryToDelete.id;
    this.deleting = true;

    this.productService.deleteCategory(id).pipe(
      finalize(() => {
        this.deleting = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: () => {
        this.successMessage = 'Category deleted successfully.';
        this.showDeleteConfirmation = false;
        this.categoryToDelete = null;
        this.loadCategories();
      },
      error: (error) => {
        console.error('Error deleting category:', error);

        this.errorMessage =
          error.error?.message ?? 'Unable to delete category.';

        this.showDeleteConfirmation = false;
        this.categoryToDelete = null;
      }
    });
  }

  clearMessages(): void {
    this.successMessage = '';
    this.errorMessage = '';
  }

  private resetForm(): void {
    this.categoryForm = {
      name: '',
      description: ''
    };

    this.errorMessage = '';
  }
}
