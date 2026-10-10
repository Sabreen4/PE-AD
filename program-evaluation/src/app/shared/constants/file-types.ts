export interface FileTypeOption {
  label: string;
  extension: string;
}

export function getExtensionFromKey(key: string | null | undefined): string {
  if (!key) return '';
  return `.${key.toLowerCase().replace(/^\.+/, '')}`;
}

export function getDisplayLabel(option: { key: string; displayName: string }): string {
  return `${option.displayName} (.${option.key.toLowerCase()})`;
}
