
import { Routes } from '@angular/router';

import { Dashboard } from './pages/dashboard/dashboard';
import { Products } from './pages/products/products';
import { Categories } from './pages/categories/categories';
import { Brands } from './pages/brands/brands';
import { Orders } from './pages/orders/orders';
import { StockManagement } from './pages/stock-management/stock-management';

import { Shop } from './pages/shop/shop';
import { ProductDetails } from './pages/product-details/product-details';
import { Cart } from './pages/cart/cart';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { Profile } from './pages/profile/profile';
import { MyOrders } from './pages/my-orders/my-orders';

export const routes: Routes = [

  // Customer Pages
  { path: 'shop', component: Shop },
  { path: 'product-details/:id', component: ProductDetails },
  { path: 'cart', component: Cart },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'profile', component: Profile },
  { path: 'my-orders', component: MyOrders },

  // Admin Pages
  { path: 'dashboard', component: Dashboard },
  { path: 'products', component: Products },
  { path: 'categories', component: Categories },
  { path: 'brands', component: Brands },
  { path: 'orders', component: Orders },
  { path: 'stock-management', component: StockManagement },

  // Default Routes
  { path: '', redirectTo: 'shop', pathMatch: 'full' },
  { path: '**', redirectTo: 'shop' }

];
