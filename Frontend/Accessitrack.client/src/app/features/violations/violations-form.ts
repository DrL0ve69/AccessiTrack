import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { LiveAnnouncer } from '@angular/cdk/a11y';
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

  readonly wcagCriteria: { value: string; label: string }[] = [
    { value: '1.1.1', label: '1.1.1 — Contenu non textuel (Alt text)' },
    { value: '1.3.1', label: '1.3.1 — Information et relations (Structure HTML)' },
    { value: '1.4.1', label: '1.4.1 — Utilisation de la couleur' },
    { value: '1.4.3', label: '1.4.3 — Contraste (minimum 4.5:1)' },
    { value: '1.4.4', label: '1.4.4 — Redimensionnement du texte' },
    { value: '2.1.1', label: '2.1.1 — Clavier (navigation complète)' },
    { value: '2.1.2', label: '2.1.2 — Pas de piège au clavier' },
    { value: '2.4.1', label: '2.4.1 — Contournement de blocs (Skip link)' },
    { value: '2.4.3', label: '2.4.3 — Parcours du focus' },
    { value: '2.4.4', label: '2.4.4 — Fonction du lien' },
    { value: '2.4.7', label: '2.4.7 — Visibilité du focus' },
    { value: '3.1.1', label: '3.1.1 — Langue de la page' },
    { value: '3.3.1', label: '3.3.1 — Identification des erreurs' },
    { value: '3.3.2', label: '3.3.2 — Étiquettes ou instructions' },
    { value: '4.1.1', label: '4.1.1 — Analyse syntaxique (HTML valide)' },
    { value: '4.1.2', label: '4.1.2 — Nom, rôle, valeur (ARIA)' },
    { value: '4.1.3', label: "4.1.3 — Messages d'état (aria-live)" },
  ];

  readonly severityOptions: { value: ViolationSeverity; label: string; description: string }[] = [
    { value: 'Critical', label: 'Critique', description: "Bloque complètement l'accès à une fonctionnalité" },
    { value: 'Major', label: 'Majeur', description: "Rend l'usage très difficile mais pas impossible" },
    { value: 'Minor', label: 'Mineur', description: 'Gêne mineure, contournement possible' },
  ];

  readonly form = this.fb.nonNullable.group({
    wcagCriterion: ['', Validators.required],
    wcagCriterionName: ['', Validators.required],
    htmlElement: ['', [Validators.required, Validators.maxLength(500)]],
    description: ['', [Validators.required, Validators.maxLength(2000)]],
    severity: ['' as ViolationSeverity, Validators.required],
  });

  // MODIFICATION : Récupération des deux IDs pour assurer une navigation de retour stable
  private readonly auditId = this.route.snapshot.paramMap.get('id') ?? '';
  private readonly projectId = this.route.snapshot.paramMap.get('projectId') ?? '';

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
      this.liveAnnouncer.announce('Le formulaire contient des erreurs.', 'assertive');
      return;
    }

    this.isSubmitting.set(true);
    this.serverError.set(null);

    const { wcagCriterion, wcagCriterionName, htmlElement, description, severity } =
      this.form.getRawValue();

    this.violationService
      .create({
        auditId: this.auditId,
        wcagCriterion,
        wcagCriterionName,
        htmlElement,
        description,
        severity,
      })
      .subscribe({
        next: () => {
          this.liveAnnouncer.announce('Violation enregistrée avec succès.', 'polite');
          // MODIFICATION : Retour explicite à la liste des audits du projet pour éviter le redirect dashboard
          this.router.navigate(['/projects', this.projectId, 'audits']);
        },
        error: (err: unknown) => {
          const message = err instanceof Error ? err.message : "Erreur lors de l'enregistrement.";
          this.serverError.set(message);
          this.isSubmitting.set(false);
        },
      });
  }
}
