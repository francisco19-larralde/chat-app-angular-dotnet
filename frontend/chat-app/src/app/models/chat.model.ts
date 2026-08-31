export interface ChatSummary {
  chatId: number;
  isGroup: boolean;
  displayName: string;
  displayPictureUrl?: string;
  lastMessageContent?: string;
  lastMessageAt?: string;
  isOtherUserOnline: boolean;
}

export interface Attachment {
  id: number;
  fileUrl: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
}

export interface Message {
  id: number;
  chatId: number;
  senderId: number;
  senderUsername: string;
  senderProfilePictureUrl?: string;
  otherUserId?: number;
  content?: string;
  sentAt: string;
  isEdited: boolean;
  attachments: Attachment[];
}
