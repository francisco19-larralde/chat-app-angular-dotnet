import { Component, inject, OnInit } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { ChatService } from '../../core/services/chat.service';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { environment } from '../../../environments/environment';
import { SignalRService } from '../../core/services/signal-r.service';

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

  private signalRService = inject(SignalRService);
  onlineUserIds = this.signalRService.onlineUserIds;

  user = this.authService.currentUser;
  chats = this.chatService.chats;

  ngOnInit(): void {
    this.chatService.loadChats().subscribe();
  }

  getImageUrl(relativeUrl?: string): string | null {
    return relativeUrl ? `${this.serverUrl}${relativeUrl}` : null;
  }

  isOnline(chat: { isGroup: boolean; otherUserId?: number; isOtherUserOnline: boolean }): boolean {
    if (chat.isGroup || chat.otherUserId === undefined) {
      return false;
    }
    return this.onlineUserIds().has(chat.otherUserId);
  }

  logout(): void {
    this.authService.logout();
  }
}
