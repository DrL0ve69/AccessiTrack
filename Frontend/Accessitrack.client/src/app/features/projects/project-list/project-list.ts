// project-list.component.ts
import { 
  ChangeDetectionStrategy, 
  Component, 
  computed, 
  inject,
  signal
} from '@angular/core';
import { ProjectService } from './project-list.service';
//import { NgOptimizedImage } from '@angular/common';

interface Project {
  id: string;
  name: string;
  targetUrl: string;
  clientName: string;
  totalAudits: number;
  isArchived: boolean;
}

@Component({
  selector: 'app-project-list',
  imports: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section aria-labelledby="projects-heading">
      
      <h1 id="projects-heading">Projets en cours</h1>

      <!-- ✅ aria-live annonce les changements sans rechargement -->
      <div aria-live="polite" aria-atomic="true" class="sr-only">
        @if (isLoading()) {
          Chargement des projets en cours...
        }
        @if (!isLoading()) {
          {{ activeProjects().length }} projets chargés.
        }
      </div>

      <!-- ✅ @if au lieu de *ngIf -->
      @if (isLoading()) {
        <div role="status" aria-label="Chargement...">
          <!-- Spinner -->
        </div>
      }

      @if (errorMessage()) {
        <!-- ✅ role="alert" annonce immédiatement l'erreur -->
        <div role="alert" aria-live="assertive">
          {{ errorMessage() }}
        </div>
      }

      <!-- ✅ @for au lieu de *ngFor — with track obligatoire -->
      <ul aria-label="Liste des projets">
        @for (project of activeProjects(); track project.id) {
          <li>
            <article [attr.aria-label]="'Projet : ' + project.name">
              <h2>{{ project.name }}</h2>
              <p>Client : {{ project.clientName }}</p>
              <p>
                <a 
                  [href]="project.targetUrl"
                  [attr.aria-label]="'Visiter le site de ' + project.name"
                  target="_blank"
                  rel="noopener noreferrer">
                  {{ project.targetUrl }}
                </a>
              </p>
              <button 
                type="button"
                [attr.aria-label]="'Voir les audits de ' + project.name">
                {{ project.totalAudits }} audit(s)
              </button>
            </article>
          </li>
        } @empty {
          <li>
            <p>Aucun projet trouvé. Créez votre premier projet.</p>
          </li>
        }
      </ul>

    </section>
  `
})
export class ProjectListComponent {
  private readonly projectService = inject(ProjectService);

  // ✅ Signals pour l'état local
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly projects = signal<Project[]>([]);

  // ✅ computed() pour l'état dérivé — recalculé automatiquement
  readonly activeProjects = computed(() =>
    this.projects().filter(p => !p.isArchived)
  );

  readonly archivedCount = computed(() =>
    this.projects().filter(p => p.isArchived).length
  );
}
