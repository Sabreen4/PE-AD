import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ProgramService as MonolithProgramService } from './ProgramService';
import { AssignmentDto } from '../models/AssignmentDto';
import { AssignmentDetailDto } from '../models/AssignmentDetailDto';
import { CreateAssignmentDto } from '../models/CreateAssignmentDto';
import { UpdateAssignmentDto } from '../models/UpdateAssignmentDto';

@Injectable({ providedIn: 'root' })
export class AssignmentService {
  private readonly svc = inject(MonolithProgramService);

  listByWeek(programId: string, weekId: string, page: number = 1, limit: number = 100): Observable<AssignmentDto[]> {
    return this.svc.getProgramProgramWeekAssignment(Number(programId), Number(weekId), page, limit).pipe(
      map((res) => res?.data ?? [])
    );
  }

  getById(programId: string, weekId: string, assignmentId: string): Observable<AssignmentDetailDto> {
    return this.svc.getProgramProgramWeekAssignment1(Number(programId), Number(weekId), Number(assignmentId));
  }

  create(programId: string, weekId: string, dto: CreateAssignmentDto): Observable<AssignmentDetailDto> {
    return this.svc.postProgramProgramWeekAssignment(Number(programId), Number(weekId), dto).pipe(
      map((res) => res as {} as AssignmentDetailDto)
    );
  }

  update(programId: string, weekId: string, assignmentId: string, dto: UpdateAssignmentDto): Observable<AssignmentDetailDto> {
    return this.svc.putProgramProgramWeekAssignment(Number(programId), Number(weekId), Number(assignmentId), dto);
  }
}
