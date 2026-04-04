import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { LiveAnnouncer } from '@angular/cdk/a11y';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <a class="skip-link" href="#main-content">Aller au contenu principal</a>

    <div class="app-layout">

      <!-- SIDEBAR -->
      <nav class="sidebar" role="navigation" aria-label="Navigation principale">
        <div class="sidebar-logo">
          <h1>
            <span class="logo-icon" aria-hidden="true">♿</span>
            AccessiTrack
          </h1>
          <p>Audits WCAG 2.1/2.2</p>
        </div>

        <div class="sidebar-nav">
          <ul>
            <li>
              <a routerLink="/dashboard"
                 routerLinkActive="active-link"
                 [routerLinkActiveOptions]="{ exact: true }"
                 aria-label="Tableau de bord">
                <span class="nav-icon" aria-hidden="true">📊</span>
                Tableau de bord
              </a>
            </li>
            <li>
              <a routerLink="/projects"
                 routerLinkActive="active-link"
                 aria-label="Liste des projets">
                <span class="nav-icon" aria-hidden="true">📁</span>
                Projets
              </a>
            </li>
            <li>
              <a routerLink="/projects/new"
                 routerLinkActive="active-link"
                 aria-label="Nouveau projet">
                <span class="nav-icon" aria-hidden="true">➕</span>
                Nouveau projet
              </a>
            </li>
          </ul>
        </div>

        <div class="sidebar-footer">
          <p>Stagiaire CRTC — Portfolio 2026</p>
          <p>Philippe Charron</p>
        </div>
      </nav>

      <!-- CONTENU PRINCIPAL -->
      <main class="main-content" id="main-content" tabindex="-1" role="main">
        <router-outlet />
      </main>

    </div>
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
      .subscribe(() => {
        this.liveAnnouncer.announce(
          `Page chargée : ${document.title}`,
          'polite',
        );
        document.getElementById('main-content')?.focus();
      });
  }
}