import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

const LABELS = {
  logoText: 'PE',
  title: 'Program Evaluation & Analytics',
  subtitle: 'Enterprise Training Platform',
} as const;

@Component({
  selector: 'app-authentication-layout',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './authentication-layout.component.html',
  styleUrl: './authentication-layout.component.scss'
})
export class AuthenticationLayoutComponent {
  public readonly labels = LABELS;
}
