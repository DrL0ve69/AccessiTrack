import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
  computed,
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
  template: `
    <div class="profile-container">
      <div class="profile-content">
        <!-- Header -->
        <header class="profile-header">
          <h1>Mon Profil</h1>
          <p>Gérez vos informations personnelles</p>
        </header>

        <!-- Loading State -->
        @if (isLoading()) {
          <div class="loading" role="status" aria-live="polite">
            Chargement du profil...
          </div>
        } @else if (error()) {
          <div class="error-message" role="alert">
            {{ error() }}
            <button (click)="dismissError()" class="btn-dismiss">✕</button>
          </div>
        } @else {
          <form [formGroup]="profileForm" (ngSubmit)="onSave()" class="profile-form">
            <!-- Avatar Section -->
            <section class="form-section">
              <h2>Photo de profil</h2>

              <div class="avatar-container">
                @if (previewUrl()) {
                  <img
                    [src]="previewUrl()"
                    alt="Aperçu de la photo"
                    class="profile-avatar"
                  />
                } @else if (profile()?.avatar) {
                  <img
                    [src]="profile()!.avatar"
                    alt="Photo de profil"
                    class="profile-avatar"
                  />
                } @else {
                  <div class="profile-avatar placeholder">
                    {{ getInitials(profile()!.fullName) }}
                  </div>
                }

                <input
                  type="file"
                  #avatarInput
                  hidden
                  accept="image/*"
                  (change)="onAvatarSelected($event)"
                  aria-label="Choisir une photo de profil"
                />

                <button
                  type="button"
                  (click)="avatarInput.click()"
                  class="btn-upload"
                  [disabled]="isUploadingAvatar()"
                >
                  @if (isUploadingAvatar()) {
                    Envoi...
                  } @else {
                    Changer la photo
                  }
                </button>
              </div>
            </section>

            <!-- Personal Info Section -->
            <section class="form-section">
              <h2>Informations personnelles</h2>

              <div class="form-group">
                <label for="fullName">Nom complet</label>
                <input
                  id="fullName"
                  type="text"
                  formControlName="fullName"
                  class="input-field"
                  required
                />
                @if (profileForm.get('fullName')?.invalid && profileForm.get('fullName')?.touched) {
                  <span class="error-text">Le nom est requis</span>
                }
              </div>

              <div class="form-group">
                <label for="email">Email</label>
                <input
                  id="email"
                  type="email"
                  [value]="profile()!.email"
                  disabled
                  class="input-field disabled"
                />
                <span class="help-text">L'email ne peut pas être modifié</span>
              </div>

              <div class="form-group">
                <label for="phoneNumber">Téléphone</label>
                <input
                  id="phoneNumber"
                  type="tel"
                  formControlName="phoneNumber"
                  class="input-field"
                  placeholder="+1 (555) 123-4567"
                />
              </div>

              <div class="form-group">
                <label for="bio">Bio</label>
                <textarea
                  id="bio"
                  formControlName="bio"
                  class="input-field"
                  rows="4"
                  placeholder="Parlez-nous de vous..."
                  maxlength="500"
                ></textarea>
                <span class="help-text">
                  {{ profileForm.get('bio')?.value?.length || 0 }} / 500
                </span>
              </div>
            </section>

            <!-- Privacy Section -->
            <section class="form-section">
              <h2>Confidentialité</h2>

              <div class="privacy-group">
                <label class="checkbox-label">
                  <input
                    type="checkbox"
                    formControlName="emailPrivate"
                    class="checkbox-input"
                  />
                  <span>Masquer mon email aux autres utilisateurs</span>
                </label>
              </div>

              <div class="privacy-group">
                <label class="checkbox-label">
                  <input
                    type="checkbox"
                    formControlName="phonePrivate"
                    class="checkbox-input"
                  />
                  <span>Masquer mon téléphone aux autres utilisateurs</span>
                </label>
              </div>

              <div class="privacy-group">
                <label class="checkbox-label">
                  <input
                    type="checkbox"
                    formControlName="bioPrivate"
                    class="checkbox-input"
                  />
                  <span>Masquer ma bio aux autres utilisateurs</span>
                </label>
              </div>
            </section>

            <!-- Preferences Section -->
            <section class="form-section">
              <h2>Préférences</h2>

              <div class="form-group">
                <label for="language">Langue préférée</label>
                <select id="language" formControlName="preferredLanguage" class="input-field">
                  <option value="fr-CA">Français (Canada)</option>
                  <option value="en-US">English (US)</option>
                </select>
              </div>

              <div class="privacy-group">
                <label class="checkbox-label">
                  <input
                    type="checkbox"
                    formControlName="highContrastEnabled"
                    class="checkbox-input"
                  />
                  <span>Activer le mode contraste élevé</span>
                </label>
              </div>
            </section>

            <!-- Action Buttons -->
            <div class="form-actions">
              <button
                type="submit"
                class="btn btn-primary"
                [disabled]="isSaving() || profileForm.invalid"
              >
                @if (isSaving()) {
                  Sauvegarde en cours...
                } @else {
                  Sauvegarder les modifications
                }
              </button>

              <button
                type="button"
                (click)="onDeleteProfile()"
                class="btn btn-danger"
                [disabled]="isSaving()"
              >
                Supprimer le compte
              </button>
            </div>

            @if (successMessage()) {
              <div class="success-message" role="status">
                {{ successMessage() }}
              </div>
            }
          </form>
        }
      </div>
    </div>
  `,
  styles: [`
    .profile-container {
      max-width: 600px;
      margin: 0 auto;
    }

    .profile-content {
      background: white;
      border-radius: 8px;
      padding: 2rem;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);

      @media (max-width: 768px) {
        padding: 1rem;
      }
    }

    .profile-header {
      margin-bottom: 2rem;
      border-bottom: 2px solid #eee;
      padding-bottom: 1rem;

      h1 {
        font-size: 1.75rem;
        margin: 0 0 0.5rem 0;
      }

      p {
        margin: 0;
        color: #666;
      }
    }

    .loading,
    .error-message,
    .success-message {
      padding: 1rem;
      border-radius: 4px;
      margin-bottom: 1.5rem;
      display: flex;
      align-items: center;
      justify-content: space-between;
    }

    .loading {
      background: #e3f2fd;
      color: #1976d2;
    }

    .error-message {
      background: #fee;
      color: #c33;

      .btn-dismiss {
        background: transparent;
        border: none;
        color: inherit;
        font-size: 1.5rem;
        cursor: pointer;
        padding: 0;
      }
    }

    .success-message {
      background: #e8f5e9;
      color: #2e7d32;
    }

    .profile-form {
      display: flex;
      flex-direction: column;
      gap: 2rem;
    }

    .form-section {
      display: flex;
      flex-direction: column;
      gap: 1rem;

      h2 {
        font-size: 1.1rem;
        margin: 0;
        color: #333;
        border-bottom: 1px solid #eee;
        padding-bottom: 0.5rem;
      }
    }

    .avatar-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 1rem;

      .profile-avatar {
        width: 150px;
        height: 150px;
        border-radius: 50%;
        object-fit: cover;
        border: 4px solid #667eea;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);

        &.placeholder {
          display: flex;
          align-items: center;
          justify-content: center;
          font-size: 3rem;
          font-weight: bold;
          color: #999;
          background: #f0f0f0;
        }
      }

      .btn-upload {
        padding: 0.75rem 1.5rem;
        background: #667eea;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        transition: all 0.3s ease;

        &:hover:not(:disabled) {
          background: #5568d3;
        }

        &:disabled {
          opacity: 0.6;
          cursor: not-allowed;
        }
      }
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;

      label {
        font-weight: 600;
        color: #333;
      }

      input,
      select,
      textarea {
        padding: 0.75rem;
        border: 1px solid #ddd;
        border-radius: 4px;
        font-size: 1rem;
        font-family: inherit;

        &:focus {
          outline: none;
          border-color: #667eea;
          box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
        }

        &:disabled {
          background: #f5f5f5;
          color: #999;
          cursor: not-allowed;
        }

        &.disabled {
          background: #f5f5f5;
        }
      }

      textarea {
        resize: vertical;
      }

      .error-text {
        color: #c33;
        font-size: 0.875rem;
      }

      .help-text {
        color: #999;
        font-size: 0.875rem;
      }
    }

    .privacy-group {
      display: flex;
      align-items: center;
    }

    .checkbox-label {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      cursor: pointer;
      user-select: none;

      .checkbox-input {
        width: 20px;
        height: 20px;
        cursor: pointer;

        &:focus {
          outline: 2px solid #667eea;
          outline-offset: 2px;
        }
      }
    }

    .form-actions {
      display: flex;
      gap: 1rem;
      flex-wrap: wrap;
      margin-top: 2rem;
      padding-top: 2rem;
      border-top: 1px solid #eee;

      .btn {
        padding: 0.75rem 1.5rem;
        border: none;
        border-radius: 4px;
        font-weight: 500;
        cursor: pointer;
        transition: all 0.3s ease;
        flex: 1;
        min-width: 150px;

        &.btn-primary {
          background: #667eea;
          color: white;

          &:hover:not(:disabled) {
            background: #5568d3;
          }
        }

        &.btn-danger {
          background: #f44336;
          color: white;

          &:hover:not(:disabled) {
            background: #da190b;
          }
        }

        &:disabled {
          opacity: 0.6;
          cursor: not-allowed;
        }
      }
    }

    @media (max-width: 768px) {
      .form-actions {
        flex-direction: column;

        .btn {
          min-width: auto;
        }
      }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserProfileComponent {
  private readonly authService = inject(AuthService);
  private readonly userProfileService = inject(UserProfileService);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);

  // State signals
  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly isUploadingAvatar = signal(false);
  readonly error = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);
  readonly profile = signal<UserProfileDto | null>(null);
  readonly previewUrl = signal<string | null>(null);

  // Form
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
    // Load profile on component init
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

    // Validate file size (5MB)
    if (file.size > 5 * 1024 * 1024) {
      this.error.set('L\'image doit être inférieure à 5 Mo');
      return;
    }

    // Validate file type
    if (!file.type.startsWith('image/')) {
      this.error.set('Veuillez sélectionner une image valide');
      return;
    }

    // Show preview
    const reader = new FileReader();
    reader.onload = (e) => {
      this.previewUrl.set(e.target?.result as string);
    };
    reader.readAsDataURL(file);

    // Upload avatar
    this.isUploadingAvatar.set(true);
    const userId = this.authService.user()?.userId;

    if (!userId) {
      this.error.set('Erreur: ID utilisateur manquant');
      this.isUploadingAvatar.set(false);
      return;
    }

    this.userProfileService
      .uploadAvatar(userId, file)
      .pipe(takeUntilDestroyed())
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
