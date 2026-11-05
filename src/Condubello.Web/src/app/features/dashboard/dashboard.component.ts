import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { catchError, forkJoin, of } from 'rxjs';
import { AuthService } from '../../core/auth.service';
import { ApiService } from '../../core/crud.service';
import { MODULES } from '../crud/modules';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, MatCardModule, MatIconModule, MatProgressBarModule],
  template: `
    <h1>Hola, {{ auth.sesion()?.nombreCompleto }}</h1>
    <div class="grid">
      @for (m of modulos(); track m.path) {
        <a [routerLink]="['/', m.path]" class="enlace">
          <mat-card>
            <mat-card-content class="tarjeta">
              <mat-icon>{{ m.icon }}</mat-icon>
              <div>
                <div class="numero">{{ conteos()[m.path] ?? '…' }}</div>
                <div>{{ m.title }}</div>
              </div>
            </mat-card-content>
          </mat-card>
        </a>
      }
    </div>

    @if (horas().length) {
      <h2>Avance de horas</h2>
      @for (h of horas(); track h.id) {
        <p>Licencia {{ h.tipoLicencia }}: {{ h.horasRealizadas }} / {{ h.horasRequeridas }} h</p>
        <mat-progress-bar mode="determinate" [value]="Math.min(100, h.horasRealizadas / h.horasRequeridas * 100)" />
      }
    }
  `,
  styles: `
    .grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px; }
    .enlace { text-decoration: none; color: inherit; }
    .tarjeta { display: flex; align-items: center; gap: 16px; }
    .tarjeta mat-icon { font-size: 36px; width: 36px; height: 36px; color: var(--mat-sys-primary); }
    .numero { font-size: 28px; font-weight: 500; }
  `
})
export class DashboardComponent {
  auth = inject(AuthService);
  private api = inject(ApiService);
  Math = Math;

  modulos = computed(() => MODULES.filter(m => this.auth.tieneRol(...m.leer)));
  conteos = signal<Record<string, number | undefined>>({});
  horas = signal<any[]>([]);

  constructor() {
    const reqs: Record<string, any> = {};
    for (const m of this.modulos()) reqs[m.path] = this.api.list(m.path).pipe(catchError(() => of([])));
    forkJoin(reqs).subscribe(res => {
      this.conteos.set(Object.fromEntries(Object.entries(res).map(([k, v]) => [k, (v as any[]).length])));
      if (this.auth.tieneRol('Estudiante')) this.horas.set((res['control-horas'] ?? []) as any[]);
    });
  }
}
