import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, shareReplay, catchError, map } from 'rxjs';
import { apiConfig } from '@core/config/api.config';
import { FileTypeResponseDto } from '../models/FileTypeResponseDto';
import { RefSetTermResponseDto } from '../models/RefSetTermResponseDto';

@Injectable({ providedIn: 'root' })
export class MetadataService {
  private readonly http = inject(HttpClient);
  private fileTypes$: Observable<FileTypeResponseDto[]> | null = null;
  private refSetCache = new Map<string, Observable<RefSetTermResponseDto[]>>();

  getFileTypes(): Observable<FileTypeResponseDto[]> {
    if (!this.fileTypes$) {
      this.fileTypes$ = this.http.get<FileTypeResponseDto[]>(`${apiConfig.gatewayUrl}/metadata/file-types`).pipe(
        catchError(() => of([])),
        shareReplay(1)
      );
    }
    return this.fileTypes$;
  }

  getRefSetTerms(refSetKey: string): Observable<RefSetTermResponseDto[]> {
    if (!this.refSetCache.has(refSetKey)) {
      const obs = this.http.get<{ data?: RefSetTermResponseDto[] | null }>(`${apiConfig.gatewayUrl}/ref-sets/${refSetKey}/terms`).pipe(
        map((res) => res?.data ?? []),
        catchError(() => of([])),
        shareReplay(1)
      );
      this.refSetCache.set(refSetKey, obs);
    }
    return this.refSetCache.get(refSetKey)!;
  }
}
