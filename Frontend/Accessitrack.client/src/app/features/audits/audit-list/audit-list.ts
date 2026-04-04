import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { AuditService } from '../../../core/services/audit.service';
import { Audit } from '../../../core/models/audit.model';

@Component({
  selector: 'app-audit-list',
  standalone: true,
  imports: [RouterLink, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section aria-labelledby="audits-heading">
      <div class="page-header">
        <h1 id="audits-heading">Audits du projet</h1>
        <button type="button"
                (click)="startNewAudit()"
                [attr.aria-disabled]="isStarting()"
                aria-label="Démarrer un nouvel audit pour ce projet">
          @if (isStarting()) { Démarrage... } @else { + Nouvel audit }
        </button>
      </div>

      <div aria-live="polite" aria-atomic="true" class="sr-only">
        @if (isLoading()) { Chargement des audits... }
        @if (!isLoading()) { {{ audits()?.length ?? 0 }} audits chargés. }
      </div>

      @if (error()) {
        <div role="alert" aria-live="assertive" class="error-banner">{{ error() }}</div>
      }

      @if (!isLoading()) {
        <div role="region" aria-label="Statistiques des audits">
          <p>En cours : <strong>{{ inProgressCount() }}</strong></p>
          <p>Complétés : <strong>{{ completedCount() }}</strong></p>
        </div>

        <ul aria-label="Liste des audits">
          @for (audit of audits(); track audit.id) {
            <li>
              <article [attr.aria-label]="'Audit du ' + audit.startedAt">
                <header>
                  <h2>Audit — {{ audit.status }}</h2>
                  <time [dateTime]="audit.startedAt">
                    Démarré le {{ audit.startedAt | date:'dd/MM/yyyy' }}
                  </time>
                </header>

                <p>
                  Violations :
                  <strong>{{ audit.violations?.length ?? 0 }}</strong>
                  dont
                  <strong class="critical"
                          [attr.aria-label]="criticalCount(audit) + ' violations critiques'">
                    {{ criticalCount(audit) }} critiques
                  </strong>
                </p>

                <footer>
                  <a [routerLink]="['/projects', projectId(), 'audits', audit.id, 'violations', 'new']"
                     [attr.aria-label]="'Ajouter une violation à cet audit'">
                    + Violation
                  </a>
                  @if (audit.status === 'InProgress') {
                    <button type="button"
                            (click)="completeAudit(audit)"
                            [attr.aria-label]="'Terminer laudit du ' + audit.startedAt">
                      Terminer l'audit
                    </button>
                  }
                </footer>
              </article>
            </li>
          } @empty {
            <li>
              <p>Aucun audit pour ce projet.</p>
              <p>Cliquez sur "+ Nouvel audit" pour commencer.</p>
            </li>
          }
        </ul>
      }
    </section>
  `,
  styles: [`
    .error-banner { color: #d32f2f; padding: 1rem; border: 1px solid currentColor; margin: 1rem 0; }
    .critical { color: #c62828; }
  `]
})
export class AuditListComponent {
  private readonly auditService = inject(AuditService);
  private readonly route = inject(ActivatedRoute);

  readonly isLoading = signal(true);
  readonly isStarting = signal(false);
  readonly error = signal<string | null>(null);
  readonly audits = signal<Audit[]>([]);
  readonly projectId = signal<string>('');

  // MODIFICATION : Ajout de vérifications de sécurité dans les computed
  readonly inProgressCount = computed(
    () => (this.audits() || []).filter(a => a.status === 'InProgress').length
  );
  readonly completedCount = computed(
    () => (this.audits() || []).filter(a => a.status === 'Completed').length
  );

  constructor() {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.projectId.set(id);

    if (!id) {
      this.error.set("ID de projet manquant dans l'URL.");
      this.isLoading.set(false);
      return;
    }

    this.auditService.getByProject(id)
      .subscribe({
        next: data => {
          this.audits.set(data || []);
          this.isLoading.set(false);
        },
        error: () => {
          this.error.set('Impossible de charger les audits.');
          this.isLoading.set(false);
        },
      });
  }

  criticalCount(audit: Audit): number {
    // MODIFICATION : Sécurisation ici aussi pour éviter le crash si violations est undefined
    if (!audit.violations) return 0;
    return audit.violations.filter(
      v => v.severity === 'Critical' && !v.isResolved
    ).length;
  }

  startNewAudit(): void {
    this.isStarting.set(true);
    this.auditService.start({ projectId: this.projectId() })
      .subscribe({
        next: () => {
          this.isStarting.set(false);
          this.auditService.getByProject(this.projectId())
            .subscribe(data => this.audits.set(data || []));
        },
        error: () => {
          this.error.set('Impossible de démarrer l\'audit.');
          this.isStarting.set(false);
        },
      });
  }

  completeAudit(audit: Audit): void {
    this.auditService.complete(audit.id)
      .subscribe({
        next: () => {
          this.audits.update(list =>
            list.map(a => a.id === audit.id ? { ...a, status: 'Completed' } : a)
          );
        },
        error: (err) => {
          this.error.set(
            err?.error?.message ??
            'Impossible de terminer l\'audit. Des violations critiques sont peut-être encore ouvertes.'
          );
        },
      });
  }
}