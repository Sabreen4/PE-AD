import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
  FormControl,
} from '@angular/forms';
import { IdentityApiService } from '@core/api/identity/identity-api.service';
import { AuthenticationStateService } from '@core/authentication/authentication-state.service';
import { CreateUserRequest } from '@core/models/identity/create-user-request.model';
import { IdResponse } from '@core/models/identity/id-response.model';
import { HttpErrorResponse } from '@angular/common/http';
import { IDENTITY_LABELS } from '@features/identity/constants/identity.constants';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';
import { ToastService } from '@shared/toast/toast.service';

const GENDER_OPTIONS: string[] = ['Male', 'Female', 'Other'];

@Component({
  selector: 'app-employee-registration',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, BackNavComponent],
  templateUrl: './employee-registration.component.html',
  styleUrl: './employee-registration.component.scss'
})
export class EmployeeRegistrationComponent {
  private readonly fb: FormBuilder = inject(FormBuilder);
  private readonly api: IdentityApiService = inject(IdentityApiService);
  private readonly authState: AuthenticationStateService = inject(AuthenticationStateService);
  private readonly toast: ToastService = inject(ToastService);

  public readonly labels = IDENTITY_LABELS.employeeRegistration;
  public readonly genderOptions = GENDER_OPTIONS;
  public readonly isLoading = signal<boolean>(false);
  public readonly isSubmitted = signal<boolean>(false);

  public registrationForm: FormGroup = this.fb.group({
    firstName: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    lastName: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    email: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    phone: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern('^[0-9]{10}$')],
    }),
    gender: new FormControl<string>('Male', {
      nonNullable: true,
    }),
  });

  public get f(): Record<string, FormControl<string>> {
    return this.registrationForm.controls as Record<string, FormControl<string>>;
  }

  public onSubmit(): void {
    if (this.registrationForm.invalid || this.isLoading()) {
      return;
    }

    this.isLoading.set(true);
    this.isSubmitted.set(true);

    const orgId: number = this.authState.user()?.organizationId ?? 0;
    const formValue: Record<string, string> = this.registrationForm.value as Record<string, string>;

    const request: CreateUserRequest = {
      firstName: formValue['firstName'],
      lastName: formValue['lastName'],
      email: formValue['email'],
      phone: formValue['phone'],
      organizationId: orgId,
      roleType: 'Employee',
      gender: formValue['gender'] ?? '',
    };

    this.api.createUser(request).subscribe({
      next: (response: IdResponse): void => {
        this.isLoading.set(false);
        this.registrationForm.reset({ gender: 'Male' });
        this.isSubmitted.set(false);
        this.toast.success('Employee Created', response.message ?? 'Employee created successfully.');
      },
      error: (error: HttpErrorResponse): void => {
        this.isLoading.set(false);
        this.isSubmitted.set(false);
        const message: string = this.parseErrorMessage(error);
        this.toast.error('Creation Failed', message);
      },
    });
  }

  private parseErrorMessage(error: HttpErrorResponse): string {
    const body: { message?: string } | null = this.tryParseError(error);
    const raw: string = body?.message ?? '';

    if (/email already exists/i.test(raw)) {
      return 'An account with this email already exists.';
    }

    return raw || 'Failed to create employee. Please try again.';
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
