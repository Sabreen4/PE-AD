import { Component, inject, output, computed, signal, HostListener, ElementRef, viewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { AuthenticationStateService } from '@core/authentication/authentication-state.service';
import { AuthenticationService } from '@core/authentication/authentication.service';
import { NotificationBellComponent } from '@features/notification/components/notification-bell/notification-bell.component';
import { ThemeService } from '@core/services/theme.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, NotificationBellComponent],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss'
})
export class NavbarComponent {
  private readonly authState: AuthenticationStateService = inject(AuthenticationStateService);
  private readonly authService: AuthenticationService = inject(AuthenticationService);
  private readonly router: Router = inject(Router);
  private readonly elementRef: ElementRef = inject(ElementRef);
  readonly themeService = inject(ThemeService);

  readonly toggleMobileNav = output<void>();

  readonly user = this.authState.user;
  readonly dropdownOpen = signal(false);

  readonly profileRoute = computed<string>(() => {
    const role = this.authState.role();
    if (role === 'Evaluator') return '/evaluator/profile';
    if (role === 'Employee') return '/employee/profile';
    return '/admin/profile';
  });

  readonly initials = computed<string>(() => {
    const email = this.user()?.email ?? '';
    return email.charAt(0).toUpperCase();
  });

  readonly displayName = computed<string>(() => {
    const email = this.user()?.email ?? '';
    return email.split('@')[0] || 'User';
  });

  toggleDropdown(): void {
    this.dropdownOpen.update((v) => !v);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.dropdownOpen.set(false);
    }
  }

  goToProfile(): void {
    this.dropdownOpen.set(false);
    this.router.navigate([this.profileRoute()]);
  }

  logout(): void {
    this.authService.logout();
  }
}
