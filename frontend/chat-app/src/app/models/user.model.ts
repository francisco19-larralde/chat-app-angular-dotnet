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
