import { Component, inject, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { BreadcrumbHistoryService, BreadcrumbItem } from '@shared/services/breadcrumb-history.service';

export { BreadcrumbItem };

@Component({
  selector: 'app-back-nav',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './back-nav.component.html',
  styleUrl: './back-nav.component.scss'
})
export class BackNavComponent {
  private readonly historyService = inject(BreadcrumbHistoryService);
  private readonly router = inject(Router);

  readonly items = input<BreadcrumbItem[]>([]);
  readonly fallbackRoute = input<string>('/');
  readonly ariaLabel = input<string>('Breadcrumb navigation');

  public readonly computedBreadcrumbs = computed<BreadcrumbItem[]>(() => {
    const explicit = this.items();
    if (explicit && explicit.length > 0) {
      return explicit;
    }
    return this.historyService.history();
  });
}
