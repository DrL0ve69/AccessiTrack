import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProjectService } from '../../core/services/project.service';
import { AuthService } from '../../core/services/auth.service';
import { Project } from '../../core/models/project.model';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'dashboard.html',
  styleUrl: 'dashboard.scss',
})
export class DashboardComponent {
  private readonly projectService = inject(ProjectService);
  private readonly authService = inject(AuthService);

  readonly isLoading = signal(true);
  readonly error = signal<string | null>(null);
  readonly projects = signal<Project[]>([]);
  readonly user = computed(() => this.authService.user());
  readonly isAdmin = computed(() => this.authService.isAdmin());

  readonly activeProjects = computed(() =>
    this.projects().filter(p => !p.isArchived)
  );
  readonly archivedProjects = computed(() =>
    this.projects().filter(p => p.isArchived)
  );
  readonly totalAudits = computed(() =>
    this.projects().reduce((s, p) => s + p.totalAudits, 0)
  );
  readonly recentProjects = computed(() =>
    this.activeProjects().slice(0, 5)
  );

  constructor() {
    this.projectService
      .getAll()
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: (data) => {
          this.projects.set(data);
          this.isLoading.set(false);
        },
        error: () => {
          this.error.set('Impossible de charger les projets.');
          this.isLoading.set(false);
        },
      });
  }

  logout(): void {
    this.authService.logout();
  }
}
