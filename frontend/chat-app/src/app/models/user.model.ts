export interface User {
  userId: number;
  username: string;
  email: string;
  profilePictureUrl?: string;
}

export interface AuthResponse {
  token: string;
  userId: number;
  username: string;
  email: string;
  profilePictureUrl?: string;
}

export interface UserProfile {
  id: number;
  username: string;
  email: string;
  profilePictureUrl?: string;
  coverPictureUrl?: string;
  isOnline: boolean;
  lastSeen?: string;
}

export interface PublicUserProfile {
  id: number;
  username: string;
  profilePictureUrl?: string;
  coverPictureUrl?: string;
  isOnline: boolean;
  lastSeen?: string;
}

