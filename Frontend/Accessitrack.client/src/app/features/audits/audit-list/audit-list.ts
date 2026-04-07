import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { take } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuditService } from '../../../core/services/audit.service';
import { Audit, AutomaticAuditResult } from '../../../core/models/audit.model';

@Component({
  selector: 'app-audit-list',
  imports: [RouterLink, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './audit-list.html',
  styleUrls: ['./audit-list.scss'],
})
export class AuditListComponent {
  private readonly auditService = inject(AuditService);
  private readonly route = inject(ActivatedRoute);

  readonly isLoading = signal(true);
  readonly isStarting = signal(false);
  readonly isScanning = signal(false);
  readonly error = signal<string | null>(null);
  readonly audits = signal<Audit[]>([]);
  readonly projectId = signal<string>('');
  readonly scanResult = signal<AutomaticAuditResult | null>(null);

  readonly inProgressCount = computed(
    () => (this.audits() || []).filter((a) => a.status === 'InProgress').length,
  );
  readonly completedCount = computed(
    () => (this.audits() || []).filter((a) => a.status === 'Completed').length,
  );
  readonly totalCriticalUnresolved = computed(() =>
    this.audits().reduce((sum, a) => sum + this.criticalCount(a), 0),
  );

  constructor() {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.projectId.set(id);

    if (!id) {
      this.error.set("ID de projet manquant dans l'URL.");
      this.isLoading.set(false);
      return;
    }

    this.auditService
      .getByProject(id)
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: (data) => {
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
    if (!audit.violations) return 0;
    return audit.violations.filter((v) => v.severity === 'Critical' && !v.isResolved).length;
  }

  /**
   * Starts an automatic accessibility audit that scans the project's TargetUrl
   * and detects WCAG violations automatically.
   */
  startAutomaticAudit(): void {
    this.isScanning.set(true);
    this.scanResult.set(null);
    this.error.set(null);

    this.auditService
      .startAutomatic(this.projectId())
      .pipe(take(1))
      .subscribe({
        next: (result) => {
          this.isScanning.set(false);
          this.scanResult.set(result);

          // Refresh audit list to include the new audit
          this.auditService
            .getByProject(this.projectId())
            .pipe(take(1))
            .subscribe((data) => this.audits.set(data || []));
        },
        error: (err) => {
          this.isScanning.set(false);
          this.error.set(
            err?.error?.message ??
              "Impossible de démarrer l'audit automatique. Vérifiez que l'URL du projet est accessible.",
          );
        },
      });
  }

  startNewAudit(): void {
    this.isStarting.set(true);
    this.auditService
      .start({ projectId: this.projectId() })
      .pipe(take(1))
      .subscribe({
        next: () => {
          this.isStarting.set(false);
          this.auditService
            .getByProject(this.projectId())
            .pipe(take(1))
            .subscribe((data) => this.audits.set(data || []));
        },
        error: () => {
          this.error.set("Impossible de démarrer l'audit.");
          this.isStarting.set(false);
        },
      });
  }

  completeAudit(audit: Audit): void {
    this.auditService
      .complete(audit.id)
      .pipe(take(1))
      .subscribe({
        next: () => {
          this.audits.update((list) =>
            list.map((a) => (a.id === audit.id ? { ...a, status: 'Completed' } : a)),
          );
        },
        error: (err) => {
          this.error.set(
            err?.error?.message ??
              "Impossible de terminer l'audit. Des violations critiques sont peut-être encore ouvertes.",
          );
        },
      });
  }
}
