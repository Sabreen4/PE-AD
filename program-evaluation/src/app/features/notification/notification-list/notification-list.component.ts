import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-notification-list',
  standalone: true,
  imports: [CommonModule, BackNavComponent],
  templateUrl: './notification-list.component.html',
})
export class NotificationListComponent {}