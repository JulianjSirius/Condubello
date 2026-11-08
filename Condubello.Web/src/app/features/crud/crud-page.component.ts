import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../core/auth.service';
import { ApiService } from '../../core/crud.service';
import { mensajeError } from '../../core/interceptors';
import { ConfirmDialogComponent } from './confirm-dialog.component';
import { FormDialogComponent } from './form-dialog.component';
import { Column, Field, ModuleDef, RowAction } from './modules';

@Component({
  selector: 'app-crud-page',
  imports: [DatePipe, MatTableModule, MatButtonModule, MatIconModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatProgressBarModule, MatTooltipModule],
  template: `
    <div class="cabecera">
      <h1>{{ def().title }}</h1>
      <span class="espacio"></span>
      @if (accionGlobal(); as g) {
        <button mat-stroked-button (click)="ejecutarGlobal()"><mat-icon>{{ g.icon }}</mat-icon> {{ g.label }}</button>
      }
      @if (puedeCrear()) {
        <button mat-flat-button (click)="crear()"><mat-icon>add</mat-icon> Nuevo</button>
      }
    </div>

    <mat-form-field appearance="outline" class="filtro">
      <mat-label>Filtrar</mat-label>
      <input matInput [value]="filtro()" (input)="filtro.set($any($event.target).value)">
    </mat-form-field>

    @if (cargando()) { <mat-progress-bar mode="indeterminate" /> }

    <div class="tabla-wrap">
      <table mat-table [dataSource]="filas()">
        @for (c of def().columns; track c.key) {
          <ng-container [matColumnDef]="c.key">
            <th mat-header-cell *matHeaderCellDef>{{ c.label }}</th>
            <td mat-cell *matCellDef="let r">
              @switch (c.format) {
                @case ('fecha') { {{ r[c.key] | date: 'dd/MM/yyyy HH:mm' }} }
                @case ('bool') { <mat-icon>{{ r[c.key] ? 'check_circle' : 'cancel' }}</mat-icon> }
                @case ('json') { <code class="json" [matTooltip]="texto(r, c)">{{ texto(r, c) }}</code> }
                @case ('enum') { <span class="chip">{{ (r[c.key] ?? '').replaceAll('_', ' ') }}</span> }
                @default { {{ texto(r, c) }} }
              }
            </td>
          </ng-container>
        }
        <ng-container matColumnDef="_acciones">
          <th mat-header-cell *matHeaderCellDef></th>
          <td mat-cell *matCellDef="let r" class="acciones">
            @for (a of accionesFila(); track a.label) {
              @if (!a.visible || a.visible(r)) {
              <button mat-icon-button [matTooltip]="a.label" (click)="accion(a, r)"><mat-icon>{{ a.icon }}</mat-icon></button>
              }
            }
            @if (puedeEditar()) {
              <button mat-icon-button matTooltip="Editar" (click)="editar(r)"><mat-icon>edit</mat-icon></button>
            }
            @if (puedeBorrar()) {
              <button mat-icon-button matTooltip="Eliminar" (click)="borrar(r)"><mat-icon>delete</mat-icon></button>
            }
          </td>
        </ng-container>
        <tr mat-header-row *matHeaderRowDef="columnas()"></tr>
        <tr mat-row *matRowDef="let row; columns: columnas()"></tr>
        <tr class="mat-row" *matNoDataRow><td class="vacio" [attr.colspan]="columnas().length">Sin registros</td></tr>
      </table>
    </div>
  `,
  styles: `
    .cabecera { display: flex; align-items: center; gap: 12px; }
    .espacio { flex: 1; }
    .filtro { width: min(360px, 100%); }
    .tabla-wrap { overflow-x: auto; }
    table { width: 100%; }
    .acciones { white-space: nowrap; text-align: right; }
    .vacio { padding: 24px; text-align: center; opacity: .7; }
    .json { display: inline-block; max-width: 220px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; vertical-align: bottom; }
    .chip { padding: 2px 10px; border-radius: 12px; background: var(--mat-sys-secondary-container); font-size: 12px; white-space: nowrap; }
  `
})
export class CrudPageComponent {
  def = input.required<ModuleDef>();

  private api = inject(ApiService);
  private auth = inject(AuthService);
  private dialog = inject(MatDialog);
  private snack = inject(MatSnackBar);

  datos = signal<any[]>([]);
  cargando = signal(false);
  filtro = signal('');
  /** Cache de relaciones: path -> id -> etiqueta. */
  private lookups = signal<Record<string, Record<string, string>>>({});

  puedeCrear = computed(() => this.auth.tieneRol(...this.def().crear));
  puedeEditar = computed(() => this.auth.tieneRol(...this.def().editar));
  puedeBorrar = computed(() => this.auth.tieneRol(...this.def().borrar));
  accionesFila = computed(() => (this.def().actions ?? []).filter(a => this.auth.tieneRol(...a.roles)));
  accionGlobal = computed(() => {
    const g = this.def().accionGlobal;
    return g && this.auth.tieneRol(...g.roles) ? g : null;
  });
  hayAcciones = computed(() => this.accionesFila().length > 0 || this.puedeEditar() || this.puedeBorrar());
  columnas = computed(() => [...this.def().columns.map(c => c.key), ...(this.hayAcciones() ? ['_acciones'] : [])]);

  filas = computed(() => {
    const q = this.filtro().trim().toLowerCase();
    if (!q) return this.datos();
    return this.datos().filter(r => this.def().columns.some(c => this.texto(r, c).toLowerCase().includes(q)));
  });

  constructor() {
    effect(() => { this.def(); this.filtro.set(''); this.cargar(); });
  }

  cargar() {
    const def = this.def();
    this.cargando.set(true);
    // Carga la lista y las tablas relacionadas para mostrar nombres en lugar de GUID.
    const rels = new Map<string, Column['lookup']>();
    def.columns.forEach(c => c.lookup && rels.set(c.lookup.path, c.lookup));
    const reqs: Record<string, any> = { _datos: this.api.list(def.path) };
    rels.forEach((l, path) => { reqs[path] = this.api.list(path); });
    forkJoin(reqs).subscribe({
      next: res => {
        const mapa: Record<string, Record<string, string>> = {};
        rels.forEach((l, path) => {
          mapa[path] = Object.fromEntries((res[path] as any[]).map(x => [x.id, l!.label(x)]));
        });
        this.lookups.set(mapa);
        this.datos.set(res['_datos'] as any[]);
        this.cargando.set(false);
      },
      error: (e: HttpErrorResponse) => { this.cargando.set(false); this.snack.open(mensajeError(e), 'OK', { duration: 5000 }); }
    });
  }

  texto(r: any, c: Column): string {
    const v = r[c.key];
    if (v === null || v === undefined) return '';
    if (c.format === 'lookup') return this.lookups()[c.lookup!.path]?.[v] ?? v;
    if (c.format === 'json') return JSON.stringify(v);
    if (c.format === 'fecha') return new Date(v).toLocaleString('es-CO');
    return String(v);
  }

  crear() {
    const d = this.def();
    this.abrir({
      title: `Nuevo ${d.singular}`,
      fields: d.fields.filter(f => !f.soloEditar),
      submit: body => this.api.create(d.path, body)
    });
  }

  editar(r: any) {
    const d = this.def();
    this.abrir({
      title: `Editar ${d.singular}`,
      fields: d.fields.filter(f => !f.soloCrear),
      value: r,
      submit: body => this.api.update(d.path, r.id, body)
    });
  }

  ejecutarGlobal() {
    const g = this.accionGlobal();
    if (!g) return;
    this.dialog.open(ConfirmDialogComponent, { data: { mensaje: g.confirmar, boton: g.label } }).afterClosed().subscribe(ok => {
      if (!ok) return;
      this.cargando.set(true);
      this.api.create(`${this.def().path}/${g.path}`, {}).subscribe({
        next: (r: any) => {
          this.snack.open(`Sincronizadas: ${r?.sincronizadas ?? 0} · Con alerta: ${r?.conAlerta ?? 0} · Sin datos: ${r?.sinDatos ?? 0}`, 'OK', { duration: 6000 });
          this.cargar();
        },
        error: (e: HttpErrorResponse) => { this.cargando.set(false); this.snack.open(mensajeError(e), 'OK', { duration: 6000 }); }
      });
    });
  }

  accion(a: RowAction, r: any) {
    const d = this.def();
    const path = `${d.path}/${a.path.replace('{id}', r.id)}`;
    if (a.confirmar) {
      this.dialog.open(ConfirmDialogComponent, { data: { mensaje: a.confirmar, boton: 'Aprobar' } }).afterClosed().subscribe(ok => {
        if (!ok) return;
        const req = a.method === 'PATCH' ? this.api.patch(path, {}) : this.api.put(path, {});
        req.subscribe({
          next: () => { this.snack.open('Hecho', 'OK', { duration: 3000 }); this.cargar(); },
          error: (e: HttpErrorResponse) => this.snack.open(mensajeError(e), 'OK', { duration: 6000 })
        });
      });
      return;
    }
    this.abrir({
      title: a.label,
      fields: a.fields,
      value: r,
      submit: body => a.method === 'PATCH' ? this.api.patch(path, body) : this.api.put(path, body)
    });
  }

  borrar(r: any) {
    const d = this.def();
    this.dialog.open(ConfirmDialogComponent, { data: { mensaje: `¿Eliminar este ${d.singular}?` } }).afterClosed().subscribe(ok => {
      if (!ok) return;
      this.api.remove(d.path, r.id).subscribe({
        next: () => { this.snack.open('Eliminado', 'OK', { duration: 3000 }); this.cargar(); },
        error: (e: HttpErrorResponse) => this.snack.open(mensajeError(e), 'OK', { duration: 6000 })
      });
    });
  }

  private abrir(data: { title: string; fields: Field[]; value?: any; submit: (b: any) => any }) {
    this.dialog.open(FormDialogComponent, { data, maxHeight: '90vh' }).afterClosed().subscribe(ok => {
      if (ok) { this.snack.open('Guardado', 'OK', { duration: 3000 }); this.cargar(); }
    });
  }
}
