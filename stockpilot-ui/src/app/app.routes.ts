import { Routes } from '@angular/router';

import { Dashboard } from './pages/dashboard/dashboard';
import { Products } from './pages/products/products';
import { Categories } from './pages/categories/categories';
import { Brands } from './pages/brands/brands';
import { Orders } from './pages/orders/orders';
import { StockManagement } from './pages/stock-management/stock-management';

import { Shop } from './pages/shop/shop';
import { Cart } from './pages/cart/cart';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { Profile } from './pages/profile/profile';

export const routes: Routes = [
  { path: 'shop', component: Shop },
  { path: 'cart', component: Cart },

  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'profile', component: Profile },

  { path: 'dashboard', component: Dashboard },
  { path: 'products', component: Products },
  { path: 'categories', component: Categories },
  { path: 'brands', component: Brands },
  { path: 'orders', component: Orders },
  { path: 'stock-management', component: StockManagement },

  { path: '', redirectTo: 'shop', pathMatch: 'full' },
  { path: '**', redirectTo: 'shop' }
];