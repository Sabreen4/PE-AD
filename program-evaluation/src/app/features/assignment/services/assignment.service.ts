import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService, QueryParams } from '@core/services/base-api.service';
import { buildPath } from '@core/utils/path-utils';
import { AssignmentTemplate } from '@features/assignment/models/assignment-template.model';
import { EmployeeAssignment } from '@features/assignment/models/employee-assignment.model';
import { Submission } from '@features/assignment/models/submission.model';
import { PagedResult } from '@core/models/api-response.model';

const ASSIGNMENT_TEMPLATES = '/assignment/api/v1/assignment-template';
const ASSIGNMENT_TEMPLATE_BY_ID = '/assignment/api/v1/assignment-template/:id';
const EMPLOYEE_ASSIGNMENTS = '/assignment/api/v1/employee-assignment';
const EMPLOYEE_ASSIGNMENT_BY_ID = '/assignment/api/v1/employee-assignment/:id';
const SUBMISSIONS = '/assignment/api/v1/submission';
const SUBMISSION_BY_ID = '/assignment/api/v1/submission/:id';

@Injectable({ providedIn: 'root' })
export class AssignmentService {
  private api = inject(BaseApiService);

  listTemplates(params?: QueryParams): Observable<PagedResult<AssignmentTemplate>> {
    return this.api.get<PagedResult<AssignmentTemplate>>(ASSIGNMENT_TEMPLATES, params);
  }

  getTemplateById(id: string): Observable<AssignmentTemplate> {
    return this.api.get<AssignmentTemplate>(buildPath(ASSIGNMENT_TEMPLATE_BY_ID, { id }));
  }

  createTemplate(template: AssignmentTemplate): Observable<AssignmentTemplate> {
    return this.api.post<AssignmentTemplate>(ASSIGNMENT_TEMPLATES, template);
  }

  updateTemplate(id: string, template: Partial<AssignmentTemplate>): Observable<AssignmentTemplate> {
    return this.api.put<AssignmentTemplate>(buildPath(ASSIGNMENT_TEMPLATE_BY_ID, { id }), template);
  }

  deleteTemplate(id: string): Observable<void> {
    return this.api.delete<void>(buildPath(ASSIGNMENT_TEMPLATE_BY_ID, { id }));
  }

  listEmployeeAssignments(params?: QueryParams): Observable<PagedResult<EmployeeAssignment>> {
    return this.api.get<PagedResult<EmployeeAssignment>>(EMPLOYEE_ASSIGNMENTS, params);
  }

  getEmployeeAssignmentById(id: string): Observable<EmployeeAssignment> {
    return this.api.get<EmployeeAssignment>(buildPath(EMPLOYEE_ASSIGNMENT_BY_ID, { id }));
  }

  createEmployeeAssignment(assignment: EmployeeAssignment): Observable<EmployeeAssignment> {
    return this.api.post<EmployeeAssignment>(EMPLOYEE_ASSIGNMENTS, assignment);
  }

  updateEmployeeAssignment(id: string, assignment: Partial<EmployeeAssignment>): Observable<EmployeeAssignment> {
    return this.api.put<EmployeeAssignment>(buildPath(EMPLOYEE_ASSIGNMENT_BY_ID, { id }), assignment);
  }

  deleteEmployeeAssignment(id: string): Observable<void> {
    return this.api.delete<void>(buildPath(EMPLOYEE_ASSIGNMENT_BY_ID, { id }));
  }

  listSubmissions(params?: QueryParams): Observable<PagedResult<Submission>> {
    return this.api.get<PagedResult<Submission>>(SUBMISSIONS, params);
  }

  getSubmissionById(id: string): Observable<Submission> {
    return this.api.get<Submission>(buildPath(SUBMISSION_BY_ID, { id }));
  }

  createSubmission(submission: Submission): Observable<Submission> {
    return this.api.post<Submission>(SUBMISSIONS, submission);
  }

  updateSubmission(id: string, submission: Partial<Submission>): Observable<Submission> {
    return this.api.put<Submission>(buildPath(SUBMISSION_BY_ID, { id }), submission);
  }

  deleteSubmission(id: string): Observable<void> {
    return this.api.delete<void>(buildPath(SUBMISSION_BY_ID, { id }));
  }

  gradeSubmission(id: string, score: number, feedback: string): Observable<Submission> {
    return this.api.put<Submission>(buildPath(SUBMISSION_BY_ID, { id }), { score, feedback });
  }
}
