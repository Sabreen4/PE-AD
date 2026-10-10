import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

const LABELS = {
  errorCode: '404',
  title: 'Page Not Found',
  description: 'The page you are looking for does not exist or has been moved.',
  linkText: 'Go to Login',
} as const;

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './not-found.component.html',
  styleUrl: './not-found.component.scss'
})
export class NotFoundComponent {
  public readonly labels = LABELS;
}
