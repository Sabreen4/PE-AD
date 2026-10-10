import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, BackNavComponent],
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent {}