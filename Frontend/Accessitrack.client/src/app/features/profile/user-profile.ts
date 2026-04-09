import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-user-profile',
  imports: [ReactiveFormsModule],
  templateUrl: 'user-profile.html',
  styleUrl: 'user-profile.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserProfileComponent {
  private readonly authService = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  protected readonly currentUser = this.authService.user;

  // Données réactives issues du service d'authentification
  protected readonly userProfile = this.authService.profile;
  protected readonly userInitial = computed(() => 
    this.userProfile()?.fullName?.charAt(0).toUpperCase() || '?'
  );

  // Formulaire réactif pour les préférences
  protected readonly profileForm = this.fb.nonNullable.group({
    highContrast: [this.userProfile()?.highContrastEnabled ?? false],
    language: [this.userProfile()?.preferredLanguage ?? 'fr-CA']
  });

  savePreferences(): void {
    // Ici, tu appellerais un service pour sauvegarder en DB
    console.log('Préférences sauvegardées :', this.profileForm.value);
  }
}