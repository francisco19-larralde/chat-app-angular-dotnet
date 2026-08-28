import { Routes } from '@angular/router';
import { MainLayout } from './layout/main-layout/main-layout';

export const routes: Routes = [
  {
    path: '',
    component: MainLayout,
    children: [
      // Acá vamos a ir agregando las rutas hijas: chat, amigos, perfil, etc
      // Por ahora dejamos un placeholder vacío en la Etapa 5
    ]
  }
  // La ruta de /login y /register van a vivir FUERA de este layout,
  // porque un usuario no logueado no debe ver el sidebar
];
