import { Component, inject, signal, OnInit, ElementRef, ViewChild, AfterViewChecked } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../../core/services/chat.service';
import { AuthService } from '../../../core/services/auth.service';
import { Message } from '../../../models/chat.model';
import { CommonModule, DatePipe } from '@angular/common';

@Component({
  selector: 'app-chat-window',
  standalone: true,
  imports: [FormsModule, DatePipe, CommonModule],
  templateUrl: './chat-window.html'
})
export class ChatWindow implements OnInit, AfterViewChecked {
  private route = inject(ActivatedRoute);
  private chatService = inject(ChatService);
  private authService = inject(AuthService);

  @ViewChild('messagesEnd') private messagesEnd!: ElementRef<HTMLDivElement>;

  currentUserId = this.authService.currentUser()?.userId;

  chatId = signal<number | null>(null);
  messages = signal<Message[]>([]);
  isLoading = signal(true);
  newMessageText = '';
  isSending = signal(false);

  private shouldScrollToBottom = false;

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = Number(params.get('chatId'));
      this.chatId.set(id);
      this.loadMessages(id);
    });
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
    const chatId = this.chatId();
    if (!content || !chatId) return;

    this.isSending.set(true);

    this.chatService.sendMessage(chatId, content).subscribe({
      next: (message) => {
        this.messages.update(list => [...list, message]);
        this.chatService.updateChatWithNewMessage(chatId, message.content ?? '', message.sentAt);
        this.newMessageText = '';
        this.isSending.set(false);
        this.shouldScrollToBottom = true;
      },
      error: () => {
        this.isSending.set(false);
      }
    });
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
