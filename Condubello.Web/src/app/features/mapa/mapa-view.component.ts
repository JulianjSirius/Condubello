import { Component, ElementRef, OnDestroy, effect, input, viewChild } from '@angular/core';
import * as maplibregl from 'maplibre-gl';
import type { GeoJSONSource, LngLatBoundsLike, Map as MapLibreMap } from 'maplibre-gl';

/** Estilo vectorial gratuito, sin clave de API (https://openfreemap.org). */
const ESTILO_MAPA = 'https://tiles.openfreemap.org/styles/liberty';
/** Bogotá, como vista inicial. */
const CENTRO_INICIAL: [number, number] = [-74.0817, 4.6097];

/** Mapa reutilizable: dibuja la ruta autorizada (azul) y el recorrido real (rojo o naranja si hay alerta). */
@Component({
  selector: 'app-mapa-view',
  template: `<div #contenedor class="mapa"></div>`,
  styles: `:host { display: block; height: 100%; } .mapa { width: 100%; height: 100%; min-height: 420px; border-radius: 8px; }`
})
export class MapaViewComponent implements OnDestroy {
  /** GeoJSON de la ruta autorizada (objeto o cadena JSON). */
  ruta = input<unknown>(null);
  /** GeoJSON del recorrido GPS (objeto o cadena JSON). */
  recorrido = input<unknown>(null);
  alerta = input(false);

  private contenedor = viewChild.required<ElementRef<HTMLDivElement>>('contenedor');
  private mapa?: MapLibreMap;
  private listo = false;

  constructor() {
    effect(() => {
      const ruta = this.normalizar(this.ruta());
      const recorrido = this.normalizar(this.recorrido());
      const alerta = this.alerta();
      this.asegurarMapa();
      this.aplicar(ruta, recorrido, alerta);
    });
  }

  ngOnDestroy() {
    this.mapa?.remove();
  }

  private asegurarMapa() {
    if (this.mapa) return;
    const mapa = new maplibregl.Map({
      container: this.contenedor().nativeElement,
      style: ESTILO_MAPA,
      center: CENTRO_INICIAL,
      zoom: 11
    });
    mapa.addControl(new maplibregl.NavigationControl(), 'top-right');
    mapa.on('load', () => {
      for (const id of ['ruta', 'recorrido']) {
        mapa.addSource(id, { type: 'geojson', data: VACIO });
      }
      mapa.addLayer({ id: 'ruta-linea', type: 'line', source: 'ruta',
        paint: { 'line-color': '#1565c0', 'line-width': 5, 'line-opacity': 0.8 } });
      mapa.addLayer({ id: 'recorrido-linea', type: 'line', source: 'recorrido',
        paint: { 'line-color': '#c62828', 'line-width': 3 } });
      this.listo = true;
      this.aplicar(this.normalizar(this.ruta()), this.normalizar(this.recorrido()), this.alerta());
    });
    this.mapa = mapa;
  }

  private aplicar(ruta: any, recorrido: any, alerta: boolean) {
    if (!this.mapa || !this.listo) return;
    (this.mapa.getSource('ruta') as GeoJSONSource).setData(ruta ?? VACIO);
    (this.mapa.getSource('recorrido') as GeoJSONSource).setData(recorrido ?? VACIO);
    this.mapa.setPaintProperty('recorrido-linea', 'line-color', alerta ? '#ef6c00' : '#c62828');

    const puntos = [...coordenadas(ruta), ...coordenadas(recorrido)];
    if (puntos.length) {
      const b = puntos.reduce((acc, p) => acc.extend(p), new maplibregl.LngLatBounds(puntos[0], puntos[0]));
      this.mapa.fitBounds(b as LngLatBoundsLike, { padding: 40, maxZoom: 16, duration: 500 });
    }
  }

  private normalizar(v: unknown): any {
    if (!v) return null;
    if (typeof v === 'string') {
      try { return JSON.parse(v); } catch { return null; }
    }
    return v;
  }
}

const VACIO = { type: 'FeatureCollection', features: [] } as any;

/** Recorre cualquier GeoJSON y devuelve todas sus coordenadas [lng, lat]. */
function coordenadas(g: any): [number, number][] {
  if (!g) return [];
  if (g.type === 'FeatureCollection') return (g.features ?? []).flatMap(coordenadas);
  if (g.type === 'Feature') return coordenadas(g.geometry);
  if (g.type === 'GeometryCollection') return (g.geometries ?? []).flatMap(coordenadas);
  const plano = (c: any): [number, number][] =>
    typeof c?.[0] === 'number' ? [[c[0], c[1]]] : Array.isArray(c) ? c.flatMap(plano) : [];
  return plano(g.coordinates);
}
