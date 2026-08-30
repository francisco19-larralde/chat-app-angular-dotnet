export interface UserSearchResult {
  id: number;
  username: string;
  profilePictureUrl?: string;
  friendshipStatus: 'None' | 'Pending' | 'Accepted';
}

export interface FriendRequest {
  friendshipId: number;
  requesterId: number;
  requesterUsername: string;
  requesterProfilePictureUrl?: string;
  createdAt: string;
}

export interface Friend {
  userId: number;
  username: string;
  profilePictureUrl?: string;
  isOnline: boolean;
  lastSeen?: string;
}
