import { Component, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { catchError, forkJoin, of } from 'rxjs';
import { AuthService } from '../../core/auth.service';
import { ApiService } from '../../core/crud.service';
import { mensajeError } from '../../core/interceptors';

interface Fila {
  id: string;
  nombre: string;
  sesiones: number;
  completadas: number;
  promedio: number | null;
  fraudes: number;
  horasRealizadas: number;
  horasRequeridas: number;
  licencias: string;
}

@Component({
  selector: 'app-resultados',
  imports: [MatCardModule, MatIconModule, MatTableModule, MatProgressBarModule],
  template: `
    <h1>Resultados</h1>
    <p class="sub">{{ auth.tieneRol('Estudiante') ? 'Su avance y calificaciones.' : 'Desempeño de los estudiantes.' }}</p>

    @if (cargando()) { <mat-progress-bar mode="indeterminate" /> }

    <div class="kpis">
      @for (k of kpis(); track k.label) {
        <mat-card><mat-card-content class="kpi">
          <mat-icon>{{ k.icon }}</mat-icon>
          <div><div class="valor">{{ k.valor }}</div><div>{{ k.label }}</div></div>
        </mat-card-content></mat-card>
      }
    </div>

    <h2>{{ auth.tieneRol('Estudiante') ? 'Mi avance' : 'Por estudiante' }}</h2>
    <div class="tabla-wrap">
      <table mat-table [dataSource]="filas()">
        <ng-container matColumnDef="nombre"><th mat-header-cell *matHeaderCellDef>Estudiante</th><td mat-cell *matCellDef="let r">{{ r.nombre }}</td></ng-container>
        <ng-container matColumnDef="licencias"><th mat-header-cell *matHeaderCellDef>Licencia</th><td mat-cell *matCellDef="let r">{{ r.licencias || '—' }}</td></ng-container>
        <ng-container matColumnDef="sesiones"><th mat-header-cell *matHeaderCellDef>Sesiones</th><td mat-cell *matCellDef="let r">{{ r.completadas }} / {{ r.sesiones }}</td></ng-container>
        <ng-container matColumnDef="promedio"><th mat-header-cell *matHeaderCellDef>Calificación</th><td mat-cell *matCellDef="let r">{{ r.promedio != null ? r.promedio.toFixed(1) : '—' }}</td></ng-container>
        <ng-container matColumnDef="fraudes"><th mat-header-cell *matHeaderCellDef>Alertas antifraude</th><td mat-cell *matCellDef="let r">{{ r.fraudes }}</td></ng-container>
        <ng-container matColumnDef="horas">
          <th mat-header-cell *matHeaderCellDef>Horas</th>
          <td mat-cell *matCellDef="let r" class="horas">
            @if (r.horasRequeridas) {
              <mat-progress-bar mode="determinate" [value]="porcentaje(r)" />
              <small>{{ r.horasRealizadas }} / {{ r.horasRequeridas }} h ({{ porcentaje(r).toFixed(0) }}%)</small>
            } @else { — }
          </td>
        </ng-container>
        <tr mat-header-row *matHeaderRowDef="columnas"></tr>
        <tr mat-row *matRowDef="let row; columns: columnas"></tr>
        <tr class="mat-row" *matNoDataRow><td class="vacio" [attr.colspan]="columnas.length">Sin resultados</td></tr>
      </table>
    </div>
  `,
  styles: `
    .sub { opacity: .7; margin-top: -8px; }
    .kpis { display: grid; grid-template-columns: repeat(auto-fill, minmax(190px, 1fr)); gap: 16px; margin: 16px 0 8px; }
    .kpi { display: flex; align-items: center; gap: 14px; }
    .kpi mat-icon { font-size: 34px; width: 34px; height: 34px; color: var(--mat-sys-primary); }
    .valor { font-size: 26px; font-weight: 500; }
    .tabla-wrap { overflow-x: auto; }
    table { width: 100%; }
    .horas { min-width: 180px; }
    .horas small { opacity: .75; }
    .vacio { padding: 24px; text-align: center; opacity: .7; }
  `
})
export class ResultadosComponent {
  auth = inject(AuthService);
  private api = inject(ApiService);
  private snack = inject(MatSnackBar);

  columnas = ['nombre', 'licencias', 'sesiones', 'promedio', 'fraudes', 'horas'];
  cargando = signal(true);
  filas = signal<Fila[]>([]);

  kpis = computed(() => {
    const f = this.filas();
    const sesiones = f.reduce((a, x) => a + x.sesiones, 0);
    const completadas = f.reduce((a, x) => a + x.completadas, 0);
    const conNota = f.filter(x => x.promedio != null);
    const prom = conNota.length ? conNota.reduce((a, x) => a + x.promedio!, 0) / conNota.length : null;
    const hechas = f.reduce((a, x) => a + x.horasRealizadas, 0);
    const req = f.reduce((a, x) => a + x.horasRequeridas, 0);
    return [
      { icon: 'event_available', label: 'Sesiones completadas', valor: `${completadas} / ${sesiones}` },
      { icon: 'star', label: 'Calificación promedio', valor: prom != null ? prom.toFixed(1) : '—' },
      { icon: 'timer', label: 'Avance de horas', valor: req ? `${Math.round(hechas / req * 100)}%` : '—' },
      { icon: 'report', label: 'Alertas antifraude', valor: String(f.reduce((a, x) => a + x.fraudes, 0)) }
    ];
  });

  constructor() {
    const opcional = (path: string) => this.api.list(path).pipe(catchError(() => of([])));
    forkJoin({
      sesiones: this.api.list('sesiones-practicas'),
      horas: opcional('control-horas'),
      validaciones: opcional('validaciones-antifraude'),
      estudiantes: opcional('estudiantes')
    }).subscribe({
      next: r => { this.filas.set(this.armar(r)); this.cargando.set(false); },
      error: (e: HttpErrorResponse) => { this.cargando.set(false); this.snack.open(mensajeError(e), 'OK', { duration: 5000 }); }
    });
  }

  porcentaje(r: Fila) { return Math.min(100, r.horasRealizadas / r.horasRequeridas * 100); }

  private armar(r: { sesiones: any[]; horas: any[]; validaciones: any[]; estudiantes: any[] }): Fila[] {
    const nombres = new Map<string, string>(r.estudiantes.map(e => [e.id, `${e.nombre} ${e.apellidos}`]));
    const sesionEstudiante = new Map<string, string>(r.sesiones.map(s => [s.id, s.estudianteId]));
    const ids = new Set<string>([...r.sesiones.map(s => s.estudianteId), ...r.horas.map(h => h.estudianteId)]);
    const propio = this.auth.sesion()?.nombreCompleto ?? 'Yo';

    return [...ids].map(id => {
      const ses = r.sesiones.filter(s => s.estudianteId === id);
      const notas = ses.map(s => s.calificacionInstructor).filter((n: number | null) => n != null) as number[];
      const hs = r.horas.filter(h => h.estudianteId === id);
      return {
        id,
        nombre: nombres.get(id) ?? (this.auth.tieneRol('Estudiante') ? propio : id.slice(0, 8)),
        sesiones: ses.length,
        completadas: ses.filter(s => s.estadoPrueba === 'COMPLETADA').length,
        promedio: notas.length ? notas.reduce((a, n) => a + n, 0) / notas.length : null,
        fraudes: r.validaciones.filter(v => !v.estadoValidacion && sesionEstudiante.get(v.sesionId) === id).length,
        horasRealizadas: hs.reduce((a, h) => a + h.horasRealizadas, 0),
        horasRequeridas: hs.reduce((a, h) => a + h.horasRequeridas, 0),
        licencias: hs.map(h => h.tipoLicencia).join(', ')
      };
    }).sort((a, b) => a.nombre.localeCompare(b.nombre));
  }
}
