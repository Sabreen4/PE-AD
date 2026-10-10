import { Injectable, inject, signal } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

export interface BreadcrumbItem {
  label: string;
  url: string;
  queryParams?: Record<string, string | number>;
}

@Injectable({ providedIn: 'root' })
export class BreadcrumbHistoryService {
  private readonly router = inject(Router);

  public readonly history = signal<BreadcrumbItem[]>([]);

  constructor() {
    // Record initial route on service instantiation
    this.recordNavigation(this.router.url);

    // Listen to subsequent navigation events
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe((event) => {
        this.recordNavigation(event.urlAfterRedirects || event.url);
      });
  }

  private recordNavigation(urlWithParams: string): void {
    if (!urlWithParams) return;
    const rawUrl = urlWithParams.split('?')[0];
    const segments = rawUrl.split('/').filter(Boolean);
    if (segments.length === 0) return;

    const root = segments[0];
    const label = this.getLabelForUrl(rawUrl, segments);
    if (!label) return;

    const rootUrl = root === 'evaluator' ? '/evaluator'
      : root === 'employee' ? '/employee/dashboard'
      : root === 'admin' ? '/admin' : '/';

    const rootLabel = root === 'evaluator' ? 'Dashboard'
      : root === 'employee' ? 'Dashboard'
      : root === 'admin' ? 'Admin Panel' : 'Dashboard';

    const newItem: BreadcrumbItem = { label, url: urlWithParams };

    this.history.update((current) => {
      let list = [...current];

      // If history is empty or root changed, reset base root
      if (list.length === 0 || list[0].url.split('?')[0] !== rootUrl) {
        list = [{ label: rootLabel, url: rootUrl }];
      }

      // If navigating to root itself
      if (rawUrl === rootUrl || rawUrl === `${rootUrl}/dashboard`) {
        return [{ label: rootLabel, url: rootUrl }];
      }

      // Check if URL is already in history stack
      const existingIndex = list.findIndex((item) => item.url.split('?')[0] === rawUrl);

      if (existingIndex !== -1) {
        // User navigated back to a previously visited page -> trim stack
        return list.slice(0, existingIndex + 1);
      } else {
        // Append new page to navigation trail
        return [...list, newItem];
      }
    });
  }

  private getLabelForUrl(rawUrl: string, segments: string[]): string | null {
    if (rawUrl.includes('/attendance')) return 'Attendance';
    if (rawUrl.includes('/leaderboard')) return 'Leaderboard';
    if (rawUrl.includes('/weekly-scores')) return 'Weekly Scores';
    if (rawUrl.includes('/assignment-submissions')) return 'Assignment Submissions';
    if (rawUrl.includes('/feedback')) return 'Feedback';
    if (rawUrl.includes('/programs/create')) return 'Create Program';
    if (rawUrl.includes('/programs')) return 'Programs';
    if (rawUrl.includes('/assignments')) return 'Assignments';

    const last = segments[segments.length - 1];
    return last ? last.replace(/[-_]/g, ' ').replace(/\b\w/g, (c) => c.toUpperCase()) : null;
  }
}
