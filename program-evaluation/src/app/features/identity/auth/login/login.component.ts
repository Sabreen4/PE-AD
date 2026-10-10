import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormControl } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthenticationService } from '@core/authentication/authentication.service';
import { LoginResponse } from '@core/models/identity/login-response.model';
import { HttpErrorResponse } from '@angular/common/http';
import { IDENTITY_LABELS } from '@features/identity/constants/identity.constants';
import { ToastService } from '@shared/toast/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private readonly fb: FormBuilder = inject(FormBuilder);
  private readonly authService: AuthenticationService = inject(AuthenticationService);
  private readonly router: Router = inject(Router);
  private readonly toast: ToastService = inject(ToastService);

  public readonly labels = IDENTITY_LABELS.login;
  public readonly isLoading = signal<boolean>(false);

  public loginForm: FormGroup = this.fb.group({
    email: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
  });

  public get emailControl(): FormControl<string> {
    return this.loginForm.get('email') as FormControl<string>;
  }

  public onSubmit(): void {
    if (this.loginForm.invalid) {
      return;
    }

    this.isLoading.set(true);

    const email: string = this.emailControl.value;

    this.authService.login(email).subscribe({
      next: (response: LoginResponse): void => {
        this.isLoading.set(false);
        this.router.navigate(['/auth/verify-otp'], { queryParams: { email } });
      },
      error: (error: HttpErrorResponse): void => {
        this.isLoading.set(false);
        let msg = 'Failed to send OTP. Please try again.';
        if (error.status === 0) {
          msg = 'Unable to reach the server. Please check your connection and try again.';
        } else if (error.status >= 500) {
          msg = 'The server encountered an error. Please try again later.';
        } else {
          const errorBody: { message?: string } | null = this.tryParseError(error);
          msg = errorBody?.message ?? msg;
        }
        this.toast.error('Login Failed', msg);
      },
    });
  }

  private tryParseError(error: HttpErrorResponse): { message?: string } | null {
    if (typeof error.error === 'object' && error.error !== null) {
      return error.error as { message?: string };
    }
    try {
      const parsed: { message?: string } = JSON.parse(error.error as string) as { message?: string };
      return parsed;
    } catch {
      return null;
    }
  }
}
