/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import { HttpClient, HttpHeaders } from '@angular/common/http';
import type { HttpResponse, HttpErrorResponse } from '@angular/common/http';
import { forkJoin, of, throwError } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import type { Observable } from 'rxjs';

import { ApiError } from './ApiError';
import type { ApiRequestOptions } from './ApiRequestOptions';
import type { ApiResult } from './ApiResult';
import type { OpenAPIConfig } from './OpenAPI';
export const isDefined = <T>(value: T | null | undefined): value is Exclude<T, null | undefined> => {
    return value !== undefined && value !== null;
};

export const isString = <T>(value: T): value is Extract<T, string> => {
    return typeof value === 'string';
};

export const isStringWithValue = <T>(value: T): value is Extract<T, string> => {
    return isString(value) && value !== '';
};

export const isBlob = (value: object | null | undefined): value is Blob => {
    if (typeof value !== 'object' || value === null) return false;
    const obj = value as { type?: string; stream?: () => void; arrayBuffer?: () => void; constructor?: { name?: string } };
    return (
        typeof obj.type === 'string' &&
        typeof obj.stream === 'function' &&
        typeof obj.arrayBuffer === 'function' &&
        typeof obj.constructor === 'object' &&
        obj.constructor !== null &&
        typeof obj.constructor.name === 'string' &&
        /^(Blob|File)$/.test(obj.constructor.name)
    );
};

export const isFormData = (value: object | null | undefined): value is FormData => {
    return value instanceof FormData;
};

export const base64 = (str: string): string => {
    try {
        return btoa(str);
    } catch (err) {
        // @ts-ignore
        return Buffer.from(str).toString('base64');
    }
};

export const getQueryString = (params: Record<string, string | number | boolean | null | undefined>): string => {
    const qs: string[] = [];

    const append = (key: string, value: string | number | boolean) => {
        qs.push(`${encodeURIComponent(key)}=${encodeURIComponent(String(value))}`);
    };

    const process = (key: string, value: string | number | boolean | null | undefined) => {
        if (isDefined(value)) {
            if (Array.isArray(value)) {
                (value as (string | number | boolean)[]).forEach(v => {
                    process(key, v);
                });
            } else if (typeof value === 'object' && value !== null) {
                Object.entries(value as Record<string, string | number | boolean | null | undefined>).forEach(([k, v]) => {
                    process(`${key}[${k}]`, v);
                });
            } else {
                append(key, value);
            }
        }
    };

    Object.entries(params).forEach(([key, value]) => {
        process(key, value);
    });

    if (qs.length > 0) {
        return `?${qs.join('&')}`;
    }

    return '';
};

const getUrl = (config: OpenAPIConfig, options: ApiRequestOptions): string => {
    const encoder = config.ENCODE_PATH || encodeURI;

    const path = options.url
        .replace('{api-version}', config.VERSION)
        .replace(/{(.*?)}/g, (substring: string, group: string) => {
            if (options.path?.hasOwnProperty(group)) {
                return encoder(String(options.path[group]));
            }
            return substring;
        });

    const url = `${config.BASE}${path}`;
    if (options.query) {
        return `${url}${getQueryString(options.query)}`;
    }
    return url;
};

export const getFormData = (options: ApiRequestOptions): FormData | undefined => {
    if (options.formData) {
        const formData = new FormData();

        const process = (key: string, value: string | number | boolean | null | Blob) => {
            if (value instanceof File) {
                formData.append(key, value, value.name);
            } else if (isString(value) || isBlob(value instanceof Blob ? value : null)) {
                formData.append(key, value as string | Blob);
            } else {
                formData.append(key, JSON.stringify(value));
            }
        };

        Object.entries(options.formData)
            .filter(([_, value]) => isDefined(value))
            .forEach(([key, value]) => {
                if (Array.isArray(value)) {
                    (value as (string | number | boolean | null | Blob)[]).forEach(v => process(key, v));
                } else {
                    process(key, value as string | number | boolean | null | Blob);
                }
            });

        return formData;
    }
    return undefined;
};

type Resolver<T> = (options: ApiRequestOptions) => Promise<T>;

export const resolve = async <T>(options: ApiRequestOptions, resolver?: T | Resolver<T>): Promise<T | undefined> => {
    if (typeof resolver === 'function') {
        return (resolver as Resolver<T>)(options);
    }
    return resolver;
};

export const getHeaders = (config: OpenAPIConfig, options: ApiRequestOptions): Observable<HttpHeaders> => {
    return forkJoin({
        token: resolve(options, config.TOKEN),
        username: resolve(options, config.USERNAME),
        password: resolve(options, config.PASSWORD),
        additionalHeaders: resolve(options, config.HEADERS),
    }).pipe(
        map(({ token, username, password, additionalHeaders }) => {
            const headers = Object.entries({
                Accept: 'application/json',
                ...additionalHeaders,
                ...options.headers,
            })
                .filter(([_, value]) => isDefined(value))
                .reduce((headers, [key, value]) => ({
                    ...headers,
                    [key]: String(value),
                }), {} as Record<string, string>);

            if (isStringWithValue(token)) {
                headers['Authorization'] = `Bearer ${token}`;
            }

            if (isStringWithValue(username) && isStringWithValue(password)) {
                const credentials = base64(`${username}:${password}`);
                headers['Authorization'] = `Basic ${credentials}`;
            }

            if (options.body !== undefined) {
                if (options.mediaType) {
                    headers['Content-Type'] = options.mediaType;
                } else if (isBlob(options.body instanceof Blob ? options.body : null)) {
                    headers['Content-Type'] = (options.body as Blob).type || 'application/octet-stream';
                } else if (isString(options.body)) {
                    headers['Content-Type'] = 'text/plain';
                } else if (!isFormData(options.body instanceof FormData ? options.body : null)) {
                    headers['Content-Type'] = 'application/json';
                }
            }

            if (options.mediaType === 'multipart/form-data' || options.formData || (options.body && isFormData(options.body instanceof FormData ? options.body : null)) || headers['Content-Type']?.includes('multipart/form-data')) {
                delete headers['Content-Type'];
            }

            return new HttpHeaders(headers);
        }),
    );
};

export const getRequestBody = (options: ApiRequestOptions): string | Blob | FormData | undefined => {
    if (options.body) {
        if (options.mediaType?.includes('/json')) {
            return JSON.stringify(options.body)
        } else if (isString(options.body) || isBlob(options.body instanceof Blob ? options.body : null) || isFormData(options.body instanceof FormData ? options.body : null)) {
            return options.body as string | Blob | FormData;
        } else {
            return JSON.stringify(options.body);
        }
    }
    return undefined;
};

export const sendRequest = <T>(
    config: OpenAPIConfig,
    options: ApiRequestOptions,
    http: HttpClient,
    url: string,
    body: string | Blob | FormData | undefined,
    formData: FormData | undefined,
    headers: HttpHeaders
): Observable<HttpResponse<T>> => {
    const requestOptions: Parameters<typeof http.request>[2] & { observe: 'response' } = {
        headers,
        body: body ?? formData,
        withCredentials: config.WITH_CREDENTIALS,
        observe: 'response',
        responseType: options.responseType,
    };
    return http.request(options.method, url, requestOptions) as Observable<HttpResponse<T>>;
};

export const getResponseHeader = <T>(response: HttpResponse<T>, responseHeader?: string): string | undefined => {
    if (responseHeader) {
        const value = response.headers.get(responseHeader);
        if (isString(value)) {
            return value;
        }
    }
    return undefined;
};

export const getResponseBody = <T>(response: HttpResponse<T>): T | undefined => {
    if (response.status !== 204 && response.body !== null) {
        return response.body;
    }
    return undefined;
};

export const catchErrorCodes = (options: ApiRequestOptions, result: ApiResult): void => {
    const errors: Record<number, string> = {
        400: 'Bad Request',
        401: 'Unauthorized',
        403: 'Forbidden',
        404: 'Not Found',
        500: 'Internal Server Error',
        502: 'Bad Gateway',
        503: 'Service Unavailable',
        ...options.errors,
    }

    const error = errors[result.status];
    if (error) {
        throw new ApiError(options, result, error);
    }

    if (!result.ok) {
        const errorStatus = result.status ?? 'n/a';
        const errorStatusText = result.statusText ?? 'n/a';
        const errorBody = (() => {
            try {
                return JSON.stringify(result.body, null, 2);
            } catch (e) {
                return undefined;
            }
        })();

        throw new ApiError(options, result,
            `Generic Error: status: ${errorStatus}; status text: ${errorStatusText}; body: ${errorBody}`
        );
    }
};

/**
 * Request method
 * @param config The OpenAPI configuration object
 * @param http The Angular HTTP client
 * @param options The request options from the service
 * @returns Observable<T>
 * @throws ApiError
 */
export const request = <T>(config: OpenAPIConfig, http: HttpClient, options: ApiRequestOptions): Observable<T> => {
    const url = getUrl(config, options);
    const formData = getFormData(options);
    const body = getRequestBody(options);

    return getHeaders(config, options).pipe(
        switchMap(headers => {
            return sendRequest<T>(config, options, http, url, formData, body as FormData | undefined, headers);
        }),
        map(response => {
            const responseBody = getResponseBody(response);
            const responseHeader = getResponseHeader(response, options.responseHeader);
            return {
                url,
                ok: response.ok,
                status: response.status,
                statusText: response.statusText,
                body: responseHeader ?? responseBody,
            } as ApiResult;
        }),
        catchError((error: HttpErrorResponse) => {
            if (!error.status) {
                return throwError(error);
            }
            return of({
                url,
                ok: error.ok,
                status: error.status,
                statusText: error.statusText,
                body: error.error ?? error.statusText,
            } as ApiResult);
        }),
        map(result => {
            catchErrorCodes(options, result);
            return result.body as T;
        }),
        catchError((error: ApiError) => {
            return throwError(error);
        }),
    );
};
