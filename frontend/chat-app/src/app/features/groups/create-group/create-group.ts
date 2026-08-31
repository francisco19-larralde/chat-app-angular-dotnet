import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { GroupService } from '../../../core/services/group.service';
import { FriendService } from '../../../core/services/friend.service';
import { ChatService } from '../../../core/services/chat.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-create-group',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './create-group.html'
})
export class CreateGroup implements OnInit {
  private groupService = inject(GroupService);
  private friendService = inject(FriendService);
  private chatService = inject(ChatService);
  private router = inject(Router);

  readonly serverUrl = environment.apiUrl.replace('/api', '');

  friends = this.friendService.friends;
  selectedIds = signal<Set<number>>(new Set());
  groupName = '';
  isCreating = signal(false);
  errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    if (this.friends().length === 0) {
      this.friendService.loadFriends().subscribe();
    }
  }

  toggleMember(userId: number): void {
    this.selectedIds.update(set => {
      const updated = new Set(set);
      updated.has(userId) ? updated.delete(userId) : updated.add(userId);
      return updated;
    });
  }

  isSelected(userId: number): boolean {
    return this.selectedIds().has(userId);
  }

  getImageUrl(relativeUrl?: string): string | null {
    return relativeUrl ? `${this.serverUrl}${relativeUrl}` : null;
  }

  createGroup(): void {
    if (this.groupName.trim().length < 2) {
      this.errorMessage.set('El nombre del grupo debe tener al menos 2 caracteres.');
      return;
    }

    if (this.selectedIds().size === 0) {
      this.errorMessage.set('Elegí al menos un amigo para el grupo.');
      return;
    }

    this.isCreating.set(true);
    this.errorMessage.set(null);

    this.groupService.createGroup(this.groupName.trim(), Array.from(this.selectedIds())).subscribe({
      next: ({ chatId }) => {
        this.chatService.loadChats().subscribe();
        this.router.navigate(['/chat', chatId]);
      },
      error: (err) => {
        this.isCreating.set(false);
        this.errorMessage.set(err.error?.message ?? 'Error al crear el grupo.');
      }
    });
  }
}
