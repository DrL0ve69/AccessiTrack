import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { LiveAnnouncer } from '@angular/cdk/a11y';
import { inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <!-- 
      skip-link : permet aux utilisateurs clavier/lecteurs d'écran
      de sauter directement au contenu principal (WCAG 2.4.1)
    -->
    <a 
      class="skip-link" 
      href="#main-content"
      aria-label="Aller au contenu principal">
      Aller au contenu principal
    </a>

    <header role="banner">
      <nav role="navigation" aria-label="Navigation principale">
        <!-- Navigation ici -->
      </nav>
    </header>

    <main id="main-content" role="main" tabindex="-1">
      <router-outlet />
    </main>
  `,
  styles: [`
    .skip-link {
      position: absolute;
      top: -100%;
      left: 0;
      background: #000;
      color: #fff;
      padding: 8px 16px;
      z-index: 9999;
      
      /* Visible uniquement au focus clavier — WCAG 2.4.7 */
      &:focus {
        top: 0;
      }
    }
  `]
})
export class AppComponent {
  // ✅ inject() au lieu du constructeur
  private readonly router = inject(Router);
  private readonly liveAnnouncer = inject(LiveAnnouncer);

  constructor() {
    // ✅ takeUntilDestroyed évite les fuites mémoire
    // Se désabonne automatiquement quand le composant est détruit
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntilDestroyed()
      )
      .subscribe((event: NavigationEnd) => {
        // ✅ Annonce les changements de page aux lecteurs d'écran
        // Critique pour les SPA — sans ça, NVDA/JAWS ne détectent
        // pas le changement de page (pas de rechargement complet)
        this.liveAnnouncer.announce(
          `Page chargée : ${this.getPageTitle(event.url)}`,
          'polite'  // 'polite' = attend que l'utilisateur ait fini de lire
        );

        // ✅ Ramène le focus sur le contenu principal après navigation
        // Sans ça, le focus reste sur le lien cliqué (mauvaise UX clavier)
        const mainContent = document.getElementById('main-content');
        mainContent?.focus();
      });
  }

  private getPageTitle(url: string): string {
    const routes: Record<string, string> = {
      '/': 'Tableau de bord',
      '/projects': 'Liste des projets',
      '/audits': 'Audits',
    };
    return routes[url] ?? 'Nouvelle page';
  }
}