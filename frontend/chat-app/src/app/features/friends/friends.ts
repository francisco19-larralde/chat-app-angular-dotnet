import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FriendService } from '../../core/services/friend.service';
import { UserSearchResult } from '../../models/friend.model';
import { environment } from '../../../environments/environment';
import { debounceTime, distinctUntilChanged, Subject, switchMap } from 'rxjs';
import { ChatService } from '../../core/services/chat.service';
import { Router } from '@angular/router';

type Tab = 'search' | 'requests' | 'friends';

@Component({
  selector: 'app-friends',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './friends.html'
})
export class Friends implements OnInit {
  private friendService = inject(FriendService);
  readonly serverUrl = environment.apiUrl.replace('/api', '');
  private chatService = inject(ChatService);
  private router = inject(Router);

  activeTab = signal<Tab>('search');

  searchQuery = '';
  searchResults = signal<UserSearchResult[]>([]);
  isSearching = signal(false);

  requests = this.friendService.pendingRequests;
  friends = this.friendService.friends;


  private searchInput$ = new Subject<string>();

  constructor() {
    this.searchInput$.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      switchMap(query => {
        this.isSearching.set(true);
        return this.friendService.searchUsers(query);
      })
    ).subscribe(results => {
      this.isSearching.set(false);
      this.searchResults.set(results);
    });
  }

  ngOnInit(): void {
    this.friendService.loadPendingRequests().subscribe();
    this.friendService.loadFriends().subscribe();
  }

  onSearchChange(): void {
    if (this.searchQuery.trim().length < 2) {
      this.searchResults.set([]);
      return;
    }
    this.searchInput$.next(this.searchQuery.trim());
  }

  sendRequest(userId: number): void {
    this.friendService.sendFriendRequest(userId).subscribe({
      next: () => {
        this.searchResults.update(list =>
          list.map(u => u.id === userId ? { ...u, friendshipStatus: 'Pending' as const } : u)
        );
      }
    });
  }

  acceptRequest(friendshipId: number): void {
    this.friendService.acceptRequest(friendshipId).subscribe({
      next: () => this.friendService.loadFriends().subscribe()
    });
  }

  rejectRequest(friendshipId: number): void {
    this.friendService.rejectRequest(friendshipId).subscribe();
  }

  removeFriend(userId: number): void {
    this.friendService.removeFriend(userId).subscribe();
  }

  getImageUrl(relativeUrl?: string): string | null {
    return relativeUrl ? `${this.serverUrl}${relativeUrl}` : null;
  }

  startChat(friendUserId: number): void {
    this.chatService.getOrCreatePrivateChat(friendUserId).subscribe({
      next: ({ chatId }) => {
        this.chatService.loadChats().subscribe();
        this.router.navigate(['/chat', chatId]);
      }
    });
  }


}
