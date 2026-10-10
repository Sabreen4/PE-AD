import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { apiConfig } from '@core/config/api.config';

export type QueryParams = Record<string, string | number | boolean | undefined | null>;

function toHttpParams(params?: QueryParams): HttpParams {
  let httpParams = new HttpParams();
  if (!params) return httpParams;
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      httpParams = httpParams.set(key, String(value));
    }
  });
  return httpParams;
}

@Injectable({ providedIn: 'root' })
export class BaseApiService {
  private http = inject(HttpClient);

  get<T>(path: string, params?: QueryParams): Observable<T> {
    return this.http.get<T>(`${apiConfig.gatewayUrl}${path}`, { params: toHttpParams(params) });
  }

  post<T>(path: string, body: string | number | boolean | null | Record<string, string | number | boolean | null> | Array<string | number | boolean | null>): Observable<T> {
    return this.http.post<T>(`${apiConfig.gatewayUrl}${path}`, body);
  }

  put<T>(path: string, body: string | number | boolean | null | Record<string, string | number | boolean | null> | Array<string | number | boolean | null>): Observable<T> {
    return this.http.put<T>(`${apiConfig.gatewayUrl}${path}`, body);
  }

  delete<T>(path: string): Observable<T> {
    return this.http.delete<T>(`${apiConfig.gatewayUrl}${path}`);
  }

  getBlob(path: string): Observable<Blob> {
    return this.http.get(`${apiConfig.gatewayUrl}${path}`, { responseType: 'blob' });
  }

  postFormData<T>(path: string, formData: FormData): Observable<T> {
    return this.http.post<T>(`${apiConfig.gatewayUrl}${path}`, formData);
  }
}