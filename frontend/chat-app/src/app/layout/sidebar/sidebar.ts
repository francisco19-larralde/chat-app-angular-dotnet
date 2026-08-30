import { Component, inject, OnInit } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { ChatService } from '../../core/services/chat.service';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { environment } from '../../../environments/environment';

@Component({
  imports: [RouterLink, RouterLinkActive],
  selector: 'app-sidebar',
  styleUrl: './sidebar.css',
  templateUrl: './sidebar.html',
})
export class Sidebar implements OnInit {
  private authService = inject(AuthService);
  private chatService = inject(ChatService);
  readonly serverUrl = environment.apiUrl.replace('/api', '');

  user = this.authService.currentUser;
  chats = this.chatService.chats;

  ngOnInit(): void {
    this.chatService.loadChats().subscribe();
  }

  getImageUrl(relativeUrl?: string): string | null {
    return relativeUrl ? `${this.serverUrl}${relativeUrl}` : null;
  }

  logout(): void {
    this.authService.logout();
  }
}
