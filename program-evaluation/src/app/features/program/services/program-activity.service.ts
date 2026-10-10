import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ProgramService as MonolithProgramService } from './ProgramService';
import { ProgramActivity } from '../models/program-activity.model';


@Injectable({ providedIn: 'root' })
export class ProgramActivityService {
  private readonly svc = inject(MonolithProgramService);

  listByWeek(
    weekId: string,
    params?: Record<string, string | number | boolean | null>
  ): Observable<{ items: ProgramActivity[] }> {
    return this.svc.getProgramProgramWeekAssignment(0, Number(weekId), 1, 100).pipe(
      map((res) => ({
        items: (res?.data ?? []) as {} as ProgramActivity[],
      }))
    );
  }

  create(weekId: string, value: Partial<ProgramActivity>): Observable<ProgramActivity> {
    return this.svc.postProgramProgramWeekAssignment(0, Number(weekId), value).pipe(
      map((res) => res as {} as ProgramActivity)
    );
  }

  update(id: string, value: Partial<ProgramActivity>): Observable<ProgramActivity> {
    return this.svc.putProgramProgramWeekAssignment(0, 0, Number(id), value).pipe(
      map((res) => res as {} as ProgramActivity)
    );
  }

  delete(id: string): Observable<void> {
    return this.svc.deleteProgramProgramWeekVideo(0, Number(id)).pipe(
      map(() => void 0)
    );
  }
}
