import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth.service';
import { UserProfileService } from '../../core/services/user-profile.service';
import { UserProfileDto } from '../../core/models/auth.model';

@Component({
  selector: 'app-user-profile',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user-profile.component.html',
  styleUrl: './user-profile.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserProfileComponent {
  private readonly authService = inject(AuthService);
  private readonly userProfileService = inject(UserProfileService);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);

  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly isUploadingAvatar = signal(false);
  readonly error = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);
  readonly profile = signal<UserProfileDto | null>(null);
  readonly previewUrl = signal<string | null>(null);

  readonly profileForm = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.minLength(2)]],
    phoneNumber: [''],
    bio: ['', [Validators.maxLength(500)]],
    preferredLanguage: ['fr-CA'],
    highContrastEnabled: [false],
    emailPrivate: [false],
    phonePrivate: [false],
    bioPrivate: [false],
  });

  constructor() {
    this.userProfileService
      .getCurrentProfile()
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: (profile) => {
          this.profile.set(profile);
          this.populateForm(profile);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Failed to load profile:', err);
          this.error.set('Impossible de charger le profil');
          this.isLoading.set(false);
        },
      });
  }

  private populateForm(profile: UserProfileDto): void {
    this.profileForm.patchValue({
      fullName: profile.fullName,
      phoneNumber: profile.phoneNumber || '',
      bio: profile.bio || '',
      preferredLanguage: profile.preferredLanguage,
      highContrastEnabled: profile.highContrastEnabled,
      emailPrivate: profile.emailPrivate,
      phonePrivate: profile.phonePrivate,
      bioPrivate: profile.bioPrivate,
    });
  }

  onAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) return;

    if (file.size > 5 * 1024 * 1024) {
      this.error.set('L\'image doit être inférieure à 5 Mo');
      return;
    }

    if (!file.type.startsWith('image/')) {
      this.error.set('Veuillez sélectionner une image valide');
      return;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
      this.previewUrl.set(e.target?.result as string);
    };
    reader.readAsDataURL(file);

    this.isUploadingAvatar.set(true);
    const userId = this.authService.user()?.userId;

    if (!userId) {
      this.error.set('Erreur: ID utilisateur manquant');
      this.isUploadingAvatar.set(false);
      return;
    }

    this.userProfileService
      .uploadAvatar(userId, file)
      .pipe()
      .subscribe({
        next: (response) => {
          this.profile.update((p) =>
            p ? { ...p, avatar: response.avatarUrl } : null
          );
          this.successMessage.set('Photo de profil mise à jour');
          this.isUploadingAvatar.set(false);
          setTimeout(() => this.successMessage.set(null), 3000);
        },
        error: (err) => {
          console.error('Avatar upload failed:', err);
          this.error.set('Impossible de mettre à jour la photo');
          this.previewUrl.set(null);
          this.isUploadingAvatar.set(false);
        },
      });
  }

  onSave(): void {
    if (this.profileForm.invalid) {
      this.error.set('Veuillez corriger les erreurs du formulaire');
      return;
    }

    this.isSaving.set(true);
    this.error.set(null);

    const userId = this.authService.user()?.userId;
    if (!userId) {
      this.error.set('Erreur: ID utilisateur manquant');
      this.isSaving.set(false);
      return;
    }

    this.userProfileService
      .updateProfile(userId, this.profileForm.getRawValue())
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: (updatedProfile) => {
          this.profile.set(updatedProfile);
          this.successMessage.set('Profil mis à jour avec succès');
          this.isSaving.set(false);
          setTimeout(() => this.successMessage.set(null), 3000);
        },
        error: (err) => {
          console.error('Profile update failed:', err);
          this.error.set(
            err.error?.message || 'Impossible de mettre à jour le profil'
          );
          this.isSaving.set(false);
        },
      });
  }

  onDeleteProfile(): void {
    const confirmed = confirm(
      'Êtes-vous sûr de vouloir supprimer votre profil ? Cette action ne peut pas être annulée.'
    );

    if (!confirmed) return;

    this.isSaving.set(true);
    const userId = this.authService.user()?.userId;

    if (!userId) {
      this.error.set('Erreur: ID utilisateur manquant');
      this.isSaving.set(false);
      return;
    }

    this.userProfileService
      .deleteProfile(userId)
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: () => {
          this.authService.logout();
          this.router.navigate(['/login']);
        },
        error: (err) => {
          console.error('Profile deletion failed:', err);
          this.error.set(
            err.error?.message || 'Impossible de supprimer le profil'
          );
          this.isSaving.set(false);
        },
      });
  }

  dismissError(): void {
    this.error.set(null);
  }

  getInitials(fullName: string): string {
    return fullName
      .split(' ')
      .map((word) => word[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }
}
