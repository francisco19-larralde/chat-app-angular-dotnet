import { Routes } from '@angular/router';
import { MainLayout } from './layout/main-layout/main-layout';
import { Login } from './features/auth/login/login';
import { Register } from './features/auth/register/register';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: Login },
  { path: 'register', component: Register },

  {
    path: '',
    component: MainLayout,
    canActivate: [authGuard],
    children: [
      // Acá vamos a ir agregando las rutas hijas: chat, amigos, perfil, etc
      // Por ahora dejamos un placeholder vacío en la Etapa 5
    ]
  }
];
