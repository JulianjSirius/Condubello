import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { map, tap } from 'rxjs';
import { ApiResponse, LoginResponse, Rol, Sesion } from './models';

const KEY = 'condubello.sesion';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  readonly sesion = signal<Sesion | null>(this.cargar());
  readonly rol = computed(() => this.sesion()?.rol ?? null);
  readonly autenticado = computed(() => {
    const s = this.sesion();
    return !!s && new Date(s.expiraEn).getTime() > Date.now();
  });

  get token(): string | null {
    return this.autenticado() ? this.sesion()!.token : null;
  }

  login(email: string, password: string) {
    return this.http.post<ApiResponse<LoginResponse>>('/api/auth/login', { email, password }).pipe(
      map(r => r.data),
      tap(s => {
        this.sesion.set(s);
        try { localStorage.setItem(KEY, JSON.stringify(s)); } catch { /* sin storage */ }
      })
    );
  }

  logout() {
    this.sesion.set(null);
    try { localStorage.removeItem(KEY); } catch { /* sin storage */ }
    this.router.navigate(['/login']);
  }

  tieneRol(...roles: Rol[]): boolean {
    const r = this.rol();
    return !!r && roles.includes(r);
  }

  private cargar(): Sesion | null {
    try {
      const raw = localStorage.getItem(KEY);
      return raw ? JSON.parse(raw) as Sesion : null;
    } catch { return null; }
  }
}
