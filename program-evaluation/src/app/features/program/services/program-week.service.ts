import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ProgramService as MonolithProgramService } from './ProgramService';
import { ProgramWeek } from '../models/program-week.model';
import { ProgramWeekDto } from '../models/ProgramWeekDto';
import { OpenAPI } from '@core/api-gen/OpenAPI';


function toId(value: string | undefined | null): number {
  if (value == null) return 0;
  const n = Number(value);
  return isNaN(n) ? 0 : n;
}

@Injectable({ providedIn: 'root' })
export class ProgramWeekService {
  private readonly http = inject(HttpClient);
  private readonly svc = inject(MonolithProgramService);

  listByProgram(
    programId: string,
    params?: Record<string, string | number | boolean | null>
  ): Observable<{ items: ProgramWeek[] }> {
    const id = toId(programId);
    const page: number = (params?.['page'] as number) ?? 1;
    const limit: number = (params?.['limit'] as number) ?? 100;
    const search = params?.['search'] as string | undefined;
    const queryParams: Record<string, string | number> = { pageNumber: page, pageSize: limit };
    if (search) queryParams['search'] = search;
    return this.http.get<{ data?: ProgramWeekDto[] }>(`${OpenAPI.BASE}/program/${id}/program-week`, { params: queryParams }).pipe(
      map((res) => ({
        items: (res?.data ?? []).map((dto: ProgramWeekDto) => ({
          id: String(dto.id ?? ''),
          programId: '',
          weekNumber: dto.weekNumber ?? 0,
          topic: dto.title ?? null,
          description: dto.description ?? null,
          startDate: '',
          endDate: '',
          videoName: dto.videoName ?? null,
          displayTitle: dto.displayTitle ?? null,
          videoDuration: dto.videoDuration ?? null,
          thumbnailPath: dto.thumbnailPath ?? null,
          createdAt: '',
          updatedAt: '',
        })),
      }))
    );
  }

  create(programId: string, value: Partial<ProgramWeek>): Observable<ProgramWeek> {
    const id = toId(programId);
    return this.svc.postProgramProgramWeek(id, value).pipe(
      map((res) => res as {} as ProgramWeek)
    );
  }

  update(programId: string, weekId: string, value: Partial<ProgramWeek>): Observable<ProgramWeek> {
    const id = toId(programId);
    return this.svc.putProgramProgramWeek(id, Number(weekId), value).pipe(
      map((res) => res as {} as ProgramWeek)
    );
  }

  delete(programId: string, weekId: string): Observable<void> {
    const id = toId(programId);
    return this.svc.deleteProgramProgramWeekVideo(id, Number(weekId)).pipe(
      map(() => void 0)
    );
  }
}
