import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'truncate', standalone: true })
export class TruncatePipe implements PipeTransform {
  transform(value: string | null | undefined, limit = 50): string {
    if (!value) return '\u2014';
    return value.length > limit ? value.slice(0, limit) + '\u2026' : value;
  }
}