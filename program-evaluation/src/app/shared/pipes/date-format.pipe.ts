import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'dateFormat', standalone: true })
export class DateFormatPipe implements PipeTransform {
  transform(value: string | Date | null | undefined, format?: string): string {
    if (!value) return '\u2014';
    return new Date(value).toLocaleDateString();
  }
}