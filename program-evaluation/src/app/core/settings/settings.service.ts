import { Injectable, signal, effect } from '@angular/core';

export type AppTheme = 'default' | 'dark-blue' | 'indigo' | 'teal' | 'green' | 'purple' | 'dark';
export type FontSize = 'small' | 'medium' | 'large' | 'xlarge';
export type LayoutMode = 'comfortable' | 'compact';
export type AnimationMode = 'enabled' | 'disabled';
export type ContrastMode = 'normal' | 'high';

export interface AppSettings {
  theme: AppTheme;
  fontSize: FontSize;
  layout: LayoutMode;
  animations: AnimationMode;
  contrast: ContrastMode;
}

const SETTINGS_KEY = 'app_settings';
const DEFAULTS: AppSettings = {
  theme: 'default',
  fontSize: 'medium',
  layout: 'comfortable',
  animations: 'enabled',
  contrast: 'normal',
};

function loadSettings(): AppSettings {
  try {
    const raw = localStorage.getItem(SETTINGS_KEY);
    if (raw) return { ...DEFAULTS, ...JSON.parse(raw) };
  } catch { /* ignore */ }
  return { ...DEFAULTS };
}

function applyToDocument(settings: AppSettings): void {
  const doc = document.documentElement;

  doc.setAttribute('data-theme', settings.theme);
  doc.setAttribute('data-font-size', settings.fontSize);
  doc.setAttribute('data-layout', settings.layout);
  doc.setAttribute('data-animations', settings.animations);
  doc.setAttribute('data-contrast', settings.contrast);
}

@Injectable({ providedIn: 'root' })
export class SettingsService {
  private readonly state = signal<AppSettings>(loadSettings());

  public readonly settings = this.state.asReadonly();

  constructor() {
    applyToDocument(this.state());
    effect(() => {
      const s = this.state();
      localStorage.setItem(SETTINGS_KEY, JSON.stringify(s));
      applyToDocument(s);
    });
  }

  public update(partial: Partial<AppSettings>): void {
    this.state.update((s) => ({ ...s, ...partial }));
  }

  public reset(): void {
    this.state.set({ ...DEFAULTS });
  }
}
