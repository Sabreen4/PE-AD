import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { EvaluationService } from './EvaluationService';
import { LeaderboardEntry } from '../models/leaderboard.model';
import { LeaderboardEntryDto } from '../models/LeaderboardEntryDto';
import { OpenAPI } from '@core/api-gen/OpenAPI';

@Injectable({ providedIn: 'root' })
export class LeaderboardService {
  private readonly http = inject(HttpClient);
  private readonly svc = inject(EvaluationService);

  recalculate(programId: string): Observable<LeaderboardEntry[]> {
    return this.svc.postProgramLeaderboardRecalculate(Number(programId)).pipe(
      map((dto) => (dto?.data ?? []).map((entry) => ({
        id: entry.id ?? 0,
        enrollmentId: entry.enrollmentId ?? 0,
        employeeId: entry.employeeId ?? '',
        employeeName: entry.employeeName ?? '',
        overallScore: entry.overallScore ?? 0,
        rank: entry.rank ?? 0,
        percentile: entry.percentile ?? 0,
      })))
    );
  }

  list(programId: string, search?: string): Observable<LeaderboardEntry[]> {
    const queryParams: Record<string, string | number> = { pageNumber: 1, pageSize: 100 };
    if (search) queryParams['search'] = search;
    return this.http.get<{ data?: LeaderboardEntryDto[] }>(`${OpenAPI.BASE}/program/${Number(programId)}/leaderboard`, { params: queryParams }).pipe(
      map((dto) => (dto?.data ?? []).map((entry: LeaderboardEntryDto) => ({
        id: entry.id ?? 0,
        enrollmentId: entry.enrollmentId ?? 0,
        employeeId: entry.employeeId ?? '',
        employeeName: entry.employeeName ?? '',
        overallScore: entry.overallScore ?? 0,
        rank: entry.rank ?? 0,
        percentile: entry.percentile ?? 0,
      })))
    );
  }
}
