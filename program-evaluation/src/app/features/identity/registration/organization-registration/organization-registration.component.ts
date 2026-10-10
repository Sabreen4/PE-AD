import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
  FormControl,
} from '@angular/forms';
import { Router } from '@angular/router';
import { IdentityApiService } from '@core/api/identity/identity-api.service';
import { CreateOrganizationRequest } from '@core/models/identity/create-organization-request.model';
import { IdResponse } from '@core/models/identity/id-response.model';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastService } from '@shared/toast/toast.service';

type StepStatus = 'completed' | 'active' | 'upcoming';

interface StepState {
  status: StepStatus;
  icon: string;
  completedIcon: string;
  label: string;
}

const GENDER_OPTIONS: string[] = ['Male', 'Female', 'Other'];

@Component({
  selector: 'app-organization-registration',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './organization-registration.component.html',
  styleUrl: './organization-registration.component.scss'
})
export class OrganizationRegistrationComponent implements OnInit {
  private readonly fb: FormBuilder = inject(FormBuilder);
  private readonly api: IdentityApiService = inject(IdentityApiService);
  private readonly router: Router = inject(Router);
  private readonly toast: ToastService = inject(ToastService);

  public readonly genderOptions = GENDER_OPTIONS;
  public readonly isLoading = signal<boolean>(false);

  private readonly orgFields = ['organizationName', 'organizationEmail', 'organizationPhone', 'city'] as const;
  private readonly adminFields = ['adminFirstName', 'adminLastName', 'adminEmail', 'adminPhone'] as const;

  public readonly steps = signal<StepState[]>([
    { status: 'active', icon: 'corporate_fare', completedIcon: 'check_circle', label: 'Organization' },
    { status: 'upcoming', icon: 'person', completedIcon: 'check_circle', label: 'Admin' },
    { status: 'upcoming', icon: 'verified', completedIcon: 'check_circle', label: 'Review' },
  ]);

  public registrationForm: FormGroup = this.fb.group({
    organizationName: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    organizationEmail: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    organizationPhone: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern('^[0-9]{10}$')],
    }),
    city: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    adminFirstName: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    adminLastName: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    adminEmail: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    adminPhone: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern('^[0-9]{10}$')],
    }),
    adminGender: new FormControl<string>('', {
      nonNullable: true,
    }),
  });

  public get f(): Record<string, FormControl<string>> {
    return this.registrationForm.controls as Record<string, FormControl<string>>;
  }

  public ngOnInit(): void {
    this.registrationForm.valueChanges.subscribe(() => this.updateSteps());
    this.updateSteps();
  }

  private isSectionValid(fields: readonly string[]): boolean {
    return fields.every(f => {
      const ctrl = this.f[f];
      return ctrl.valid && (ctrl.dirty || ctrl.touched);
    });
  }

  private updateSteps(): void {
    const orgDone = this.isSectionValid(this.orgFields);
    const adminDone = this.isSectionValid(this.adminFields);

    const steps: StepState[] = [
      { status: orgDone ? 'completed' : 'active', icon: 'corporate_fare', completedIcon: 'check_circle', label: 'Organization' },
      { status: adminDone ? 'completed' : orgDone ? 'active' : 'upcoming', icon: 'person', completedIcon: 'check_circle', label: 'Admin' },
      { status: adminDone ? 'active' : 'upcoming', icon: 'verified', completedIcon: 'check_circle', label: 'Review' },
    ];
    this.steps.set(steps);
  }

  public goBack(): void {
    if (window.history.length > 1) {
      window.history.back();
    } else {
      this.router.navigate(['/auth/login']);
    }
  }

  public onSubmit(): void {
    if (this.registrationForm.invalid) {
      return;
    }

    this.isLoading.set(true);

    const request: CreateOrganizationRequest = this.registrationForm.value as CreateOrganizationRequest;

    this.api.createOrganization(request).subscribe({
      next: (response: IdResponse): void => {
        this.isLoading.set(false);
        this.toast.success('Registration Complete', response.message ?? 'Organization registered successfully. You can now login.');
        setTimeout((): void => {
          this.router.navigate(['/auth/login']);
        }, 3000);
      },
      error: (error: HttpErrorResponse): void => {
        this.isLoading.set(false);
        const errorBody: { message?: string } | null = this.tryParseError(error);
        this.toast.error('Registration Failed', errorBody?.message ?? 'Registration failed. Please try again.');
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
