import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'statusLabel', standalone: true })
export class StatusLabelPipe implements PipeTransform {
  transform(value: boolean | null | undefined, trueLabel?: string, falseLabel?: string): string {
    return value ? (trueLabel || 'Active') : (falseLabel || 'Inactive');
  }
}