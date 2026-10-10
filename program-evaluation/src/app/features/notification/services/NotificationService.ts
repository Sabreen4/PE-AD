/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import type { Observable } from 'rxjs';
import type { SendOtpEmailRequestDto } from '../models/SendOtpEmailRequestDto';
import type { SendRegistrationEmailRequestDto } from '../models/SendRegistrationEmailRequestDto';
import { OpenAPI } from '@core/api-gen/OpenAPI';
import { request as __request } from '@core/api-gen/request';
@Injectable({
    providedIn: 'root',
})
export class NotificationService {
    constructor(public readonly http: HttpClient) {}
    /**
     * @param isRead
     * @param pageNumber
     * @param pageSize
     * @returns any OK
     * @throws ApiError
     */
    public getNotifications(
        isRead?: boolean,
        pageNumber: number = 1,
        pageSize: number = 10,
    ): Observable<Record<string, string | number | boolean | null>> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/notifications',
            query: {
                'isRead': isRead,
                'pageNumber': pageNumber,
                'pageSize': pageSize,
            },
        });
    }
    /**
     * @param notificationId
     * @returns any OK
     * @throws ApiError
     */
    public getNotifications1(
        notificationId: number,
    ): Observable<Record<string, string | number | boolean | null>> {
        return __request(OpenAPI, this.http, {
            method: 'GET',
            url: '/notifications/{notificationId}',
            path: {
                'notificationId': notificationId,
            },
        });
    }
    /**
     * @param notificationId
     * @returns any OK
     * @throws ApiError
     */
    public deleteNotifications(
        notificationId: number,
    ): Observable<void> {
        return __request(OpenAPI, this.http, {
            method: 'DELETE',
            url: '/notifications/{notificationId}',
            path: {
                'notificationId': notificationId,
            },
        });
    }
    /**
     * @param notificationId
     * @returns any OK
     * @throws ApiError
     */
    public putNotificationsMarkAsRead(
        notificationId: number,
    ): Observable<void> {
        return __request(OpenAPI, this.http, {
            method: 'PUT',
            url: '/notifications/{notificationId}/mark-as-read',
            path: {
                'notificationId': notificationId,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public putNotificationsMarkAllAsRead(): Observable<void> {
        return __request(OpenAPI, this.http, {
            method: 'PUT',
            url: '/notifications/mark-all-as-read',
        });
    }
    /**
     * @param requestBody
     * @returns any OK
     * @throws ApiError
     */
    public postNotificationsSendRegistrationEmail(
        requestBody?: SendRegistrationEmailRequestDto,
    ): Observable<void> {
        return __request(OpenAPI, this.http, {
            method: 'POST',
            url: '/notifications/send-registration-email',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @param requestBody
     * @returns any OK
     * @throws ApiError
     */
    public postNotificationsSendOtpEmail(
        requestBody?: SendOtpEmailRequestDto,
    ): Observable<void> {
        return __request(OpenAPI, this.http, {
            method: 'POST',
            url: '/notifications/send-otp-email',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
}
