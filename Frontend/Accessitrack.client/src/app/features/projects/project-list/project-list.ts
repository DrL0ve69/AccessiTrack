import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProjectService } from '../../../core/services/project.service';
import { Project } from '../../../core/models/project.model';

@Component({
  selector: 'app-project-list',
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section aria-labelledby="projects-heading">
      <div class="page-header">
        <h1 id="projects-heading">Projets</h1>
        <a routerLink="/projects/new"
           role="button"
           aria-label="Créer un nouveau projet d'audit">
          + Nouveau projet
        </a>
      </div>

      <div aria-live="polite" aria-atomic="true" class="sr-only">
        @if (isLoading()) { Chargement des projets en cours... }
        @if (!isLoading()) { {{ activeProjects().length }} projets chargés. }
      </div>

      @if (isLoading()) {
        <div role="status" aria-label="Chargement des projets">
          <span aria-hidden="true">⏳</span> Chargement...
        </div>
      }

      @if (error()) {
        <div role="alert" aria-live="assertive">
          <p>{{ error() }}</p>
          <button type="button" (click)="reload()"
                  aria-label="Réessayer de charger les projets">
            Réessayer
          </button>
        </div>
      }

      @if (!isLoading() && !error()) {
        <ul aria-label="Liste des projets actifs">
          @for (project of activeProjects(); track project.id) {
            <li>
              <article [attr.aria-label]="'Projet ' + project.name">
                <header>
                  <h2>{{ project.name }}</h2>
                  <span [class.badge-active]="!project.isArchived"
                        [class.badge-archived]="project.isArchived"
                        aria-label="Statut : {{ project.isArchived ? 'Archivé' : 'Actif' }}">
                    {{ project.isArchived ? 'Archivé' : 'Actif' }}
                  </span>
                </header>

                <dl>
                  <dt>Client</dt>
                  <dd>{{ project.clientName }}</dd>
                  <dt>URL cible</dt>
                  <dd>
                    <a [href]="project.targetUrl"
                       target="_blank"
                       rel="noopener noreferrer"
                       [attr.aria-label]="'Visiter ' + project.targetUrl + ' (ouvre un nouvel onglet)'">
                      {{ project.targetUrl }}
                    </a>
                  </dd>
                  <dt>Audits</dt>
                  <dd>{{ project.totalAudits }}</dd>
                </dl>

                <footer>
                  <a [routerLink]="['/projects', project.id, 'audits']"
                     [attr.aria-label]="'Voir les audits du projet ' + project.name">
                    Voir les audits →
                  </a>
                  <button type="button"
                          (click)="archiveProject(project)"
                          [attr.aria-label]="'Archiver le projet ' + project.name">
                    Archiver
                  </button>
                </footer>
              </article>
            </li>
          } @empty {
            <li>
              <p>Aucun projet actif.</p>
              <a routerLink="/projects/new">Créer votre premier projet →</a>
            </li>
          }
        </ul>
      }
    </section>
  `,
})
export class ProjectListComponent {
  private readonly projectService = inject(ProjectService);

  readonly isLoading = signal(true);
  readonly error = signal<string | null>(null);
  readonly projects = signal<Project[]>([]);

  readonly activeProjects = computed(
    () => this.projects().filter(p => !p.isArchived)
  );

  constructor() {
    this.load();
  }

  private load(): void {
    this.isLoading.set(true);
    this.error.set(null);
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

  reload(): void {
    this.load();
  }

  archiveProject(project: Project): void {
    if (!confirm(`Archiver le projet "${project.name}" ?`)) return;

    this.projectService.archive(project.id)
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: () => {
          this.projects.update(list =>
            list.map(p => p.id === project.id ? { ...p, isArchived: true } : p)
          );
        },
        error: () => {
          this.error.set('Impossible d\'archiver ce projet.');
        },
      });
  }
}
