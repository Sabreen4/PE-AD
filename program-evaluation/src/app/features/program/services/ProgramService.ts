/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import type { Observable } from 'rxjs';
import type { AssignmentCreatedDto } from '../models/AssignmentCreatedDto';
import type { AssignmentDetailDto } from '../models/AssignmentDetailDto';
import type { AssignmentDtoPagedResponseDto } from '../models/AssignmentDtoPagedResponseDto';
import type { CreateAssignmentDto } from '../models/CreateAssignmentDto';
import type { UpdateAssignmentDto } from '../models/UpdateAssignmentDto';
import type { CreateEmployeeAssignmentResponseDto } from '../models/CreateEmployeeAssignmentResponseDto';
import type { EmployeeAssignmentDetailDto } from '../models/EmployeeAssignmentDetailDto';
import type { EmployeeAssignmentDtoPagedResponseDto } from '../models/EmployeeAssignmentDtoPagedResponseDto';
import type { EvaluateEmployeeAssignmentDto } from '../models/EvaluateEmployeeAssignmentDto';
import type { SubmitEmployeeAssignmentResponseDto } from '../models/SubmitEmployeeAssignmentResponseDto';
import type { EnrollmentCreatedDto } from '../models/EnrollmentCreatedDto';
import type { EnrollmentDetailDto } from '../models/EnrollmentDetailDto';
import type { EnrollmentDtoPagedResponseDto } from '../models/EnrollmentDtoPagedResponseDto';
import type { CreateProgramDto } from '../models/CreateProgramDto';
import type { ProgramCreatedDto } from '../models/ProgramCreatedDto';
import type { ProgramDetailDto } from '../models/ProgramDetailDto';
import type { ProgramDtoPagedResponseDto } from '../models/ProgramDtoPagedResponseDto';
import type { UpdateProgramDto } from '../models/UpdateProgramDto';
import type { CreateProgramWeekDto } from '../models/CreateProgramWeekDto';
import type { ProgramWeekCreatedDto } from '../models/ProgramWeekCreatedDto';
import type { ProgramWeekDetailDto } from '../models/ProgramWeekDetailDto';
import type { ProgramWeekDtoPagedResponseDto } from '../models/ProgramWeekDtoPagedResponseDto';
import type { UpdateProgramWeekDto } from '../models/UpdateProgramWeekDto';
import type { VideoMetadataDto } from '../models/VideoMetadataDto';
import type { VideoResponseDto } from '../models/VideoResponseDto';
import { OpenAPI } from '@core/api-gen/OpenAPI';
import { request as __request } from '@core/api-gen/request';

@Injectable({
    providedIn: 'root',
})
export class ProgramService {
    constructor(public readonly http: HttpClient) {}

    // ---- Program ----

    public getProgram(pageNumber: number = 1, pageSize: number = 10): Observable<ProgramDtoPagedResponseDto> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/program',
            query: { pageNumber, pageSize },
        });
    }

    public postProgram(requestBody?: CreateProgramDto): Observable<ProgramCreatedDto> {
        return __request(OpenAPI, this.http, {
            method: 'POST', url: '/program', body: requestBody, mediaType: 'application/json',
        });
    }

    public getProgram1(programId: number): Observable<ProgramDetailDto> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/program/{programId}', path: { programId },
        });
    }

    public putProgram(programId: number, requestBody?: UpdateProgramDto): Observable<ProgramDetailDto> {
        return __request(OpenAPI, this.http, {
            method: 'PUT', url: '/program/{programId}', path: { programId }, body: requestBody, mediaType: 'application/json',
        });
    }

    public postProgramPublish(programId: number): Observable<ProgramDetailDto> {
        return __request(OpenAPI, this.http, {
            method: 'POST', url: '/program/{programId}/publish', path: { programId },
        });
    }

    // ---- Program Week ----

    public getProgramProgramWeek(programId: number, pageNumber: number = 1, pageSize: number = 10): Observable<ProgramWeekDtoPagedResponseDto> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/program/{programId}/program-week', path: { programId },
            query: { pageNumber, pageSize },
        });
    }

    public postProgramProgramWeek(programId: number, requestBody?: CreateProgramWeekDto): Observable<ProgramWeekCreatedDto> {
        return __request(OpenAPI, this.http, {
            method: 'POST', url: '/program/{programId}/program-week', path: { programId }, body: requestBody, mediaType: 'application/json',
        });
    }

    public getProgramProgramWeek1(programId: number, weekId: number): Observable<ProgramWeekDetailDto> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/program/{programId}/program-week/{weekId}', path: { programId, weekId },
        });
    }

    public putProgramProgramWeek(programId: number, weekId: number, requestBody?: UpdateProgramWeekDto): Observable<ProgramWeekDetailDto> {
        return __request(OpenAPI, this.http, {
            method: 'PUT', url: '/program/{programId}/program-week/{weekId}', path: { programId, weekId }, body: requestBody, mediaType: 'application/json',
        });
    }

    public postProgramProgramWeekVideo(programId: number, weekId: number, formData?: { file?: Blob }): Observable<VideoResponseDto> {
        return __request(OpenAPI, this.http, {
            method: 'POST', url: '/program/{programId}/program-week/{weekId}/video', path: { programId, weekId },
            formData: formData, mediaType: 'multipart/form-data',
        });
    }

    public getProgramProgramWeekVideo(programId: number, weekId: number): Observable<Blob> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/program/{programId}/program-week/{weekId}/video', path: { programId, weekId },
            responseType: 'blob',
        });
    }

    public deleteProgramProgramWeekVideo(programId: number, weekId: number): Observable<void> {
        return __request(OpenAPI, this.http, {
            method: 'DELETE', url: '/program/{programId}/program-week/{weekId}/video', path: { programId, weekId },
        });
    }

    public getProgramProgramWeekVideoDetails(programId: number, weekId: number): Observable<VideoMetadataDto> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/program/{programId}/program-week/{weekId}/video/details', path: { programId, weekId },
        });
    }

    // ---- Assignment ----

    public getProgramProgramWeekAssignment(programId: number, weekId: number, pageNumber: number = 1, pageSize: number = 10): Observable<AssignmentDtoPagedResponseDto> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/program/{programId}/program-week/{weekId}/assignment', path: { programId, weekId },
            query: { pageNumber, pageSize },
        });
    }

    public postProgramProgramWeekAssignment(programId: number, weekId: number, requestBody?: CreateAssignmentDto): Observable<AssignmentCreatedDto> {
        return __request(OpenAPI, this.http, {
            method: 'POST', url: '/program/{programId}/program-week/{weekId}/assignment', path: { programId, weekId },
            body: requestBody, mediaType: 'application/json',
        });
    }

    public getProgramProgramWeekAssignment1(programId: number, weekId: number, assignmentId: number): Observable<AssignmentDetailDto> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/program/{programId}/program-week/{weekId}/assignment/{assignmentId}', path: { programId, weekId, assignmentId },
        });
    }

    public putProgramProgramWeekAssignment(programId: number, weekId: number, assignmentId: number, requestBody?: UpdateAssignmentDto): Observable<AssignmentDetailDto> {
        return __request(OpenAPI, this.http, {
            method: 'PUT', url: '/program/{programId}/program-week/{weekId}/assignment/{assignmentId}', path: { programId, weekId, assignmentId },
            body: requestBody, mediaType: 'application/json',
        });
    }

    // ---- Employee Assignment ----

    public getProgramProgramWeekAssignmentEmployeeAssignment(programId: number, weekId: number, assignmentId: number, pageNumber: number = 1, pageSize: number = 10): Observable<EmployeeAssignmentDtoPagedResponseDto> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/program/{programId}/program-week/{weekId}/assignment/{assignmentId}/employee-assignment', path: { programId, weekId, assignmentId },
            query: { pageNumber, pageSize },
        });
    }

    public postProgramProgramWeekAssignmentEmployeeAssignment(programId: number, weekId: number, assignmentId: number, employeeId?: string): Observable<CreateEmployeeAssignmentResponseDto> {
        return __request(OpenAPI, this.http, {
            method: 'POST', url: '/program/{programId}/program-week/{weekId}/assignment/{assignmentId}/employee-assignment', path: { programId, weekId, assignmentId },
            query: { employeeId },
        });
    }

    public getProgramProgramWeekAssignmentEmployeeAssignment1(programId: number, weekId: number, assignmentId: number, employeeAssignmentId: number): Observable<EmployeeAssignmentDetailDto> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/program/{programId}/program-week/{weekId}/assignment/{assignmentId}/employee-assignment/{employeeAssignmentId}', path: { programId, weekId, assignmentId, employeeAssignmentId },
        });
    }

    public postProgramProgramWeekAssignmentEmployeeAssignmentSubmit(programId: number, weekId: number, assignmentId: number, employeeAssignmentId: number, formData?: { file?: Blob }): Observable<SubmitEmployeeAssignmentResponseDto> {
        return __request(OpenAPI, this.http, {
            method: 'POST', url: '/program/{programId}/program-week/{weekId}/assignment/{assignmentId}/employee-assignment/{employeeAssignmentId}/submit', path: { programId, weekId, assignmentId, employeeAssignmentId },
            formData: formData, mediaType: 'multipart/form-data',
        });
    }

    public putProgramProgramWeekAssignmentEmployeeAssignmentEvaluate(programId: number, weekId: number, assignmentId: number, employeeAssignmentId: number, requestBody?: EvaluateEmployeeAssignmentDto): Observable<EmployeeAssignmentDetailDto> {
        return __request(OpenAPI, this.http, {
            method: 'PUT', url: '/program/{programId}/program-week/{weekId}/assignment/{assignmentId}/employee-assignment/{employeeAssignmentId}/evaluate', path: { programId, weekId, assignmentId, employeeAssignmentId },
            body: requestBody, mediaType: 'application/json',
        });
    }

    public getProgramProgramWeekAssignmentEmployeeAssignmentMy(programId: number, weekId: number, assignmentId: number): Observable<EmployeeAssignmentDetailDto> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/program/{programId}/program-week/{weekId}/assignment/{assignmentId}/employee-assignment/my', path: { programId, weekId, assignmentId },
        });
    }

    public postProgramProgramWeekAssignmentEmployeeAssignmentMy(programId: number, weekId: number, assignmentId: number): Observable<CreateEmployeeAssignmentResponseDto> {
        return __request(OpenAPI, this.http, {
            method: 'POST', url: '/program/{programId}/program-week/{weekId}/assignment/{assignmentId}/employee-assignment/my', path: { programId, weekId, assignmentId },
        });
    }

    // ---- Enrollment ----

    public getProgramEnrollment(programId: number, pageNumber: number = 1, pageSize: number = 10): Observable<EnrollmentDtoPagedResponseDto> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/program/{programId}/enrollment', path: { programId },
            query: { pageNumber, pageSize },
        });
    }

    public postProgramEnrollment(programId: number, employeeId?: string): Observable<EnrollmentCreatedDto> {
        return __request(OpenAPI, this.http, {
            method: 'POST', url: '/program/{programId}/enrollment', path: { programId },
            query: { employeeId },
        });
    }

    public getProgramEnrollment1(programId: number, enrollmentId: number): Observable<EnrollmentDetailDto> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/program/{programId}/enrollment/{enrollmentId}', path: { programId, enrollmentId },
        });
    }

    public deleteProgramEnrollment(programId: number, enrollmentId: number): Observable<void> {
        return __request(OpenAPI, this.http, {
            method: 'DELETE', url: '/program/{programId}/enrollment/{enrollmentId}', path: { programId, enrollmentId },
        });
    }

    public getProgramEnrollmentMy(programId: number): Observable<EnrollmentDetailDto> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/program/{programId}/enrollment/my-enrollment', path: { programId },
        });
    }
}