import {
  ChangeDetectionStrategy,
  Component,
  inject,
} from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { LiveAnnouncer } from '@angular/cdk/a11y';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    .skip-link {
      position: absolute;
      top: -100%;
      left: 1rem;
      background: #000;
      color: #fff;
      padding: 0.75rem 1.5rem;
      border-radius: 0 0 4px 4px;
      z-index: 9999;
      font-weight: 600;
      text-decoration: none;
    }
    .skip-link:focus {
      top: 0;
    }
    nav a {
      margin-right: 1rem;
      padding: 0.5rem;
    }
    nav a.active-link {
      font-weight: bold;
      text-decoration: underline;
    }
  `],
  template: `
    <!-- WCAG 2.4.1 — Skip to main content -->
    <a class="skip-link" href="#main-content">
      Aller au contenu principal
    </a>

    <header role="banner">
      <nav role="navigation" aria-label="Navigation principale">
        <a routerLink="/dashboard"
           routerLinkActive="active-link"
           [routerLinkActiveOptions]="{ exact: true }"
           aria-label="Aller au tableau de bord">
          Tableau de bord
        </a>
        <a routerLink="/projects"
           routerLinkActive="active-link"
           aria-label="Aller à la liste des projets">
          Projets
        </a>
        <a routerLink="/projects/new"
           routerLinkActive="active-link"
           aria-label="Créer un nouveau projet">
          + Nouveau projet
        </a>
      </nav>
    </header>

    <!-- tabindex="-1" permet au focus d'être déplacé ici par JS -->
    <main id="main-content" tabindex="-1" role="main">
      <router-outlet />
    </main>
  `,
})
export class AppComponent {
  private readonly router = inject(Router);
  private readonly liveAnnouncer = inject(LiveAnnouncer);

  constructor() {
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntilDestroyed(),
      )
      .subscribe((event: NavigationEnd) => {
        // Annonce la navigation aux lecteurs d'écran — critique pour les SPA
        this.liveAnnouncer.announce(
          `Navigation vers : ${document.title}`,
          'polite',
        );
        // Remet le focus sur le contenu principal
        const main = document.getElementById('main-content');
        main?.focus();
      });
  }
}