import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, User } from '../../models/user.model';
import { SignalRService } from './signal-r.service';

interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

interface LoginRequest {
  email: string;
  password: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private readonly tokenKey = 'chatapp_token';
  private readonly userKey = 'chatapp_user';

  private signalRService = inject(SignalRService);

  private currentUserSignal = signal<User | null>(this.getUserFromStorage());

  readonly currentUser = this.currentUserSignal.asReadonly();
  readonly isLoggedIn = computed(() => this.currentUserSignal() !== null);

  constructor(private http: HttpClient, private router: Router) {
    const existingToken = this.getToken();
    if (existingToken && this.currentUserSignal()) {
      this.signalRService.start(existingToken);
    }
  }

  register(data: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, data).pipe(
      tap(response => this.handleAuthSuccess(response))
    );
  }


  login(data: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, data).pipe(
      tap(response => this.handleAuthSuccess(response))
    );
  }

  loginWithGoogle(idToken: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/google`, { idToken }).pipe(
      tap(response => this.handleAuthSuccess(response))
    );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.currentUserSignal.set(null);
    this.signalRService.stop();
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  private handleAuthSuccess(response: AuthResponse): void {
    const user: User = {
      userId: response.userId,
      username: response.username,
      email: response.email,
      profilePictureUrl: response.profilePictureUrl
    };

    localStorage.setItem(this.tokenKey, response.token);
    localStorage.setItem(this.userKey, JSON.stringify(user));
    this.currentUserSignal.set(user);
    this.signalRService.start(response.token);
  }

  updateCurrentUser(partialUser: Partial<User>): void {
    const current = this.currentUserSignal();
    if (!current) return;

    const updated: User = { ...current, ...partialUser };

    localStorage.setItem(this.userKey, JSON.stringify(updated));
    this.currentUserSignal.set(updated);
  }

  private getUserFromStorage(): User | null {
    const stored = localStorage.getItem(this.userKey);
    return stored ? JSON.parse(stored) : null;
  }

}
