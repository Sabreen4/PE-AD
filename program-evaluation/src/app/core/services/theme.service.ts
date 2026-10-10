import { Injectable, inject, computed } from '@angular/core';
import { SettingsService } from '@core/settings/settings.service';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly settings = inject(SettingsService);

  public readonly isDarkMode = computed(() =>
    this.settings.settings().theme === 'dark'
  );

  public toggleDarkMode(): void {
    this.settings.update({
      theme: this.isDarkMode() ? 'default' : 'dark',
    });
  }
}
