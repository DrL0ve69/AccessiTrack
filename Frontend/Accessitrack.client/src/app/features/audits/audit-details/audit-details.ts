import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuditService } from '../../../core/services/audit.service';
import { Audit } from '../../../core/models/audit.model';
import { Violation, ViolationSeverity } from '../../../core/models/violation.model';

@Component({
  selector: 'app-audit-details',
  imports: [CommonModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './audit-details.html',
  styleUrls: ['./audit-details.scss'],
})
export class AuditDetailsComponent {
  private readonly auditService = inject(AuditService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly isLoading = signal(true);
  readonly error = signal<string | null>(null);
  readonly audit = signal<Audit | null>(null);
  readonly projectId = signal<string>('');
  readonly auditId = signal<string>('');

  readonly violations = computed(() => this.audit()?.violations ?? []);

  readonly violationsBySeverity = computed(() => {
    const viols = this.violations();
    return {
      critical: viols.filter((v) => v.severity === 'Critical'),
      major: viols.filter((v) => v.severity === 'Major'),
      minor: viols.filter((v) => v.severity === 'Minor'),
    };
  });

  readonly resolvedCount = computed(() =>
    this.violations().filter((v) => v.isResolved).length,
  );

  readonly unresolvedCount = computed(() =>
    this.violations().filter((v) => !v.isResolved).length,
  );

  constructor() {
    const projectId = this.route.snapshot.paramMap.get('projectId') ?? '';
    const auditId = this.route.snapshot.paramMap.get('auditId') ?? '';

    this.projectId.set(projectId);
    this.auditId.set(auditId);

    if (!projectId || !auditId) {
      this.error.set('IDs manquants dans l\'URL.');
      this.isLoading.set(false);
      return;
    }

    this.loadAuditDetails(auditId);
  }

  private loadAuditDetails(auditId: string): void {
    this.auditService
      .getById(auditId)
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: (audit) => {
          this.audit.set(audit);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Erreur lors du chargement de l\'audit:', err);
          this.error.set('Impossible de charger les détails de l\'audit.');
          this.isLoading.set(false);
        },
      });
  }

  getSeverityClass(severity: ViolationSeverity): string {
    return `severity-${severity.toLowerCase()}`;
  }

  goBack(): void {
    this.router.navigate(['/projects', this.projectId(), 'audits']);
  }

  getSeverityLabel(severity: ViolationSeverity): string {
    const labels: Record<ViolationSeverity, string> = {
      Critical: 'Critique',
      Major: 'Majeure',
      Minor: 'Mineure',
    };
    return labels[severity];
  }
}
