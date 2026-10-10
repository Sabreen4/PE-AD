import { Component, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProgramService } from '@features/program/services/program.service';
import { Program } from '@features/program/models/program.model';
import { PaginationComponent } from '@shared/components/pagination/pagination.component';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-programs',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, PaginationComponent, BackNavComponent],
  templateUrl: './programs.component.html',
  styleUrl: './programs.component.scss'
})
export class ProgramsComponent implements OnInit {
  private readonly programService: ProgramService = inject(ProgramService);
  private readonly router: Router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly search$ = new Subject<string>();

  public readonly programs = signal<Program[]>([]);
  public readonly totalRecords = signal<number>(0);
  public readonly currentPage = signal<number>(1);
  public readonly pageSize = signal<number>(10);
  public readonly searchQuery = signal<string>('');
  public readonly isLoading = signal<boolean>(false);
  public readonly errorMessage = signal<string | null>(null);

  constructor() {
    this.search$.pipe(
      debounceTime(500),
      distinctUntilChanged(),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe((value: string) => {
      this.searchQuery.set(value);
      this.currentPage.set(1);
      this.load();
    });
  }

  public ngOnInit(): void {
    this.load();
  }

  public onSearchInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.search$.next(value);
  }

  public goToPage(page: number): void {
    this.currentPage.set(page);
    this.load();
  }

  public onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.currentPage.set(1);
    this.load();
  }

  public navigateToDetail(id: string): void {
    this.router.navigate(['/evaluator/programs', id]);
  }

  public clearSearch(input: HTMLInputElement): void {
    this.searchQuery.set('');
    this.currentPage.set(1);
    input.value = '';
    this.load();
  }

  public load(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    const params: Record<string, string | number> = {
      page: this.currentPage(),
      limit: this.pageSize(),
    };

    const query: string = this.searchQuery().trim();
    if (query) {
      params['search'] = query;
    }

    this.programService.list(params).subscribe({
      next: (res) => {
        this.programs.set(res.items);
        this.totalRecords.set(res.total ?? res.items.length);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load programs.');
        this.isLoading.set(false);
      },
    });
  }
}
