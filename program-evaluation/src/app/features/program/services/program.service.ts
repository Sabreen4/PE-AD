import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ProgramService as MonolithProgramService } from './ProgramService';
import { Program } from '../models/program.model';
import { PaginatedResponse } from '@core/models/paginated-response.model';
import { ProgramDtoPagedResponseDto } from '../models/ProgramDtoPagedResponseDto';
import { ProgramDto } from '../models/ProgramDto';
import { UpdateProgramDto } from '../models/UpdateProgramDto';
import { ProgramCreatedDto } from '../models/ProgramCreatedDto';
import { ProgramDetailDto } from '../models/ProgramDetailDto';
import { OpenAPI } from '@core/api-gen/OpenAPI';


function toId(value: string): number {
  const n = Number(value);
  return isNaN(n) ? 0 : n;
}

@Injectable({ providedIn: 'root' })
export class ProgramService {
  private readonly svc = inject(MonolithProgramService);
  private readonly http = inject(HttpClient);

  list(params?: Record<string, string | number | boolean | null>): Observable<PaginatedResponse<Program>> {
    const page: number = (params?.['page'] as number) ?? 1;
    const limit: number = (params?.['limit'] as number) ?? 100;
    const search = params?.['search'] as string | undefined;

    const queryParams: Record<string, string | number> = { pageNumber: page, pageSize: limit };
    if (search) queryParams['search'] = search;

    return this.http.get<ProgramDtoPagedResponseDto>(`${OpenAPI.BASE}/program`, { params: queryParams }).pipe(
      map((res: ProgramDtoPagedResponseDto) => ({
        items: (res?.data ?? []).map((item: ProgramDto) => ({
          ...item,
          id: String(item.id),
          isActive: (item as ProgramDto & { isActive?: boolean }).isActive !== undefined ? Boolean((item as ProgramDto & { isActive?: boolean }).isActive) : (item.status ? item.status.toLowerCase() === 'active' : true)
        })) as unknown as Program[],
        total: res?.pagination?.totalRecords,
        page: res?.pagination?.page,
        limit: res?.pagination?.pageSize,
        totalPages: res?.pagination?.totalPages,
      }))
    );
  }

  getById(id: string): Observable<Program> {
    const numId = toId(id);
    return this.svc.getProgram1(numId).pipe(
      map((res) => ({ ...(res as {} as Program), id }))
    );
  }

  create(value: Partial<Program>): Observable<Program> {
    return this.svc.postProgram(value).pipe(
      map((res: ProgramCreatedDto) => ({ ...(res ?? {}), id: String(res?.id ?? '') } as {} as Program))
    );
  }

  update(id: string, value: Partial<Program>): Observable<Program> {
    const numId = toId(id);
    return this.svc.putProgram(numId, value).pipe(
      map((res: ProgramDetailDto) => ({ ...(res ?? {}), id } as {} as Program))
    );
  }

  deactivate(id: string): Observable<void> {
    const dto: UpdateProgramDto = { status: 'Inactive' };
    return this.svc.putProgram(toId(id), dto).pipe(
      map(() => void 0)
    );
  }

  activate(id: string): Observable<void> {
    return this.svc.postProgramPublish(toId(id)).pipe(
      map(() => void 0)
    );
  }
}
