import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
  computed,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { UserProfileService } from '../../core/services/user-profile.service';
import { AuthService } from '../../core/services/auth.service';
import { UserProfileDto } from '../../core/models/auth.model';

@Component({
  selector: 'app-member-detail',
  imports: [CommonModule, RouterLink],
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MemberDetailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly userProfileService = inject(UserProfileService);
  private readonly authService = inject(AuthService);

  readonly isLoading = signal(true);
  readonly error = signal<string | null>(null);
  readonly profile = signal<UserProfileDto | null>(null);

  readonly currentUserId = computed(() => this.authService.user()?.userId);
  readonly isAdmin = computed(() => this.authService.isAdmin());
  readonly isOwnProfile = computed(
    () => this.profile()?.userId === this.currentUserId()
  );
  readonly canViewPrivate = computed(
    () => this.isOwnProfile() || this.isAdmin()
  );
  readonly canEdit = computed(() => this.isOwnProfile() || this.isAdmin());

  constructor() {
    this.route.params.pipe(takeUntilDestroyed()).subscribe((params) => {
      const userId = params['userId'];
      if (userId) {
        this.loadProfile(userId);
      }
    });
  }

  private loadProfile(userId: string): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.userProfileService
      .getProfileById(userId)
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: (profile) => {
          this.profile.set(profile);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Failed to load profile:', err);
          this.error.set('Impossible de charger le profil');
          this.isLoading.set(false);
        },
      });
  }

  retry(): void {
    const userId = this.route.snapshot.params['userId'];
    if (userId) {
      this.loadProfile(userId);
    }
  }

  getInitials(fullName: string): string {
    return fullName
      .split(' ')
      .map((word) => word[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }

  formatDate(dateString: string): string {
    try {
      return new Date(dateString).toLocaleDateString('fr-CA', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
      });
    } catch {
      return dateString;
    }
  }
}
