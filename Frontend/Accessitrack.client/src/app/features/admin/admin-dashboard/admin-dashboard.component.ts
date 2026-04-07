import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../../core/services/auth.service';
import { ProjectService } from '../../../core/services/project.service';
import { Project } from '../../../core/models/project.model';

@Component({
  selector: 'app-admin-dashboard',
  imports: [RouterLink, DatePipe],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminDashboardComponent {
  private readonly authService = inject(AuthService);
  private readonly projectService = inject(ProjectService);

  readonly projects = signal<Project[]>([]);
  readonly isLoading = signal<boolean>(true);
  readonly error = signal<string | null>(null);

  readonly user = computed(() => this.authService.user());
  readonly projectCount = computed(() => this.projects().length);

  constructor() {
    this.loadProjects();
  }

  private loadProjects(): void {
    this.isLoading.set(true);
    this.projectService
      .getAll()
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: (projects) => {
          this.projects.set(projects);
          this.isLoading.set(false);
        },
        error: () => {
          this.error.set('Erreur lors du chargement des projets');
          this.isLoading.set(false);
        },
      });
  }

  logout(): void {
    this.authService.logout();
  }
}
