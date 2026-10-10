import { Directive, input, output, HostListener, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, debounceTime, distinctUntilChanged, map, filter } from 'rxjs';

@Directive({
  selector: '[appDebouncedSearch]',
  standalone: true,
})
export class DebouncedSearchDirective {
  public readonly debounceTime = input(300);
  public readonly minLength = input(1);
  public readonly appDebouncedSearch = output<string>();

  private readonly destroyRef = inject(DestroyRef);
  private readonly search$ = new Subject<string>();

  constructor() {
    this.search$.pipe(
      debounceTime(this.debounceTime()),
      map((v) => v.trim()),
      filter((v) => v.length === 0 || v.length >= this.minLength()),
      distinctUntilChanged(),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe((value) => this.appDebouncedSearch.emit(value));
  }

  @HostListener('input', ['$event.target.value'])
  protected onInput(value: string): void {
    this.search$.next(value);
  }
}
