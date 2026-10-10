import { Component, computed, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TableColumnConfig } from '@shared/common/entity-types';
import { LoadingSpinnerComponent } from '@shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '@shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-entity-table',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, MatIconModule, MatTooltipModule, LoadingSpinnerComponent, EmptyStateComponent],
  templateUrl: './entity-table.component.html',
  styleUrl: './entity-table.component.scss'
})
export class EntityTableComponent<T extends { id: string }> {
  columns = input.required<TableColumnConfig<T>[]>();
  data = input<T[]>([]);
  loading = input(false);
  showEdit = input(true);
  showDelete = input(true);
  emptyTitle = input('No records yet');
  emptyHint = input('Nothing has been added yet.');

  edit = output<T>();
  delete = output<T>();
  rowClick = output<T>();

  displayedColumns = computed(() => {
    const keys = this.columns().map((c) => c.key);
    return this.showEdit() || this.showDelete() ? [...keys, 'actions'] : keys;
  });

  cellValue(row: T, col: TableColumnConfig<T>): string {
    if (col.formatter) return col.formatter(row);
    const value = (row as Record<string, string | number | boolean | null>)[col.key as string];
    return value === undefined || value === null || value === '' ? '\u2014' : String(value);
  }
}