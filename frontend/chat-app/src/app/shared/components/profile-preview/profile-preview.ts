import { Component, inject, input, output, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { UserService } from '../../../core/services/user.service';
import { ChatService } from '../../../core/services/chat.service';
import { PublicUserProfile } from '../../../models/user.model';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-profile-preview',
  standalone: true,
  templateUrl: './profile-preview.html'
})
export class ProfilePreview implements OnInit {
  userId = input.required<number>();
  close = output<void>();

  private userService = inject(UserService);
  private chatService = inject(ChatService);
  private router = inject(Router);

  readonly serverUrl = environment.apiUrl.replace('/api', '');

  profile = signal<PublicUserProfile | null>(null);
  isLoading = signal(true);
  isStartingChat = signal(false);

  ngOnInit(): void {
    this.userService.getPublicProfile(this.userId()).subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  getImageUrl(relativeUrl?: string): string | null {
    return relativeUrl ? `${this.serverUrl}${relativeUrl}` : null;
  }

  startChat(): void {
    this.isStartingChat.set(true);
    this.chatService.getOrCreatePrivateChat(this.userId()).subscribe({
      next: ({ chatId }) => {
        this.chatService.loadChats().subscribe();
        this.close.emit();
        this.router.navigate(['/chat', chatId]);
      },
      error: () => this.isStartingChat.set(false)
    });
  }


  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.close.emit();
    }
  }
}
