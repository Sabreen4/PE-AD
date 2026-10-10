import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthenticationStateService } from '@core/authentication/authentication-state.service';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

const LABELS = {
  pageEyebrow: 'Dashboard',
  pageTitle: 'Evaluator Dashboard',
  welcomePrefix: 'Welcome,',
  subtitle: 'Your evaluator portal',
} as const;

@Component({
  selector: 'app-evaluator-dashboard',
  standalone: true,
  imports: [CommonModule, BackNavComponent],
  templateUrl: './evaluator-dashboard.component.html',
  styleUrl: './evaluator-dashboard.component.scss'
})
export class EvaluatorDashboardComponent {
  private readonly authState: AuthenticationStateService = inject(AuthenticationStateService);
  public readonly labels = LABELS;
  public readonly user = this.authState.user;
}
