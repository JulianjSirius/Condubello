import { Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { catchError, forkJoin, of } from 'rxjs';
import { AuthService } from '../../core/auth.service';
import { ApiService } from '../../core/crud.service';
import { mensajeError } from '../../core/interceptors';
import { ESTADOS_PRUEBA } from '../../core/models';
import { FormDialogComponent } from '../crud/form-dialog.component';

@Component({
  selector: 'app-actividades',
  imports: [DatePipe, MatTableModule, MatChipsModule, MatButtonModule, MatIconModule, MatProgressBarModule, MatTooltipModule],
  template: `
    <h1>Actividades</h1>
    <p class="sub">Sesiones prácticas {{ auth.tieneRol('Estudiante') ? 'programadas para usted' : 'de la escuela' }}.</p>

    <mat-chip-listbox class="filtros" aria-label="Filtrar por estado" (change)="estado.set($event.value)">
      <mat-chip-option [value]="null" [selected]="estado() === null">Todas ({{ datos().length }})</mat-chip-option>
      @for (e of estados; track e) {
        <mat-chip-option [value]="e">{{ etiqueta(e) }} ({{ conteo(e) }})</mat-chip-option>
      }
    </mat-chip-listbox>

    @if (cargando()) { <mat-progress-bar mode="indeterminate" /> }

    <div class="tabla-wrap">
      <table mat-table [dataSource]="filas()">
        <ng-container matColumnDef="fecha">
          <th mat-header-cell *matHeaderCellDef>Fecha</th>
          <td mat-cell *matCellDef="let r">{{ r.fechaHoraInicio | date: 'dd/MM/yyyy HH:mm' }}</td>
        </ng-container>
        <ng-container matColumnDef="estudiante">
          <th mat-header-cell *matHeaderCellDef>Estudiante</th>
          <td mat-cell *matCellDef="let r">{{ nombre(estudiantes(), r.estudianteId) }}</td>
        </ng-container>
        <ng-container matColumnDef="instructor">
          <th mat-header-cell *matHeaderCellDef>Instructor</th>
          <td mat-cell *matCellDef="let r">{{ nombre(instructores(), r.instructorId) }}</td>
        </ng-container>
        <ng-container matColumnDef="duracion">
          <th mat-header-cell *matHeaderCellDef>Duración</th>
          <td mat-cell *matCellDef="let r">{{ r.duracionHoras != null ? r.duracionHoras + ' h' : '—' }}</td>
        </ng-container>
        <ng-container matColumnDef="estado">
          <th mat-header-cell *matHeaderCellDef>Estado</th>
          <td mat-cell *matCellDef="let r"><span class="chip" [class]="'e-' + r.estadoPrueba">{{ etiqueta(r.estadoPrueba) }}</span></td>
        </ng-container>
        <ng-container matColumnDef="acciones">
          <th mat-header-cell *matHeaderCellDef></th>
          <td mat-cell *matCellDef="let r" class="acciones">
            @if (puedeSeguimiento()) {
              <button mat-icon-button matTooltip="Seguimiento" (click)="seguimiento(r)"><mat-icon>fact_check</mat-icon></button>
            }
          </td>
        </ng-container>
        <tr mat-header-row *matHeaderRowDef="columnas()"></tr>
        <tr mat-row *matRowDef="let row; columns: columnas()"></tr>
        <tr class="mat-row" *matNoDataRow><td class="vacio" [attr.colspan]="columnas().length">No hay actividades para mostrar</td></tr>
      </table>
    </div>
  `,
  styles: `
    .sub { opacity: .7; margin-top: -8px; }
    .filtros { margin-bottom: 16px; }
    .tabla-wrap { overflow-x: auto; }
    table { width: 100%; }
    .acciones { text-align: right; white-space: nowrap; }
    .vacio { padding: 24px; text-align: center; opacity: .7; }
    .chip { padding: 2px 10px; border-radius: 12px; font-size: 12px; white-space: nowrap; background: var(--mat-sys-secondary-container); }
    .e-EN_CURSO { background: #fff3cd; color: #664d03; }
    .e-COMPLETADA { background: #d1e7dd; color: #0a3622; }
    .e-CANCELADA { background: #e2e3e5; color: #41464b; }
    .e-FRAUDE_DETECTADO { background: #f8d7da; color: #58151c; }
  `
})
export class ActividadesComponent {
  auth = inject(AuthService);
  private api = inject(ApiService);
  private dialog = inject(MatDialog);
  private snack = inject(MatSnackBar);

  estados = ESTADOS_PRUEBA;
  datos = signal<any[]>([]);
  estudiantes = signal<Record<string, string>>({});
  instructores = signal<Record<string, string>>({});
  estado = signal<string | null>(null);
  cargando = signal(false);

  puedeSeguimiento = computed(() => this.auth.tieneRol('Administrativo', 'Instructor'));
  columnas = computed(() => ['fecha', 'estudiante', 'instructor', 'duracion', 'estado', ...(this.puedeSeguimiento() ? ['acciones'] : [])]);
  filas = computed(() => {
    const e = this.estado();
    return [...this.datos()]
      .filter(r => !e || r.estadoPrueba === e)
      .sort((a, b) => new Date(b.fechaHoraInicio).getTime() - new Date(a.fechaHoraInicio).getTime());
  });

  constructor() { this.cargar(); }

  cargar() {
    this.cargando.set(true);
    // Estudiantes e instructores solo los puede listar el personal; para un estudiante se ignora el error.
    const opcional = (path: string) => this.api.list(path).pipe(catchError(() => of([])));
    forkJoin({ sesiones: this.api.list('sesiones-practicas'), est: opcional('estudiantes'), ins: opcional('instructores') }).subscribe({
      next: ({ sesiones, est, ins }) => {
        const mapa = (l: any[]) => Object.fromEntries(l.map(x => [x.id, `${x.nombre} ${x.apellidos}`]));
        this.datos.set(sesiones);
        this.estudiantes.set(mapa(est));
        this.instructores.set(mapa(ins));
        this.cargando.set(false);
      },
      error: (e: HttpErrorResponse) => { this.cargando.set(false); this.snack.open(mensajeError(e), 'OK', { duration: 5000 }); }
    });
  }

  conteo(e: string) { return this.datos().filter(r => r.estadoPrueba === e).length; }
  etiqueta(e: string) { return (e ?? '').replaceAll('_', ' ').toLowerCase().replace(/^./, c => c.toUpperCase()); }
  nombre(mapa: Record<string, string>, id: string) { return mapa[id] ?? '—'; }

  seguimiento(r: any) {
    this.dialog.open(FormDialogComponent, {
      maxHeight: '90vh',
      data: {
        title: 'Seguimiento de la sesión',
        value: r,
        fields: [
          { key: 'fechaHoraInicio', label: 'Inicio', type: 'datetime', required: true },
          { key: 'fechaHoraFin', label: 'Fin', type: 'datetime' },
          { key: 'estadoPrueba', label: 'Estado', type: 'select', options: ESTADOS_PRUEBA },
          { key: 'calificacionInstructor', label: 'Calificación (1-5)', type: 'number', min: 1, max: 5 }
        ],
        submit: (b: any) => this.api.patch(`sesiones-practicas/${r.id}/seguimiento`, b)
      }
    }).afterClosed().subscribe(ok => { if (ok) { this.snack.open('Seguimiento guardado', 'OK', { duration: 3000 }); this.cargar(); } });
  }
}
