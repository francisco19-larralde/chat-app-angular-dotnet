import { Component, inject, signal, AfterViewInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';


declare const google: any;

@Component({
  imports: [ReactiveFormsModule, RouterLink],
  selector: 'app-login',
  styleUrl: './login.css',
  templateUrl: './login.html',
})
export class Login implements AfterViewInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  errorMessage = signal<string | null>(null);
  isLoading = signal(false);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });


  onSubmit(): void {

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.authService.login({
      email: this.form.value.email!,
      password: this.form.value.password!
    }).subscribe({
      next: () => this.router.navigate(['/']),
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.message ?? 'Error al iniciar sesión. Intentá de nuevo.');
      }
    });
  }

  private googleInitialized = false;

  ngAfterViewInit(): void {
    if (this.googleInitialized) return;
    this.googleInitialized = true;

    google.accounts.id.initialize({
      client_id: '705699064739-ou3cdhuk9btqp7e315bfsl8j5vn9g0le.apps.googleusercontent.com',
      callback: (response: any) => this.handleGoogleResponse(response)
    });

    google.accounts.id.renderButton(
      document.getElementById('googleSignInButton'),
      { theme: 'outline', size: 'large', width: 300 }
    );
  }

  private handleGoogleResponse(response: any): void {
    this.isLoading.set(true);
    this.authService.loginWithGoogle(response.credential).subscribe({
      next: () => this.router.navigate(['/']),
      error: () => {
        this.isLoading.set(false);
        this.errorMessage.set('No se pudo iniciar sesión con Google.');
      }
    });
  }


}
