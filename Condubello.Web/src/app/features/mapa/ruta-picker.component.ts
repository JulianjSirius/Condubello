import { Component, ElementRef, OnDestroy, AfterViewInit, effect, input, signal, viewChild, inject } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import * as maplibregl from 'maplibre-gl';
import type { GeoJSONSource, Map as MapLibreMap, Marker } from 'maplibre-gl';
import { ApiService } from '../../core/crud.service';
import { mensajeError } from '../../core/interceptors';

const ESTILO_MAPA = 'https://tiles.openfreemap.org/styles/liberty';
const CENTRO_INICIAL: [number, number] = [-74.0817, 4.6097];
type Punto = [number, number]; // [lng, lat]

/** Selector de ruta A → B sobre el mapa: clic para poner A y B, arrastrar para ajustar. Guarda el GeoJSON en el control. */
@Component({
  selector: 'app-ruta-picker',
  imports: [MatIconModule, MatButtonModule],
  template: `
    <div class="barra">
      <span class="ayuda">{{ ayuda() }}</span>
      @if (puntosReferencia().length > 1) {
        <button mat-button type="button" (click)="copiarReferencia()"><mat-icon>content_copy</mat-icon> Copiar ruta autorizada</button>
      }
      <button mat-button type="button" (click)="limpiar()" [disabled]="!a"><mat-icon>restart_alt</mat-icon> Reiniciar</button>
    </div>
    <div #contenedor class="mapa"></div>
    @if (resumen()) { <p class="resumen"><mat-icon>route</mat-icon> {{ resumen() }}</p> }
    @if (error()) { <p class="error">{{ error() }}</p> }
  `,
  styles: `
    :host { display: block; }
    .mapa { width: 100%; height: 340px; border-radius: 8px; }
    .barra { display: flex; align-items: center; justify-content: space-between; gap: 8px; }
    .ayuda { font-size: 13px; }
    .resumen { display: flex; align-items: center; gap: 6px; margin: 8px 0 0; font-weight: 500; }
    .error { color: var(--mat-sys-error); margin: 8px 0 0; }
  `
})
export class RutaPickerComponent implements AfterViewInit, OnDestroy {
  control = input.required<FormControl>();
  /** Ruta autorizada de referencia (solo lectura): se dibuja punteada y puede copiarse como punto de partida. */
  referencia = input<unknown>(null);
  puntosReferencia = signal<Punto[]>([]);
  private contenedor = viewChild.required<ElementRef<HTMLDivElement>>('contenedor');
  private api = inject(ApiService);
  private mapa?: MapLibreMap;
  private listo = false;
  private mA?: Marker;
  private mB?: Marker;
  a?: Punto;
  b?: Punto;
  resumen = signal('');
  error = signal('');
  ayuda = signal('Haz clic en el mapa para marcar el punto A (inicio).');
  private peticion = 0;

  constructor() {
    effect(() => {
      let g: any = this.referencia();
      if (typeof g === 'string') { try { g = JSON.parse(g); } catch { g = null; } }
      const pts = coordenadas(g);
      this.puntosReferencia.set(pts);
      this.dibujarReferencia(pts.length > 1 ? g : null);
    });
  }

  ngAfterViewInit() {
    const mapa = new maplibregl.Map({ container: this.contenedor().nativeElement, style: ESTILO_MAPA, center: CENTRO_INICIAL, zoom: 11 });
    mapa.addControl(new maplibregl.NavigationControl(), 'top-right');
    mapa.on('load', () => {
      mapa.addSource('trazado', { type: 'geojson', data: { type: 'FeatureCollection', features: [] } });
      mapa.addLayer({ id: 'trazado-linea', type: 'line', source: 'trazado', paint: { 'line-color': '#1565c0', 'line-width': 5, 'line-opacity': 0.85 } });
      mapa.addSource('referencia', { type: 'geojson', data: { type: 'FeatureCollection', features: [] } });
      mapa.addLayer({ id: 'referencia-linea', type: 'line', source: 'referencia',
        paint: { 'line-color': '#546e7a', 'line-width': 4, 'line-dasharray': [2, 2], 'line-opacity': 0.9 } }, 'trazado-linea');
      this.listo = true;
      this.dibujarReferencia(this.puntosReferencia().length > 1 ? this.referencia() : null);
      this.cargarInicial();
    });
    mapa.on('click', (e) => {
      const p: Punto = [e.lngLat.lng, e.lngLat.lat];
      if (!this.a) this.poner('A', p);
      else if (!this.b) this.poner('B', p);
    });
    this.mapa = mapa;
  }

  ngOnDestroy() { this.mapa?.remove(); }

  private cargarInicial() {
    const v = this.control().value;
    let geo: any = null;
    try { geo = typeof v === 'string' ? JSON.parse(v) : v; } catch { /* sin trazado previo */ }
    const pts = coordenadas(geo);
    if (pts.length < 2) return;
    this.poner('A', pts[0], false);
    this.poner('B', pts[pts.length - 1], false);
    this.dibujar(geo);
    this.resumen.set('Ruta guardada. Arrastra A o B para recalcularla.');
    this.ajustarVista(pts);
  }

  private poner(cual: 'A' | 'B', p: Punto, calcular = true) {
    const el = document.createElement('div');
    el.textContent = cual;
    el.style.cssText = `width:28px;height:28px;border-radius:50%;color:#fff;font-weight:700;display:flex;align-items:center;justify-content:center;border:2px solid #fff;box-shadow:0 1px 4px #0006;background:${cual === 'A' ? '#2e7d32' : '#c62828'}`;
    const m = new maplibregl.Marker({ element: el, draggable: true }).setLngLat(p).addTo(this.mapa!);
    m.on('dragend', () => {
      const ll = m.getLngLat();
      if (cual === 'A') this.a = [ll.lng, ll.lat]; else this.b = [ll.lng, ll.lat];
      this.calcular();
    });
    if (cual === 'A') { this.a = p; this.mA = m; this.ayuda.set('Ahora marca el punto B (destino).'); }
    else { this.b = p; this.mB = m; this.ayuda.set('Arrastra A o B para ajustar el recorrido.'); }
    if (calcular && this.a && this.b) this.calcular();
  }

  private calcular() {
    if (!this.a || !this.b) return;
    const id = ++this.peticion;
    this.error.set('');
    this.resumen.set('Calculando trazado por calles…');
    this.api.create('rutas/calcular-trazado', {
      origenLat: this.a[1], origenLon: this.a[0], destinoLat: this.b[1], destinoLon: this.b[0]
    }).subscribe({
      next: (r: any) => {
        if (id !== this.peticion) return;
        this.dibujar(r.geoJson);
        this.control().setValue(JSON.stringify(r.geoJson));
        this.control().markAsDirty();
        this.resumen.set(`${r.distanciaKm} km · ~${r.duracionMin} min en auto`);
        this.ajustarVista(coordenadas(r.geoJson));
      },
      error: (e) => {
        if (id !== this.peticion) return;
        this.control().setValue('');
        this.dibujar(null);
        this.resumen.set('');
        this.error.set(mensajeError(e));
      }
    });
  }

  /** Copia los extremos de la ruta autorizada como A y B; el trazado resultante es una copia independiente. */
  copiarReferencia() {
    const pts = this.puntosReferencia();
    if (pts.length < 2) return;
    this.limpiar();
    this.poner('A', pts[0], false);
    this.poner('B', pts[pts.length - 1], true);
    this.ajustarVista(pts);
  }

  private dibujarReferencia(geo: unknown) {
    if (!this.listo) return;
    let g: any = geo;
    if (typeof g === 'string') { try { g = JSON.parse(g); } catch { g = null; } }
    (this.mapa!.getSource('referencia') as GeoJSONSource).setData(g ?? { type: 'FeatureCollection', features: [] });
  }

  limpiar() {
    this.peticion++;
    this.mA?.remove(); this.mB?.remove();
    this.mA = this.mB = this.a = this.b = undefined;
    this.dibujar(null);
    this.control().setValue('');
    this.resumen.set(''); this.error.set('');
    this.ayuda.set('Haz clic en el mapa para marcar el punto A (inicio).');
  }

  private dibujar(geo: any) {
    if (!this.listo) return;
    (this.mapa!.getSource('trazado') as GeoJSONSource).setData(geo ?? { type: 'FeatureCollection', features: [] });
  }

  private ajustarVista(pts: Punto[]) {
    if (!pts.length) return;
    const b = pts.reduce((acc, p) => acc.extend(p), new maplibregl.LngLatBounds(pts[0], pts[0]));
    this.mapa?.fitBounds(b, { padding: 50, maxZoom: 16, duration: 400 });
  }
}

function coordenadas(g: any): Punto[] {
  if (!g) return [];
  if (g.type === 'FeatureCollection') return (g.features ?? []).flatMap(coordenadas);
  if (g.type === 'Feature') return coordenadas(g.geometry);
  const plano = (c: any): Punto[] => typeof c?.[0] === 'number' ? [[c[0], c[1]]] : Array.isArray(c) ? c.flatMap(plano) : [];
  return plano(g.coordinates);
}
