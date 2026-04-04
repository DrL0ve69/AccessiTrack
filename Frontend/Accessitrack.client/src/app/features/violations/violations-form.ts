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

  private readonly auditId = this.route.snapshot.paramMap.get('id') ?? '';
  private readonly projectId = this.route.snapshot.paramMap.get('projectId') ?? '';

  private readonly severityMap: Record<ViolationSeverity, number> = {
    'Minor': 0,
    'Major': 1,
    'Critical': 2
  };

  readonly wcagCriteria = [
    { value: '1.1.1', label: '1.1.1 — Contenu non textuel' },
    { value: '1.3.1', label: '1.3.1 — Information et relations' },
    { value: '1.4.3', label: '1.4.3 — Contraste (minimum)' },
    { value: '2.1.1', label: '2.1.1 — Clavier' },
    { value: '4.1.2', label: '4.1.2 — Nom, rôle, valeur' },
  ];

  readonly severityOptions: { value: ViolationSeverity; label: string }[] = [
    { value: 'Critical', label: 'Critique' },
    { value: 'Major', label: 'Majeur' },
    { value: 'Minor', label: 'Mineur' },
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
      return;
    }

    this.isSubmitting.set(true);
    this.serverError.set(null);

    const raw = this.form.getRawValue();

    // ON CHANGE TOUT ICI : PascalCase + Pas de wrapper command
    // Car l'erreur mentionne "Description" (D majuscule) et non "command.Description"
    const payload = {
      AuditId: this.auditId,
      WcagCriterion: raw.wcagCriterion,
      WcagCriterionName: raw.wcagCriterionName,
      HtmlElement: raw.htmlElement,
      Description: raw.description,
      Severity: this.severityMap[raw.severity],
      IsResolved: false
    };

    console.log('Tentative avec payload PascalCase :', payload);

    // On utilise "as any" pour passer outre le check de type du DTO Angular
    this.violationService.create(payload as any)
      .pipe(take(1))
      .subscribe({
        next: () => {
          this.liveAnnouncer.announce('Succès', 'polite');
          this.router.navigate(['/projects', this.projectId, 'audits']);
        },
        error: (err) => {
          this.isSubmitting.set(false);
          this.serverError.set(err.error?.title || "Erreur de validation");
          console.error('Rejet API détaillé :', err.error);
        }
      });
  }
}
