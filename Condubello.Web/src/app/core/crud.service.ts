import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_URL } from './config';
import { ApiResponse } from './models';

/** Cliente genérico para los endpoints REST de la API; desempaqueta ApiResponse. */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);

  list<T = any>(path: string): Observable<T[]> {
    return this.http.get<ApiResponse<T[]>>(`${API_URL}/api/${path}`).pipe(map(r => r.data ?? []));
  }
  get<T = any>(path: string): Observable<T> {
    return this.http.get<ApiResponse<T>>(`${API_URL}/api/${path}`).pipe(map(r => r.data));
  }
  create<T = any>(path: string, body: unknown): Observable<T> {
    return this.http.post<ApiResponse<T>>(`${API_URL}/api/${path}`, body).pipe(map(r => r.data));
  }
  update<T = any>(path: string, id: string, body: unknown): Observable<T> {
    return this.http.put<ApiResponse<T>>(`${API_URL}/api/${path}/${id}`, body).pipe(map(r => r.data));
  }
  patch<T = any>(path: string, body: unknown): Observable<T> {
    return this.http.patch<ApiResponse<T>>(`${API_URL}/api/${path}`, body).pipe(map(r => r.data));
  }
  put<T = any>(path: string, body: unknown): Observable<T> {
    return this.http.put<ApiResponse<T>>(`${API_URL}/api/${path}`, body).pipe(map(r => r.data));
  }
  remove(path: string, id: string): Observable<void> {
    return this.http.delete<ApiResponse<unknown>>(`${API_URL}/api/${path}/${id}`).pipe(map(() => undefined));
  }
}
