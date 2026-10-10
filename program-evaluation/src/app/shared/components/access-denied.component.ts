import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

const LABELS = {
  errorCode: '403',
  title: 'Access Denied',
  description: 'You do not have permission to access this page.',
  linkText: 'Go to Login',
} as const;

@Component({
  selector: 'app-access-denied',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './access-denied.component.html',
  styleUrl: './access-denied.component.scss'
})
export class AccessDeniedComponent {
  public readonly labels = LABELS;
}
