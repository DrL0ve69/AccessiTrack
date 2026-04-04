import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { LiveAnnouncer } from '@angular/cdk/a11y';
import { take } from 'rxjs';
import { ViolationService } from '../../core/services/violation.service';
import { ViolationSeverity } from '../../core/models/violation.model';

@Component({
  selector: 'app-violation-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './violation-form.html',
  styleUrl: './violation-form.scss',
})
export class ViolationFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly violationService = inject(ViolationService);
  private readonly router = inject(Router);
  private readonly liveAnnouncer = inject(LiveAnnouncer);
  readonly route = inject(ActivatedRoute);

  readonly isSubmitting = signal(false);
  readonly serverError = signal<string | null>(null);

  // Mapping des IDs depuis l'URL (Projet :projectId / Audit :id)
  private readonly auditId = this.route.snapshot.paramMap.get('id') ?? '';
  private readonly projectId = this.route.snapshot.paramMap.get('projectId') ?? '';

  // Mapping pour l'Enum C# (Minor=0, Major=1, Critical=2)
  private readonly severityMap: Record<ViolationSeverity, number> = {
    'Minor': 0,
    'Major': 1,
    'Critical': 2
  };

  readonly wcagCriteria = [
    { value: '1.1.1', label: '1.1.1 — Contenu non textuel (Alt text)' },
    { value: '1.3.1', label: '1.3.1 — Information et relations (Structure)' },
    { value: '1.4.3', label: '1.4.3 — Contraste (minimum 4.5:1)' },
    { value: '2.1.1', label: '2.1.1 — Clavier (navigation complète)' },
    { value: '2.4.7', label: '2.4.7 — Visibilité du focus' },
    { value: '3.3.1', label: '3.3.1 — Identification des erreurs' },
    { value: '4.1.2', label: '4.1.2 — Nom, rôle, valeur (ARIA)' },
  ];

  readonly severityOptions: { value: ViolationSeverity; label: string; description: string }[] = [
    { value: 'Critical', label: 'Critique', description: "Bloque l'usage" },
    { value: 'Major', label: 'Majeur', description: "Usage difficile" },
    { value: 'Minor', label: 'Mineur', description: 'Gêne légère' },
  ];

  readonly form = this.fb.nonNullable.group({
    wcagCriterion: ['', Validators.required],
    wcagCriterionName: ['', Validators.required],
    htmlElement: ['', [Validators.required, Validators.maxLength(500)]],
    description: ['', [Validators.required, Validators.maxLength(2000)]],
    severity: ['' as ViolationSeverity, Validators.required],
  });

  isInvalid(control: AbstractControl): boolean {
    return control.invalid && (control.dirty || control.touched);
  }

  onCriterionChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    const found = this.wcagCriteria.find((c) => c.value === value);
    if (found) {
      const name = found.label.split(' — ')[1] ?? '';
      this.form.controls.wcagCriterionName.setValue(name);
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.liveAnnouncer.announce('Erreurs dans le formulaire', 'assertive');
      return;
    }

    this.isSubmitting.set(true);
    this.serverError.set(null);

    const raw = this.form.getRawValue();

    // WRAPPER "command" + conversion Severity en Number pour l'API
    const payload = {
      command: {
        auditId: this.auditId,
        wcagCriterion: raw.wcagCriterion,
        wcagCriterionName: raw.wcagCriterionName,
        htmlElement: raw.htmlElement,
        description: raw.description,
        severity: this.severityMap[raw.severity],
        isResolved: false
      }
    };

    this.violationService.create(payload)
      .pipe(take(1))
      .subscribe({
        next: () => {
          this.liveAnnouncer.announce('Violation créée', 'polite');
          this.router.navigate(['/projects', this.projectId, 'audits']);
        },
        error: (err) => {
          this.isSubmitting.set(false);
          // Affiche l'erreur de validation précise du backend
          const msg = err.error?.errors ? JSON.stringify(err.error.errors) : "Données invalides (400).";
          this.serverError.set(msg);
          console.error('Rejet API:', err.error);
        }
      });
  }
}
