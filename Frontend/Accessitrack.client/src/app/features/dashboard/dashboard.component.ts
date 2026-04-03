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
import { Project } from '../../core/models/project.model';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section aria-labelledby="dashboard-heading">
      <h1 id="dashboard-heading">Tableau de bord</h1>

      <!-- Annonce dynamique pour lecteurs d'écran -->
      <div aria-live="polite" aria-atomic="true" class="sr-only">
        @if (isLoading()) { Chargement des statistiques... }
        @if (!isLoading() && !error()) {
          {{ totalProjects() }} projets actifs chargés.
        }
      </div>

      @if (error()) {
        <div role="alert" aria-live="assertive">
          <p>{{ error() }}</p>
        </div>
      }

      @if (!isLoading()) {
        <div class="stats-grid" role="list" aria-label="Statistiques du tableau de bord">

          <article role="listitem" aria-label="Projets actifs">
            <h2>Projets actifs</h2>
            <p class="stat-number" aria-label="{{ totalProjects() }} projets actifs">
              {{ totalProjects() }}
            </p>
            <a routerLink="/projects" aria-label="Voir tous les projets actifs">
              Voir tous →
            </a>
          </article>

          <article role="listitem" aria-label="Projets archivés">
            <h2>Archivés</h2>
            <p class="stat-number">{{ archivedProjects() }}</p>
          </article>

          <article role="listitem" aria-label="Total des audits">
            <h2>Total audits</h2>
            <p class="stat-number">{{ totalAudits() }}</p>
          </article>

        </div>

        <!-- Projets récents -->
        <section aria-labelledby="recent-heading">
          <h2 id="recent-heading">Projets récents</h2>
          <ul aria-label="Liste des 3 projets les plus récents">
            @for (project of recentProjects(); track project.id) {
              <li>
                <a
                  [routerLink]="['/projects', project.id, 'audits']"
                  [attr.aria-label]="'Voir les audits de ' + project.name">
                  {{ project.name }} — {{ project.clientName }}
                </a>
              </li>
            } @empty {
              <li>
                <p>Aucun projet. 
                  <a routerLink="/projects/new">Créez votre premier projet</a>.
                </p>
              </li>
            }
          </ul>
        </section>
      }
    </section>
  `,
})
export class DashboardComponent {
  private readonly projectService = inject(ProjectService);

  readonly isLoading = signal(true);
  readonly error = signal<string | null>(null);
  readonly projects = signal<Project[]>([]);

  // ✅ computed() — recalculé automatiquement quand projects change
  readonly totalProjects = computed(
    () => this.projects().filter(p => !p.isArchived).length
  );
  readonly archivedProjects = computed(
    () => this.projects().filter(p => p.isArchived).length
  );
  readonly totalAudits = computed(
    () => this.projects().reduce((sum, p) => sum + p.totalAudits, 0)
  );
  readonly recentProjects = computed(
    () => this.projects().filter(p => !p.isArchived).slice(0, 3)
  );

  constructor() {
    this.projectService.getAll()
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: data => {
          this.projects.set(data);
          this.isLoading.set(false);
        },
        error: () => {
          this.error.set('Impossible de charger les projets.');
          this.isLoading.set(false);
        },
      });
  }
}