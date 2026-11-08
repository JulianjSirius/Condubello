import { Component, OnInit, computed, effect, inject, signal } from '@angular/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { ApiService } from '../../core/crud.service';
import { MapaViewComponent } from './mapa-view.component';

@Component({
  selector: 'app-mapa',
  imports: [MatFormFieldModule, MatSelectModule, MatIconModule, MapaViewComponent],
  template: `
    <h2>Mapa de rutas y recorridos</h2>
    <div class="filtros">
      <mat-form-field>
        <mat-label>Ruta autorizada</mat-label>
        <mat-select [value]="rutaId()" (valueChange)="rutaId.set($event)">
          <mat-option [value]="null">Ninguna</mat-option>
          @for (r of rutas(); track r.id) {
            <mat-option [value]="r.id">{{ r.nombre }}</mat-option>
          }
        </mat-select>
      </mat-form-field>
      <mat-form-field>
        <mat-label>Recorrido GPS (sesión)</mat-label>
        <mat-select [value]="telemetriaId()" (valueChange)="telemetriaId.set($event)">
          <mat-option [value]="null">Ninguno</mat-option>
          @for (t of telemetrias(); track t.id) {
            <mat-option [value]="t.id">{{ etiqueta(t) }}</mat-option>
          }
        </mat-select>
      </mat-form-field>
    </div>
    @if (telemetria()?.alertaDesviacion) {
      <p class="alerta"><mat-icon>warning</mat-icon> Este recorrido tiene alerta de desviación.</p>
    }
    @if (direcciones().inicio || direcciones().fin) {
      <p class="direcciones">
        @if (direcciones().inicio) { <span><mat-icon>trip_origin</mat-icon> Inicio: {{ direcciones().inicio }}</span> }
        @if (direcciones().fin) { <span><mat-icon>place</mat-icon> Fin: {{ direcciones().fin }}</span> }
      </p>
    }
    <div class="lienzo">
      <app-mapa-view
        [ruta]="ruta()?.geoJson"
        [recorrido]="telemetria()?.rutaGps"
        [alerta]="!!telemetria()?.alertaDesviacion"
      />
    </div>
    <p class="leyenda">
      <span class="azul"></span> Ruta autorizada &nbsp; <span class="rojo"></span> Recorrido real
    </p>
  `,
  styles: `
    .filtros {
      display: flex;
      gap: 16px;
      flex-wrap: wrap;
    }
    mat-form-field {
      min-width: 280px;
    }
    .lienzo {
      height: 60vh;
    }
    .alerta {
      display: flex;
      align-items: center;
      gap: 6px;
      color: #ef6c00;
      font-weight: 500;
    }
    .leyenda {
      font-size: 13px;
    }
    .leyenda span {
      display: inline-block;
      width: 24px;
      height: 4px;
      vertical-align: middle;
      margin-right: 4px;
    }
    .azul {
      background: #1565c0;
    }
    .rojo {
      background: #c62828;
    }
  `,
})
export class MapaComponent implements OnInit {
  private api = inject(ApiService);

  rutas = signal<any[]>([]);
  telemetrias = signal<any[]>([]);
  private sesiones = signal<any[]>([]);
  rutaId = signal<string | null>(null);
  telemetriaId = signal<string | null>(null);

  ruta = computed(() => this.rutas().find((r) => r.id === this.rutaId()));
  telemetria = computed(() => this.telemetrias().find((t) => t.id === this.telemetriaId()));

  /** Direcciones (geocodificación inversa) del inicio y el fin del recorrido seleccionado. */
  direcciones = signal<{ inicio?: string; fin?: string }>({});

  constructor() {
    effect(() => {
      const extremos = extremosDe(this.telemetria()?.rutaGps);
      this.direcciones.set({});
      if (!extremos) return;
      const pedir = (p: [number, number], clave: 'inicio' | 'fin') =>
        this.api.get<{ texto: string }>(`geocodificacion/inversa?lat=${p[1]}&lon=${p[0]}`).subscribe({
          next: d => this.direcciones.update(x => ({ ...x, [clave]: d.texto })),
          error: () => {} // sin clave de MapTiler, sin permiso o sin resultado: simplemente no se muestra
        });
      pedir(extremos[0], 'inicio');
      pedir(extremos[1], 'fin');
    });
  }

  ngOnInit() {
    this.api.list('rutas').subscribe({ next: (r) => this.rutas.set(r), error: () => {} });
    this.api
      .list('sesiones-practicas')
      .subscribe({ next: (s) => this.sesiones.set(s), error: () => {} });
    this.api
      .list('telemetria')
      .subscribe({ next: (t) => this.telemetrias.set(t), error: () => {} });
  }

  etiqueta(t: any): string {
    const s = this.sesiones().find((x) => x.id === t.sesionId);
    const base = s ? new Date(s.fechaHoraInicio).toLocaleString('es-CO') : t.sesionId;
    return t.alertaDesviacion ? `${base} ⚠` : base;
  }
}

/** Primer y último vértice [lon, lat] de un GeoJSON (objeto o cadena). */
function extremosDe(g: unknown): [[number, number], [number, number]] | null {
  try {
    const geo: any = typeof g === 'string' ? JSON.parse(g) : g;
    const pts: [number, number][] = [];
    const rec = (n: any) => {
      if (!n) return;
      if (typeof n[0] === 'number') { pts.push([n[0], n[1]]); return; }
      if (Array.isArray(n)) n.forEach(rec);
      else if (n.type === 'FeatureCollection') n.features?.forEach(rec);
      else if (n.type === 'Feature') rec(n.geometry);
      else rec(n.coordinates);
    };
    rec(geo);
    return pts.length >= 2 ? [pts[0], pts[pts.length - 1]] : null;
  } catch { return null; }
}
