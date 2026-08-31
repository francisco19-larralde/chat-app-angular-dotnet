import { Component, inject, signal, OnInit, OnDestroy, ElementRef, ViewChild, AfterViewChecked, effect, computed } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../../core/services/chat.service';
import { AuthService } from '../../../core/services/auth.service';
import { SignalRService } from '../../../core/services/signal-r.service';
import { Message } from '../../../models/chat.model';
import { GroupInfo } from '../../groups/group-info/group-info';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-chat-window',
  standalone: true,
  imports: [FormsModule, CommonModule, GroupInfo],
  templateUrl: './chat-window.html'
})
export class ChatWindow implements OnInit, OnDestroy, AfterViewChecked {
  private route = inject(ActivatedRoute);
  private chatService = inject(ChatService);
  private authService = inject(AuthService);
  private signalRService = inject(SignalRService);
  readonly serverUrl = environment.apiUrl.replace('/api', '');

  @ViewChild('messagesEnd') private messagesEnd!: ElementRef<HTMLDivElement>;

  currentUserId = this.authService.currentUser()?.userId;

  chatId = signal<number | null>(null);
  messages = signal<Message[]>([]);
  isLoading = signal(true);
  newMessageText = '';
  isSending = signal(false);
  showGroupInfo = signal(false);
  selectedFile = signal<File | null>(null);
  selectedFilePreviewUrl = signal<string | null>(null);

  private shouldScrollToBottom = false;
  private previousChatId: number | null = null;

  constructor() {
    effect(() => {
      const message = this.signalRService.lastMessage();
      if (message && message.chatId === this.chatId()) {
        this.addMessageIfNotExists(message);
        this.shouldScrollToBottom = true;
      }
    });
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe(async params => {
      const id = Number(params.get('chatId'));

      if (this.previousChatId !== null) {
        await this.signalRService.leaveChat(this.previousChatId);
      }

      this.chatId.set(id);
      this.previousChatId = id;

      await this.signalRService.joinChat(id);

      this.loadMessages(id);
    });
  }

  ngOnDestroy(): void {
    if (this.chatId() !== null) {
      this.signalRService.leaveChat(this.chatId()!);
    }
  }

  ngAfterViewChecked(): void {
    if (this.shouldScrollToBottom) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
  }

  private loadMessages(chatId: number): void {
    this.isLoading.set(true);
    this.chatService.getMessages(chatId).subscribe({
      next: (messages) => {
        this.messages.set(messages.reverse());
        this.isLoading.set(false);
        this.shouldScrollToBottom = true;
      }
    });
  }

  sendMessage(): void {
    const content = this.newMessageText.trim();
    const file = this.selectedFile();
    const chatId = this.chatId();

    if (!content && !file) return;
    if (!chatId) return;

    this.isSending.set(true);

    this.chatService.sendMessage(chatId, content || null, file ?? undefined).subscribe({
      next: (message) => {
        this.addMessageIfNotExists(message);
        this.chatService.updateChatWithNewMessage(
          chatId,
          message.content ?? (message.attachments.length > 0 ? '📎 Archivo adjunto' : ''),
          message.sentAt
        );
        this.newMessageText = '';
        this.clearSelectedFile();
        this.isSending.set(false);
        this.shouldScrollToBottom = true;
      },
      error: () => {
        this.isSending.set(false);
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const maxSize = 20 * 1024 * 1024;
    if (file.size > maxSize) {
      alert('El archivo no puede superar los 20 MB.');
      input.value = '';
      return;
    }

    this.selectedFile.set(file);


    if (file.type.startsWith('image/')) {
      const reader = new FileReader();
      reader.onload = () => this.selectedFilePreviewUrl.set(reader.result as string);
      reader.readAsDataURL(file);
    } else {
      this.selectedFilePreviewUrl.set(null);
    }

    input.value = '';
  }

  clearSelectedFile(): void {
    this.selectedFile.set(null);
    this.selectedFilePreviewUrl.set(null);
  }

  currentChatSummary = computed(() =>
    this.chatService.chats().find(c => c.chatId === this.chatId())
  );

  private addMessageIfNotExists(message: Message): void {
    const alreadyExists = this.messages().some(m => m.id === message.id);
    if (!alreadyExists) {
      this.messages.update(list => [...list, message]);
    }
  }

  isMine(message: Message): boolean {
    return message.senderId === this.currentUserId;
  }

  private scrollToBottom(): void {
    try {
      this.messagesEnd.nativeElement.scrollIntoView({ behavior: 'smooth' });
    } catch { }
  }
}
