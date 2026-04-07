import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { LiveAnnouncer } from '@angular/cdk/a11y';
import { take } from 'rxjs';
import { ViolationService } from '../../core/services/violation.service';
import { LogViolationCommand } from '../../core/models/violation.model';

@Component({
  selector: 'app-violation-form',
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

  readonly wcagCriteria = [
    { value: '1.1.1', label: '1.1.1 — Contenu non textuel' },
    { value: '1.3.1', label: '1.3.1 — Information et relations' },
    { value: '1.4.3', label: '1.4.3 — Contraste (minimum)' },
    { value: '2.1.1', label: '2.1.1 — Clavier' },
    { value: '4.1.2', label: '4.1.2 — Nom, rôle, valeur' },
  ];

  readonly severityMap = { Critical: 1, Major: 2, Minor: 3 };

  readonly severityOptions = [
    { value: 'Critical', label: 'Critique' },
    { value: 'Major', label: 'Majeur' },
    { value: 'Minor', label: 'Mineur' },
  ];

  readonly form = this.fb.nonNullable.group({
    wcagCriterion: ['', Validators.required],
    wcagCriterionName: ['', Validators.required],
    htmlElement: ['', [Validators.required, Validators.maxLength(500)]],
    description: ['', [Validators.required, Validators.maxLength(2000)]],
    severity: ['', Validators.required],
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

    const payload: LogViolationCommand = {
      auditId: this.auditId,
      wcagCriterion: raw.wcagCriterion,
      wcagCriterionName: raw.wcagCriterionName,
      htmlElement: raw.htmlElement,
      description: raw.description,
      severity: this.severityMap[raw.severity as keyof typeof this.severityMap],
      isResolved: false,
    };

    this.violationService
      .create(payload)
      .pipe(take(1))
      .subscribe({
        next: () => {
          this.liveAnnouncer.announce('Violation créée avec succès.', 'polite');
          this.router.navigate(['/projects', this.projectId, 'audits']);
        },
        error: (err) => {
          this.isSubmitting.set(false);
          this.serverError.set(
            err.error?.title || 'Erreur lors de la création de la violation.'
          );
        },
      });
  }
}
