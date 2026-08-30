import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ChatSummary, Message } from '../../models/chat.model';

@Injectable({ providedIn: 'root' })
export class ChatService {
  private readonly apiUrl = `${environment.apiUrl}/chats`;

  readonly chats = signal<ChatSummary[]>([]);

  constructor(private http: HttpClient) { }

  loadChats() {
    return this.http.get<ChatSummary[]>(this.apiUrl).pipe(
      tap(chats => this.chats.set(chats))
    );
  }

  getOrCreatePrivateChat(otherUserId: number) {
    return this.http.post<{ chatId: number }>(`${this.apiUrl}/private/${otherUserId}`, {});
  }

  getMessages(chatId: number, skip = 0, take = 30) {
    return this.http.get<Message[]>(`${this.apiUrl}/${chatId}/messages`, {
      params: { skip, take }
    });
  }

  sendMessage(chatId: number, content: string) {
    return this.http.post<Message>(`${this.apiUrl}/${chatId}/messages`, { content });
  }


  updateChatWithNewMessage(chatId: number, content: string, sentAt: string): void {
    this.chats.update(list => {
      const index = list.findIndex(c => c.chatId === chatId);
      if (index === -1) return list;

      const updated = { ...list[index], lastMessageContent: content, lastMessageAt: sentAt };
      const rest = list.filter(c => c.chatId !== chatId);
      return [updated, ...rest];
    });
  }
}
