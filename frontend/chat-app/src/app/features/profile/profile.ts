import { Component, inject, OnInit, signal } from '@angular/core';
import { Validators, FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../core/services/auth.service';
import { UserService } from '../../core/services/user.service';
import { UserProfile } from '../../models/user.model';

@Component({
  imports: [ReactiveFormsModule],
  selector: 'app-profile',
  styleUrl: './profile.css',
  templateUrl: './profile.html',
})
export class Profile implements OnInit {
  private fb = inject(FormBuilder);
  private userService = inject(UserService);
  private authService = inject(AuthService);

  profile = signal<UserProfile | null>(null);
  isSaving = signal(false);
  isUploadingProfilePic = signal(false);
  isUploadingCover = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);


  readonly serverUrl = environment.apiUrl.replace('/api', '');

  form = this.fb.group({
    username: ['', [Validators.required, Validators.minLength(3)]]
  });


  ngOnInit(): void {
    this.userService.getMyProfile().subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.form.patchValue({ username: profile.username });
      }
    });
  }


  getImageUrl(relativeUrl?: string): string | null {
    return relativeUrl ? `${this.serverUrl}${relativeUrl}` : null;
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.userService.updateProfile(this.form.value.username!).subscribe({
      next: (updated) => {
        this.profile.set(updated);
        this.authService.updateCurrentUser({ username: updated.username });
        this.isSaving.set(false);
        this.successMessage.set('Perfil actualizado correctamente.');
      },
      error: (err) => {
        this.isSaving.set(false);
        this.errorMessage.set(err.error?.message ?? 'Error al actualizar el perfil.');
      }
    });
  }

  onProfilePictureSelected(event: Event): void {
    const file = this.getSelectedFile(event);
    if (!file) return;

    this.isUploadingProfilePic.set(true);
    this.userService.uploadProfilePicture(file).subscribe({
      next: ({ url }) => {
        this.isUploadingProfilePic.set(false);
        this.profile.update(p => p ? { ...p, profilePictureUrl: url } : p);
        this.authService.updateCurrentUser({ profilePictureUrl: url });
      },
      error: (err) => {
        this.isUploadingProfilePic.set(false);
        this.errorMessage.set(err.error?.message ?? 'Error al subir la imagen.');
      }
    });
  }

  onCoverPictureSelected(event: Event): void {
    const file = this.getSelectedFile(event);
    if (!file) return;

    this.isUploadingCover.set(true);
    this.userService.uploadCoverPicture(file).subscribe({
      next: ({ url }) => {
        this.isUploadingCover.set(false);
        this.profile.update(p => p ? { ...p, coverPictureUrl: url } : p);
      },
      error: (err) => {
        this.isUploadingCover.set(false);
        this.errorMessage.set(err.error?.message ?? 'Error al subir la imagen.');
      }
    });
  }

  private getSelectedFile(event: Event): File | null {
    const input = event.target as HTMLInputElement;
    return input.files && input.files.length > 0 ? input.files[0] : null;
  }
}
