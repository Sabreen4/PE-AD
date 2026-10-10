import { Component, inject, signal, ViewChildren, QueryList, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { AuthenticationService } from '@core/authentication/authentication.service';
import { HttpErrorResponse } from '@angular/common/http';
import { IDENTITY_LABELS } from '@features/identity/constants/identity.constants';
import { ToastService } from '@shared/toast/toast.service';

@Component({
  selector: 'app-verify-otp',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './verify-otp.component.html',
  styleUrl: './verify-otp.component.scss'
})
export class VerifyOtpComponent implements AfterViewInit {
  private readonly authService: AuthenticationService = inject(AuthenticationService);
  private readonly router: Router = inject(Router);
  private readonly route: ActivatedRoute = inject(ActivatedRoute);
  private readonly toast: ToastService = inject(ToastService);

  public readonly labels = IDENTITY_LABELS.verifyOtp;
  public readonly isLoading = signal<boolean>(false);

  public readonly digits = signal<string[]>(['', '', '', '']);
  public readonly email = signal<string>('');

  @ViewChildren('otpInput') public otpInputs!: QueryList<ElementRef<HTMLInputElement>>;

  public ngAfterViewInit(): void {
    this.route.queryParams.subscribe((params) => {
      const emailParam: string = params['email'] ?? '';
      this.email.set(emailParam);
    });

    setTimeout(() => {
      this.focusInput(0);
    });
  }

  public onInput(index: number, event: Event): void {
    const input: HTMLInputElement = event.target as HTMLInputElement;
    const value: string = input.value.replace(/\D/g, '');

    if (value.length > 1) {
      const pasted: string[] = value.split('').slice(0, 4);
      const updated: string[] = ['', '', '', ''];
      pasted.forEach((char: string, i: number): void => {
        updated[i] = char;
      });
      this.digits.set(updated);
      this.focusInput(Math.min(pasted.length, 3));
      return;
    }

    const updated: string[] = [...this.digits()];
    updated[index] = value;
    this.digits.set(updated);

    if (value !== '' && index < 3) {
      this.focusInput(index + 1);
    }
  }

  public onKeydown(index: number, event: KeyboardEvent): void {
    if (event.key === 'Backspace' && this.digits()[index] === '' && index > 0) {
      this.focusInput(index - 1);
    }
  }

  public get maskedEmail(): string {
    const e: string = this.email();
    if (!e || !e.includes('@')) return e;
    const [local, domain]: string[] = e.split('@');
    return local.charAt(0) + '***@' + domain;
  }

  public get otpValue(): string {
    return this.digits().join('');
  }

  public get isComplete(): boolean {
    return this.digits().every((d: string): boolean => d !== '');
  }

  public onSubmit(): void {
    if (!this.isComplete || this.isLoading()) {
      return;
    }

    this.isLoading.set(true);

    const emailValue: string = this.email();
    const otp: string = this.otpValue;

    if (!emailValue) {
      this.toast.error('Session Expired', 'Session expired. Please login again.');
      this.isLoading.set(false);
      return;
    }

    this.authService.verifyOtp(emailValue, otp).subscribe({
      next: (): void => {
        this.isLoading.set(false);
      },
      error: (error: HttpErrorResponse): void => {
        this.isLoading.set(false);
        const updated: string[] = ['', '', '', ''];
        this.digits.set(updated);
        this.focusInput(0);

        const errorBody: { message?: string } | null = this.tryParseError(error);
        this.toast.error('Verification Failed', errorBody?.message ?? 'Invalid OTP. Please try again.');
      },
    });
  }

  public onResendOtp(): void {
    const emailValue: string = this.email();
    if (!emailValue) {
      return;
    }

    this.isLoading.set(true);

    this.authService.login(emailValue).subscribe({
      next: (): void => {
        this.isLoading.set(false);
        this.toast.success('OTP Sent', 'A new OTP has been sent to your email.');
        const updated: string[] = ['', '', '', ''];
        this.digits.set(updated);
        this.focusInput(0);
      },
      error: (error: HttpErrorResponse): void => {
        this.isLoading.set(false);
        const errorBody: { message?: string } | null = this.tryParseError(error);
        this.toast.error('Failed', errorBody?.message ?? 'Failed to resend OTP.');
      },
    });
  }

  public trackByIndex(index: number): number {
    return index;
  }

  private focusInput(index: number): void {
    if (this.otpInputs && this.otpInputs.get(index)) {
      this.otpInputs.get(index)!.nativeElement.focus();
    }
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
