import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormControl } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '@features/identity/services/user.service';
import { AuthenticationStateService } from '@core/authentication/authentication-state.service';
import { User } from '@features/identity/models/user.model';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastService } from '@shared/toast/toast.service';

@Component({
  selector: 'app-employee-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class EmployeeProfileComponent implements OnInit {
  private readonly fb: FormBuilder = inject(FormBuilder);
  private readonly userService: UserService = inject(UserService);
  protected readonly authState: AuthenticationStateService = inject(AuthenticationStateService);
  private readonly router: Router = inject(Router);
  private readonly toast: ToastService = inject(ToastService);

  public readonly isLoading = signal<boolean>(true);
  public readonly isSaving = signal<boolean>(false);

  public profileForm: FormGroup = this.fb.group({
    firstName: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    lastName: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    email: new FormControl<string>({ value: '', disabled: true }, { nonNullable: true }),
    phone: new FormControl<string>('', { nonNullable: true, validators: [Validators.pattern('^[0-9]{10}$')] }),
  });

  public get f(): Record<string, FormControl<string>> {
    return this.profileForm.controls as Record<string, FormControl<string>>;
  }

  public ngOnInit(): void {
    this.loadProfile();
  }

  private loadProfile(): void {
    const userId: string | undefined = this.authState.user()?.userId;
    if (userId === undefined) {
      this.toast.error('Profile', 'User not found.');
      this.isLoading.set(false);
      return;
    }

    this.isLoading.set(true);
    this.userService.getById(userId).subscribe({
      next: (user: User) => {
        this.profileForm.patchValue({
          firstName: user.firstName ?? '',
          lastName: user.lastName ?? '',
          email: user.email ?? '',
          phone: user.phone ?? '',
        });
        this.isLoading.set(false);
      },
      error: () => {
        this.toast.error('Profile', 'Failed to load profile.');
        this.isLoading.set(false);
      },
    });
  }

  public close(): void {
    if (window.history.length > 1) {
      window.history.back();
    } else {
      this.router.navigate(['/employee']);
    }
  }

  public onSubmit(): void {
    if (this.profileForm.invalid || this.isSaving()) return;

    this.isSaving.set(true);

    const userId: string | undefined = this.authState.user()?.userId;
    if (userId === undefined) {
      this.toast.error('Profile', 'User not found.');
      this.isSaving.set(false);
      return;
    }

    const formValue: Record<string, string> = this.profileForm.getRawValue() as Record<string, string>;

    this.userService.update(userId, {
      firstName: formValue['firstName'],
      lastName: formValue['lastName'],
      phone: formValue['phone'] || null,
    }).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.toast.success('Profile', 'Profile updated successfully.');
      },
      error: (err: HttpErrorResponse) => {
        this.isSaving.set(false);
        this.toast.error('Profile', err.error?.message || 'Failed to update profile.');
      },
    });
  }
}
