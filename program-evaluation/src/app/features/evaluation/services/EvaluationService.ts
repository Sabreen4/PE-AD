/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import type { Observable } from 'rxjs';
import type { AssessmentEvaluationDetailDto } from '../models/AssessmentEvaluationDetailDto';
import type { AttendanceCreatedDto } from '../models/AttendanceCreatedDto';
import type { AttendanceListResponseDto } from '../models/AttendanceListResponseDto';
import type { CreateAttendanceDto } from '../models/CreateAttendanceDto';
import type { CreateFeedbackDto } from '../models/CreateFeedbackDto';
import type { FeedbackCreatedDto } from '../models/FeedbackCreatedDto';
import type { FeedbackDto } from '../models/FeedbackDto';
import type { FeedbackListResponseDto } from '../models/FeedbackListResponseDto';
import type { UpdateFeedbackDto } from '../models/UpdateFeedbackDto';
import type { LeaderboardListResponseDto } from '../models/LeaderboardListResponseDto';
import type { CreateWeeklyEvaluationDto } from '../models/CreateWeeklyEvaluationDto';
import type { WeeklyEvaluationCreatedDto } from '../models/WeeklyEvaluationCreatedDto';
import type { WeeklyEvaluationListResponseDto } from '../models/WeeklyEvaluationListResponseDto';
import { OpenAPI } from '@core/api-gen/OpenAPI';
import { request as __request } from '@core/api-gen/request';

@Injectable({
    providedIn: 'root',
})
export class EvaluationService {
    constructor(public readonly http: HttpClient) {}

    // ---- Assessment Evaluation ----

    public getEmployeeAssignmentAssessmentEvaluation(employeeAssignmentId: number): Observable<AssessmentEvaluationDetailDto> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/employee-assignment/{employeeAssignmentId}/assessment-evaluation', path: { employeeAssignmentId },
        });
    }

    // ---- Attendance ----

    public getEnrollmentAttendance(enrollmentId: number, pageNumber: number = 1, pageSize: number = 10): Observable<AttendanceListResponseDto> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/enrollment/{enrollmentId}/attendance', path: { enrollmentId },
            query: { pageNumber, pageSize },
        });
    }

    public postEnrollmentAttendance(enrollmentId: number, requestBody?: CreateAttendanceDto): Observable<AttendanceCreatedDto> {
        return __request(OpenAPI, this.http, {
            method: 'POST', url: '/enrollment/{enrollmentId}/attendance', path: { enrollmentId },
            body: requestBody, mediaType: 'application/json',
        });
    }

    // ---- Feedback ----

    public getFeedbackByWeeklyEvaluation(weeklyEvaluationId: number, pageNumber: number = 1, pageSize: number = 10): Observable<FeedbackListResponseDto> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/feedback/by-weekly-evaluation/{weeklyEvaluationId}', path: { weeklyEvaluationId },
            query: { pageNumber, pageSize },
        });
    }

    public postFeedback(requestBody?: CreateFeedbackDto): Observable<FeedbackCreatedDto> {
        return __request(OpenAPI, this.http, {
            method: 'POST', url: '/feedback', body: requestBody, mediaType: 'application/json',
        });
    }

    public putFeedback(feedbackId: number, requestBody?: UpdateFeedbackDto): Observable<FeedbackDto> {
        return __request(OpenAPI, this.http, {
            method: 'PUT', url: '/feedback/{feedbackId}', path: { feedbackId }, body: requestBody, mediaType: 'application/json',
        });
    }

    public deleteFeedback(feedbackId: number): Observable<void> {
        return __request(OpenAPI, this.http, {
            method: 'DELETE', url: '/feedback/{feedbackId}', path: { feedbackId },
        });
    }

    // ---- Leaderboard ----

    public getProgramLeaderboard(programId: number, pageNumber: number = 1, pageSize: number = 10): Observable<LeaderboardListResponseDto> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/program/{programId}/leaderboard', path: { programId },
            query: { pageNumber, pageSize },
        });
    }

    public postProgramLeaderboardRecalculate(programId: number): Observable<LeaderboardListResponseDto> {
        return __request(OpenAPI, this.http, {
            method: 'POST', url: '/program/{programId}/leaderboard/recalculate', path: { programId },
        });
    }

    // ---- Weekly Evaluation ----

    public getEnrollmentWeeklyEvaluation(enrollmentId: number, pageNumber: number = 1, pageSize: number = 10): Observable<WeeklyEvaluationListResponseDto> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/enrollment/{enrollmentId}/weekly-evaluation', path: { enrollmentId },
            query: { pageNumber, pageSize },
        });
    }

    public getEnrollmentWeeklyEvaluationAttendanceScore(enrollmentId: number, programWeekId: number): Observable<number> {
        return __request(OpenAPI, this.http, {
            method: 'GET', url: '/enrollment/{enrollmentId}/weekly-evaluation/{programWeekId}/attendance-score',
            path: { enrollmentId, programWeekId },
        });
    }

    public postEnrollmentWeeklyEvaluation(enrollmentId: number, requestBody?: CreateWeeklyEvaluationDto): Observable<WeeklyEvaluationCreatedDto> {
        return __request(OpenAPI, this.http, {
            method: 'POST', url: '/enrollment/{enrollmentId}/weekly-evaluation', path: { enrollmentId },
            body: requestBody, mediaType: 'application/json',
        });
    }
}