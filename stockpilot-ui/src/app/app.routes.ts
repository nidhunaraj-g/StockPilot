
import { Routes } from '@angular/router';
import { Dashboard } from './pages/dashboard/dashboard';
import { Products } from './pages/products/products';
import { Categories } from './pages/categories/categories';
import { Brands } from './pages/brands/brands';
import { Orders } from './pages/orders/orders';
import { StockManagement } from './pages/stock-management/stock-management';

export const routes: Routes = [
  {
    path: 'dashboard',
    component: Dashboard
  },
  {
    path: 'products',
    component: Products
  },
  {
    path: 'categories',
    component: Categories
  },
  { 
    path: 'brands',
    component: Brands 
  },
  { 
    path: 'orders', 
    component: Orders 
  },
  {
    path: 'stock-management',
    component: StockManagement
  },
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
