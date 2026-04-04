import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { Router } from '@angular/router';
import { LiveAnnouncer } from '@angular/cdk/a11y';
// import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProjectService } from '../../../core/services/project.service';

@Component({
  selector: 'app-project-form',
  imports: [ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section aria-labelledby="form-heading">
      <h1 id="form-heading">Nouveau projet</h1>

      <!-- Erreur globale -->
      @if (serverError()) {
        <div role="alert" aria-live="assertive">
          {{ serverError() }}
        </div>
      }

      <!-- Confirmation succès -->
      @if (successMessage()) {
        <div role="status" aria-live="polite">
          {{ successMessage() }}
        </div>
      }

      <form [formGroup]="form"
            (ngSubmit)="submit()"
            novalidate
            aria-describedby="form-heading">

        <!-- Nom du projet -->
        <div class="form-group">
          <label for="name">
            Nom du projet
            <span aria-label="champ obligatoire">*</span>
          </label>
          <input
            id="name"
            type="text"
            formControlName="name"
            autocomplete="organization"
            aria-required="true"
            [attr.aria-invalid]="isInvalid(form.controls.name)"
            aria-describedby="name-error"
          />
          @if (isInvalid(form.controls.name)) {
            <span id="name-error" role="alert" aria-live="polite">
              @if (form.controls.name.hasError('required')) {
                Le nom est obligatoire.
              }
              @if (form.controls.name.hasError('maxlength')) {
                Le nom ne peut pas dépasser 200 caractères.
              }
            </span>
          }
        </div>

        <!-- URL cible -->
        <div class="form-group">
          <label for="targetUrl">
            URL cible
            <span aria-label="champ obligatoire">*</span>
          </label>
          <input
            id="targetUrl"
            type="url"
            formControlName="targetUrl"
            autocomplete="url"
            placeholder="https://exemple.com"
            aria-required="true"
            [attr.aria-invalid]="isInvalid(form.controls.targetUrl)"
            aria-describedby="url-error url-hint"
          />
          <span id="url-hint" class="hint">
            Doit commencer par https:// ou http://
          </span>
          @if (isInvalid(form.controls.targetUrl)) {
            <span id="url-error" role="alert" aria-live="polite">
              @if (form.controls.targetUrl.hasError('required')) {
                L'URL est obligatoire.
              }
              @if (form.controls.targetUrl.hasError('pattern')) {
                L'URL doit être valide (ex: https://exemple.com).
              }
            </span>
          }
        </div>

        <!-- Nom du client -->
        <div class="form-group">
          <label for="clientName">
            Nom du client
            <span aria-label="champ obligatoire">*</span>
          </label>
          <input
            id="clientName"
            type="text"
            formControlName="clientName"
            autocomplete="organization"
            aria-required="true"
            [attr.aria-invalid]="isInvalid(form.controls.clientName)"
            aria-describedby="client-error"
          />
          @if (isInvalid(form.controls.clientName)) {
            <span id="client-error" role="alert" aria-live="polite">
              Le nom du client est obligatoire.
            </span>
          }
        </div>

        <!-- Actions -->
        <div class="form-actions">
          <button
            type="submit"
            [attr.aria-disabled]="isSubmitting() || form.invalid"
            [attr.aria-label]="isSubmitting() ? 'Création en cours...' : 'Créer le projet'"
          >
            @if (isSubmitting()) { Création... } @else { Créer le projet }
          </button>

          <a routerLink="/projects"
             aria-label="Annuler et retourner à la liste des projets">
            Annuler
          </a>
        </div>

      </form>
    </section>
  `,
})
export class ProjectFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly projectService = inject(ProjectService);
  private readonly router = inject(Router);
  private readonly liveAnnouncer = inject(LiveAnnouncer);
  //private readonly destroyRef = inject(DestroyRef);

  readonly isSubmitting = signal(false);
  readonly serverError = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    targetUrl: ['', [
      Validators.required,
      Validators.pattern(/^https?:\/\/.+\..+/)
    ]],
    clientName: ['', [Validators.required, Validators.maxLength(200)]],
  });

  isInvalid(control: AbstractControl): boolean {
    return control.invalid && (control.dirty || control.touched);
  }

  submit(): void {
    if (this.form.invalid) {
      // Marque tous les champs comme touchés pour afficher les erreurs
      this.form.markAllAsTouched();
      this.liveAnnouncer.announce(
        'Le formulaire contient des erreurs. Veuillez les corriger.',
        'assertive'
      );
      return;
    }

    this.isSubmitting.set(true);
    this.serverError.set(null);

    this.projectService.create(this.form.getRawValue())
//      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.liveAnnouncer.announce(
            'Projet créé avec succès.',
            'polite'
          );
          this.router.navigate(['/projects']);
        },
        error: () => {
          this.serverError.set('Erreur lors de la création. Veuillez réessayer.');
          this.isSubmitting.set(false);
          this.liveAnnouncer.announce(
            'Erreur lors de la création du projet.',
            'assertive'
          );
        },
      });
  }
}