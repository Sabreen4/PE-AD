import { Component, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export const DEFAULT_PAGE_SIZES = [10, 25, 50, 100];

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pagination.component.html',
  styleUrl: './pagination.component.scss',
})
export class PaginationComponent {
  readonly currentPage = input<number>(1);
  readonly totalRecords = input<number>(0);
  readonly pageSize = input<number>(10);
  readonly pageSizes = input<number[]>(DEFAULT_PAGE_SIZES);
  readonly isLoading = input<boolean>(false);

  readonly pageChange = output<number>();
  readonly pageSizeChange = output<number>();

  readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.totalRecords() / this.pageSize()))
  );

  readonly startRecord = computed(() =>
    this.totalRecords() > 0
      ? (this.currentPage() - 1) * this.pageSize() + 1
      : 0
  );

  readonly endRecord = computed(() =>
    Math.min(this.startRecord() + this.pageSize() - 1, this.totalRecords())
  );

  readonly pageNumbers = computed(() => {
    const total = this.totalPages();
    const current = this.currentPage();
    const pages: number[] = [];
    const start = Math.max(1, current - 2);
    const end = Math.min(total, current + 2);
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  });

  readonly showPagination = computed(() => this.totalPages() > 1);

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.currentPage()) return;
    this.pageChange.emit(page);
  }

  onPageSizeChange(value: number): void {
    this.pageSizeChange.emit(value);
  }
}
