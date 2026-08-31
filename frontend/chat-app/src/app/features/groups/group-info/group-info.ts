import { Component, inject, input, output, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { GroupService } from '../../../core/services/group.service';
import { FriendService } from '../../../core/services/friend.service';
import { ChatService } from '../../../core/services/chat.service';
import { AuthService } from '../../../core/services/auth.service';
import { GroupDetails } from '../../../models/group.model';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-group-info',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './group-info.html'
})
export class GroupInfo implements OnInit {
  chatId = input.required<number>();
  close = output<void>();

  private groupService = inject(GroupService);
  private friendService = inject(FriendService);
  private chatService = inject(ChatService);
  private authService = inject(AuthService);
  private router = inject(Router);

  readonly serverUrl = environment.apiUrl.replace('/api', '');

  details = signal<GroupDetails | null>(null);
  isLoading = signal(true);
  errorMessage = signal<string | null>(null);

  friends = this.friendService.friends;
  addableFriends = signal<{ userId: number; username: string; profilePictureUrl?: string }[]>([]);

  ngOnInit(): void {
    this.loadDetails();
    if (this.friends().length === 0) {
      this.friendService.loadFriends().subscribe();
    }
  }

  private loadDetails(): void {
    this.isLoading.set(true);
    this.groupService.getGroupDetails(this.chatId()).subscribe({
      next: (details) => {
        this.details.set(details);
        this.isLoading.set(false);
        this.updateAddableFriends();
      },
      error: () => this.isLoading.set(false)
    });
  }

  private updateAddableFriends(): void {
    const memberIds = new Set(this.details()?.members.map(m => m.userId) ?? []);
    this.addableFriends.set(this.friends().filter(f => !memberIds.has(f.userId)));
  }

  addMember(userId: number): void {
    this.groupService.addMember(this.chatId(), userId).subscribe({
      next: () => this.loadDetails()
    });
  }

  removeMember(userId: number): void {
    if (!confirm('¿Sacar a este miembro del grupo?')) return;

    this.groupService.removeMember(this.chatId(), userId).subscribe({
      next: () => this.loadDetails(),
      error: (err) => this.errorMessage.set(err.error?.message ?? 'Error al quitar al miembro.')
    });
  }

  leaveGroup(): void {
    if (!confirm('¿Salir de este grupo?')) return;

    this.groupService.leaveGroup(this.chatId()).subscribe({
      next: () => {
        this.chatService.loadChats().subscribe();
        this.close.emit();
        this.router.navigate(['/friends']);
      },
      error: (err) => this.errorMessage.set(err.error?.message ?? 'Error al salir del grupo.')
    });
  }

  getImageUrl(relativeUrl?: string): string | null {
    return relativeUrl ? `${this.serverUrl}${relativeUrl}` : null;
  }
}
