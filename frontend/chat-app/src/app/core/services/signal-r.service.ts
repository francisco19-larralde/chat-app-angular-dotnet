import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { Message } from '../../models/chat.model';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;


  private startPromise: Promise<void> | null = null;


  private joinedChatIds = new Set<number>();

  readonly lastMessage = signal<Message | null>(null);
  readonly onlineUserIds = signal<Set<number>>(new Set());
  readonly connectionState = signal<'disconnected' | 'connecting' | 'connected'>('disconnected');
  readonly chatListUpdate = signal<Message | null>(null);

  start(token: string): Promise<void> {
    if (this.startPromise) {
      return this.startPromise;
    }

    const hubUrl = `${environment.apiUrl.replace('/api', '')}/hubs/chat`;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .build();

    this.registerHandlers();

    this.connectionState.set('connecting');

    this.startPromise = this.hubConnection.start()
      .then(() => {
        this.connectionState.set('connected');
      })
      .catch(err => {
        console.error('Error al conectar con SignalR:', err);
        this.connectionState.set('disconnected');
        this.startPromise = null;
        throw err;
      });

    return this.startPromise;
  }

  stop(): void {
    this.hubConnection?.stop();
    this.hubConnection = null;
    this.startPromise = null;
    this.joinedChatIds.clear();
    this.connectionState.set('disconnected');
  }


  async joinChat(chatId: number): Promise<void> {
    try {
      if (this.startPromise) {
        await this.startPromise;
      }
      await this.hubConnection?.invoke('JoinChat', chatId);
      this.joinedChatIds.add(chatId);
    } catch (err) {
      console.error('Error al unirse al chat:', err);
    }
  }

  async leaveChat(chatId: number): Promise<void> {
    try {
      await this.hubConnection?.invoke('LeaveChat', chatId);
      this.joinedChatIds.delete(chatId);
    } catch (err) {
      console.error('Error al salir del chat:', err);
    }
  }

  private registerHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('NewMessage', (message: Message) => {
      this.lastMessage.set(message);
    });

    this.hubConnection.on('UserOnline', (userId: number) => {
      this.onlineUserIds.update(set => new Set(set).add(userId));
    });

    this.hubConnection.on('UserOffline', (userId: number) => {
      this.onlineUserIds.update(set => {
        const updated = new Set(set);
        updated.delete(userId);
        return updated;
      });
    });

    this.hubConnection.on('ChatUpdated', (message: Message) => {
      this.chatListUpdate.set(message);
    });

    this.hubConnection.onreconnected(() => {
      this.connectionState.set('connected');
      this.joinedChatIds.forEach(chatId => {
        this.hubConnection?.invoke('JoinChat', chatId).catch(err =>
          console.error('Error al re-unirse al chat tras reconexión:', err)
        );
      });
    });

    this.hubConnection.onreconnecting(() => {
      this.connectionState.set('connecting');
    });

    this.hubConnection.onclose(() => {
      this.connectionState.set('disconnected');
      this.startPromise = null;
    });
  }
}
