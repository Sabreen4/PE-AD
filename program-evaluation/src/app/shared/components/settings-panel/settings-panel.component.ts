import { Component, inject, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { SettingsService, AppTheme, FontSize } from '@core/settings/settings.service';

@Component({
  selector: 'app-settings-panel',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './settings-panel.component.html',
  styleUrl: './settings-panel.component.scss',
})
export class SettingsPanelComponent {
  protected readonly service = inject(SettingsService);
  readonly closePanel = output<void>();

  protected readonly themes: { value: AppTheme; label: string; color: string }[] = [
    { value: 'default', label: 'Default Blue', color: '#3b5bdb' },
    { value: 'dark-blue', label: 'Dark Blue', color: '#1e40af' },
    { value: 'indigo', label: 'Indigo', color: '#4f46e5' },
    { value: 'teal', label: 'Teal', color: '#0d9488' },
    { value: 'green', label: 'Green', color: '#16a34a' },
    { value: 'purple', label: 'Purple', color: '#9333ea' },
    { value: 'dark', label: 'Dark Mode', color: '#0f172a' },
  ];

  protected readonly fontSizes: { value: FontSize; label: string }[] = [
    { value: 'small', label: 'Small' },
    { value: 'medium', label: 'Medium' },
    { value: 'large', label: 'Large' },
    { value: 'xlarge', label: 'Extra Large' },
  ];

  protected setTheme(theme: AppTheme): void {
    this.service.update({ theme });
  }

  protected setFontSize(fontSize: FontSize): void {
    this.service.update({ fontSize });
  }

  protected toggleLayout(): void {
    const next = this.service.settings().layout === 'comfortable' ? 'compact' : 'comfortable';
    this.service.update({ layout: next });
  }

  protected toggleAnimations(): void {
    const next = this.service.settings().animations === 'enabled' ? 'disabled' : 'enabled';
    this.service.update({ animations: next });
  }

  protected toggleContrast(): void {
    const next = this.service.settings().contrast === 'normal' ? 'high' : 'normal';
    this.service.update({ contrast: next });
  }

  protected resetSettings(): void {
    this.service.reset();
  }
}
