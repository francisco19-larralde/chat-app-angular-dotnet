import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { GroupDetails } from '../../models/group.model';

@Injectable({ providedIn: 'root' })
export class GroupService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/groups`;



  createGroup(name: string, memberIds: number[]) {
    return this.http.post<{ chatId: number }>(this.apiUrl, { name, memberIds });
  }

  getGroupDetails(chatId: number) {
    return this.http.get<GroupDetails>(`${this.apiUrl}/${chatId}`);
  }

  addMember(chatId: number, userId: number) {
    return this.http.post<void>(`${this.apiUrl}/${chatId}/members/${userId}`, {});
  }

  removeMember(chatId: number, userId: number) {
    return this.http.delete<void>(`${this.apiUrl}/${chatId}/members/${userId}`);
  }

  leaveGroup(chatId: number) {
    return this.http.post<void>(`${this.apiUrl}/${chatId}/leave`, {});
  }

  updateGroup(chatId: number, name: string) {
    return this.http.put<void>(`${this.apiUrl}/${chatId}`, { name });
  }
}
