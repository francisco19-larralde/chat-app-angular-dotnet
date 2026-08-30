import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { UserSearchResult, FriendRequest, Friend } from '../../models/friend.model';

@Injectable({ providedIn: 'root' })
export class FriendService {
  private readonly apiUrl = `${environment.apiUrl}/friends`;
  private readonly http = inject(HttpClient);
  readonly friends = signal<Friend[]>([]);
  readonly pendingRequests = signal<FriendRequest[]>([]);


  searchUsers(query: string) {
    return this.http.get<UserSearchResult[]>(`${this.apiUrl}/search`, { params: { query } });
  }

  sendFriendRequest(addresseeId: number) {
    return this.http.post<void>(`${this.apiUrl}/request/${addresseeId}`, {});
  }

  loadPendingRequests() {
    return this.http.get<FriendRequest[]>(`${this.apiUrl}/requests/pending`).pipe(
      tap(requests => this.pendingRequests.set(requests))
    );
  }

  acceptRequest(friendshipId: number) {
    return this.http.post<void>(`${this.apiUrl}/requests/${friendshipId}/accept`, {}).pipe(
      tap(() => {
        this.pendingRequests.update(list => list.filter(r => r.friendshipId !== friendshipId));
      })
    );
  }

  rejectRequest(friendshipId: number) {
    return this.http.post<void>(`${this.apiUrl}/requests/${friendshipId}/reject`, {}).pipe(
      tap(() => {
        this.pendingRequests.update(list => list.filter(r => r.friendshipId !== friendshipId));
      })
    );
  }

  loadFriends() {
    return this.http.get<Friend[]>(this.apiUrl).pipe(
      tap(friends => this.friends.set(friends))
    );
  }

  removeFriend(friendUserId: number) {
    return this.http.delete<void>(`${this.apiUrl}/${friendUserId}`).pipe(
      tap(() => {
        this.friends.update(list => list.filter(f => f.userId !== friendUserId));
      })
    );
  }
}
