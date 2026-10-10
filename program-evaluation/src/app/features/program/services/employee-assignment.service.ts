import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, of, catchError } from 'rxjs';
import { ProgramService as MonolithProgramService } from './ProgramService';
import { EmployeeAssignmentDto } from '../models/EmployeeAssignmentDto';
import { SubmitEmployeeAssignmentResponseDto } from '../models/SubmitEmployeeAssignmentResponseDto';
import { EmployeeAssignmentDetailDto } from '../models/EmployeeAssignmentDetailDto';
import { CreateEmployeeAssignmentResponseDto } from '../models/CreateEmployeeAssignmentResponseDto';
import { EvaluateEmployeeAssignmentDto } from '../models/EvaluateEmployeeAssignmentDto';
import { OpenAPI } from '@core/api-gen/OpenAPI';

@Injectable({ providedIn: 'root' })
export class EmployeeAssignmentService {
  private readonly svc = inject(MonolithProgramService);
  private readonly http = inject(HttpClient);

  private get baseUrl(): string {
    return OpenAPI.BASE;
  }

  listByAssignment(programId: string, weekId: string, assignmentId: string, page: number = 1, limit: number = 100, search?: string): Observable<EmployeeAssignmentDto[]> {
    const queryParams: Record<string, string | number> = { pageNumber: page, pageSize: limit };
    if (search) queryParams['search'] = search;
    return this.http.get<{ data?: EmployeeAssignmentDto[] }>(
      `${this.baseUrl}/program/${Number(programId)}/program-week/${Number(weekId)}/assignment/${Number(assignmentId)}/employee-assignment`,
      { params: queryParams }
    ).pipe(
      map((res) => res?.data ?? [])
    );
  }

  getById(programId: string, weekId: string, assignmentId: string, empAssignmentId: string): Observable<EmployeeAssignmentDetailDto> {
    return this.svc.getProgramProgramWeekAssignmentEmployeeAssignment1(Number(programId), Number(weekId), Number(assignmentId), Number(empAssignmentId));
  }

  create(programId: string, weekId: string, assignmentId: string, employeeId?: string): Observable<EmployeeAssignmentDetailDto> {
    return this.svc.postProgramProgramWeekAssignmentEmployeeAssignment(Number(programId), Number(weekId), Number(assignmentId), employeeId).pipe(
      map((res) => res as {} as EmployeeAssignmentDetailDto)
    );
  }

  getMyAssignment(programId: string, weekId: string, assignmentId: string): Observable<EmployeeAssignmentDetailDto | null> {
    return this.svc.getProgramProgramWeekAssignmentEmployeeAssignmentMy(Number(programId), Number(weekId), Number(assignmentId)).pipe(
      catchError(() => of(null))
    );
  }

  createMyAssignment(programId: string, weekId: string, assignmentId: string): Observable<CreateEmployeeAssignmentResponseDto> {
    return this.svc.postProgramProgramWeekAssignmentEmployeeAssignmentMy(Number(programId), Number(weekId), Number(assignmentId));
  }

  submit(programId: string, weekId: string, assignmentId: string, empAssignmentId: string, file: Blob): Observable<SubmitEmployeeAssignmentResponseDto> {
    return this.svc.postProgramProgramWeekAssignmentEmployeeAssignmentSubmit(
      Number(programId), Number(weekId), Number(assignmentId), Number(empAssignmentId), { file }
    );
  }

  evaluate(programId: string, weekId: string, assignmentId: string, empAssignmentId: string, dto: EvaluateEmployeeAssignmentDto): Observable<EmployeeAssignmentDetailDto> {
    return this.svc.putProgramProgramWeekAssignmentEmployeeAssignmentEvaluate(Number(programId), Number(weekId), Number(assignmentId), Number(empAssignmentId), dto);
  }

  downloadFile(programId: string, weekId: string, assignmentId: string, empAssignmentId: string): Observable<Blob> {
    const url = `${this.baseUrl}/program/${Number(programId)}/program-week/${Number(weekId)}/assignment/${Number(assignmentId)}/employee-assignment/${Number(empAssignmentId)}/download`;
    return this.http.get(url, { responseType: 'blob' });
  }
}
