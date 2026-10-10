/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
export type ApiRequestOptions = {
    readonly method: 'GET' | 'PUT' | 'POST' | 'DELETE' | 'OPTIONS' | 'HEAD' | 'PATCH';
    readonly url: string;
    readonly path?: Record<string, string | number>;
    readonly cookies?: Record<string, string>;
    readonly headers?: Record<string, string>;
    readonly query?: Record<string, string | number | boolean | null | undefined>;
    readonly formData?: Record<string, string | number | boolean | null | Blob | undefined>;
    readonly body?: string | number | boolean | null | Record<string, string | number | boolean | null> | Array<string | number | boolean | null> | Blob | FormData;
    readonly mediaType?: string;
    readonly responseHeader?: string;
    readonly responseType?: 'json' | 'blob' | 'text' | 'arraybuffer';
    readonly errors?: Record<number, string>;
};
