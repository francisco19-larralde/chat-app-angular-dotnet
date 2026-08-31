import { Routes } from '@angular/router';
import { MainLayout } from './layout/main-layout/main-layout';
import { Login } from './features/auth/login/login';
import { Register } from './features/auth/register/register';
import { authGuard } from './core/guards/auth.guard';
import { Profile } from './features/profile/profile';
import { Friends } from './features/friends/friends';
import { guestGuard } from './core/guards/guest.guard';
import { ChatWindow } from './features/chat/chat-window/chat-window';
import { CreateGroup } from './features/groups/create-group/create-group';

export const routes: Routes = [
  { path: 'login', component: Login, canActivate: [guestGuard] },
  { path: 'register', component: Register, canActivate: [guestGuard] },

  {
    path: '',
    component: MainLayout,
    canActivate: [authGuard],
    children: [
      { path: 'profile', component: Profile },
      { path: 'friends', component: Friends },
      { path: 'chat/:chatId', component: ChatWindow },
      { path: 'groups/new', component: CreateGroup }
    ]
  }
];
