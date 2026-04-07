import { Component, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, AbstractControlOptions } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { take } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';

function passwordMatchValidator(form: FormGroup): null | { passwordMismatch: true } {
  const password = form.get('password')?.value;
  const confirmPassword = form.get('confirmPassword')?.value;
  return password === confirmPassword ? null : { passwordMismatch: true };
}

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly errorMessage = signal<string | null>(null);
  readonly isLoading = signal<boolean>(false);

  readonly registerForm = this.fb.nonNullable.group(
    {
      userName: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: passwordMatchValidator } as AbstractControlOptions
  );

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.errorMessage.set('Veuillez corriger les erreurs du formulaire.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const { userName, email, password } = this.registerForm.getRawValue();

    this.authService.register({ userName, email, password })
      .pipe(take(1))
      .subscribe({
        next: () => {
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          this.errorMessage.set(error.message || 'Erreur lors de l\'inscription.');
          this.isLoading.set(false);
        },
      });
  }
}
