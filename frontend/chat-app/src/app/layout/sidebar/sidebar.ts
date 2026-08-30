import { Component, inject, signal } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { RouterLink } from '@angular/router';
import { environment } from '../../../environments/environment';

@Component({
  imports: [RouterLink],
  selector: 'app-sidebar',
  styleUrl: './sidebar.css',
  templateUrl: './sidebar.html',
})
export class Sidebar {
  private authService = inject(AuthService);
  readonly serverUrl = environment.apiUrl.replace('/api', '');


  user = this.authService.currentUser;

  getImageUrl(relativeUrl?: string): string | null {
    return relativeUrl ? `${this.serverUrl}${relativeUrl}` : null;
  }


  logout(): void {
    this.authService.logout();
  }
}
