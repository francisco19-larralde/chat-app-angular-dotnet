export interface GroupMember {
  userId: number;
  username: string;
  profilePictureUrl?: string;
  isOnline: boolean;
  role: 'Member' | 'Admin';
}

export interface GroupDetails {
  chatId: number;
  name: string;
  groupPictureUrl?: string;
  members: GroupMember[];
  currentUserIsAdmin: boolean;
}
