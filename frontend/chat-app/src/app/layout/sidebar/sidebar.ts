import { Component, inject } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';

@Component({
  imports: [],
  selector: 'app-sidebar',
  styleUrl: './sidebar.css',
  templateUrl: './sidebar.html',
})
export class Sidebar {
  private authService = inject(AuthService);


  user = this.authService.currentUser;


  logout(): void {
    this.authService.logout();
  }
}
