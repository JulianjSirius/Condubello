import { Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, ValidatorFn, Validators, AbstractControl } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin, Observable, of } from 'rxjs';
import { ApiService } from '../../core/crud.service';
import { mensajeError } from '../../core/interceptors';
import { Field } from './modules';
import { RutaPickerComponent } from '../mapa/ruta-picker.component';

export interface FormDialogData {
  title: string;
  fields: Field[];
  value?: any;
  /** Envía el formulario; devuelve el observable de la petición. */
  submit: (body: any) => Observable<unknown>;
}

const jsonValido: ValidatorFn = (c: AbstractControl) => {
  const v = (c.value ?? '').toString().trim();
  if (!v) return null;
  try { JSON.parse(v); return null; } catch { return { json: true }; }
};

/** "YYYY-MM-DDTHH:mm" en hora local para <input type=datetime-local>. */
function aLocal(iso: string | null | undefined): string {
  if (!iso) return '';
  const d = new Date(iso);
  const p = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}T${p(d.getHours())}:${p(d.getMinutes())}`;
}

@Component({
  selector: 'app-form-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatCheckboxModule, RutaPickerComponent],
  template: `
    <h2 mat-dialog-title>{{ data.title }}</h2>
    <form [formGroup]="form" (ngSubmit)="guardar()">
      <mat-dialog-content class="contenido">
        @for (f of data.fields; track f.key) {
          @switch (f.type) {
            @case ('checkbox') {
              <mat-checkbox [formControlName]="f.key">{{ f.label }}</mat-checkbox>
            }
            @case ('select') {
              <mat-form-field appearance="outline">
                <mat-label>{{ f.label }}</mat-label>
                <mat-select [formControlName]="f.key">
                  @if (!f.required) { <mat-option [value]="null">—</mat-option> }
                  @for (o of opciones()[f.key] || []; track o.value) {
                    <mat-option [value]="o.value">{{ o.label }}</mat-option>
                  }
                </mat-select>
                @if (form.get(f.key)?.hasError('required')) { <mat-error>Obligatorio</mat-error> }
              </mat-form-field>
            }
            @case ('ruta') {
              <app-ruta-picker [control]="$any(form.get(f.key))" [referencia]="f.referenciaSesion ? referencias()[f.referenciaSesion] : null" />
              @if (f.referenciaSesion) { <p class="nota">La línea punteada es la ruta autorizada. Lo que marques aquí es una copia: no modifica la ruta original (solo se edita en Rutas).</p> }
              @if (form.get(f.key)?.hasError('required')) { <mat-error>Marca el punto A y el punto B en el mapa</mat-error> }
            }
            @case ('json') {
              <mat-form-field appearance="outline">
                <mat-label>{{ f.label }}</mat-label>
                <textarea matInput rows="6" [formControlName]="f.key" class="mono"></textarea>
                @if (form.get(f.key)?.hasError('json')) { <mat-error>JSON no válido</mat-error> }
              </mat-form-field>
            }
            @default {
              <mat-form-field appearance="outline">
                <mat-label>{{ f.label }}</mat-label>
                <input matInput [formControlName]="f.key" [type]="inputType(f)"
                       [attr.step]="f.step ?? (f.type === 'number' ? 1 : null)" [attr.min]="f.min" [attr.max]="f.max">
                @if (form.get(f.key)?.hasError('required')) { <mat-error>Obligatorio</mat-error> }
                @else if (form.get(f.key)?.hasError('minlength')) { <mat-error>Mínimo {{ f.minLength }} caracteres</mat-error> }
                @else if (form.get(f.key)?.hasError('maxlength')) { <mat-error>Máximo {{ f.maxLength }} caracteres</mat-error> }
                @else if (form.get(f.key)?.hasError('email')) { <mat-error>Email no válido</mat-error> }
                @else if (form.get(f.key)?.hasError('min') || form.get(f.key)?.hasError('max')) { <mat-error>Entre {{ f.min }} y {{ f.max }}</mat-error> }
              </mat-form-field>
            }
          }
        }
        @if (error()) { <p class="error">{{ error() }}</p> }
      </mat-dialog-content>
      <mat-dialog-actions align="end">
        <button mat-button type="button" mat-dialog-close>Cancelar</button>
        <button mat-flat-button type="submit" [disabled]="form.invalid || enviando()">Guardar</button>
      </mat-dialog-actions>
    </form>
  `,
  styles: `
    .contenido { display: flex; flex-direction: column; gap: 4px; min-width: min(480px, 80vw); padding-top: 8px !important; }
    .nota { font-size: 12px; margin: 0 0 8px; opacity: .75; }
    .mono { font-family: monospace; }
    .error { color: var(--mat-sys-error); margin: 0; }
  `
})
export class FormDialogComponent implements OnInit {
  data = inject<FormDialogData>(MAT_DIALOG_DATA);
  private ref = inject(MatDialogRef<FormDialogComponent>);
  private api = inject(ApiService);

  form = new FormGroup<Record<string, FormControl>>({});
  opciones = signal<Record<string, { value: string; label: string }[] | undefined>>({});
  referencias = signal<Record<string, unknown>>({});
  error = signal('');
  enviando = signal(false);

  ngOnInit() {
    const inicial = this.data.value ?? {};
    for (const f of this.data.fields) {
      const validators: ValidatorFn[] = [];
      if (f.required && f.type !== 'checkbox') validators.push(Validators.required);
      if (f.minLength) validators.push(Validators.minLength(f.minLength));
      if (f.maxLength) validators.push(Validators.maxLength(f.maxLength));
      if (f.min !== undefined) validators.push(Validators.min(f.min));
      if (f.max !== undefined) validators.push(Validators.max(f.max));
      if (f.type === 'email') validators.push(Validators.email);
      if (f.type === 'json') validators.push(jsonValido);
      
      let v = inicial[f.key];
      if (f.type === 'datetime') v = aLocal(v);
      else if (f.type === 'ruta') v = v == null ? '' : JSON.stringify(v);
      else if (f.type === 'json') v = v == null ? '' : JSON.stringify(v, null, 2);
      else if (f.type === 'checkbox') v = !!v;
      else if (f.type === 'password') v = '';
      else if (v === undefined) v = f.type === 'select' ? null : '';
      this.form.addControl(f.key, new FormControl(v, validators));
    }

    // Ruta autorizada de la sesión, como referencia de solo lectura para los campos de ruta.
    for (const f of this.data.fields) {
      const clave = f.referenciaSesion;
      if (f.type !== 'ruta' || !clave) continue;
      const cargarRef = (sesionId: string | null) => {
        if (!sesionId) { this.referencias.update(r => ({ ...r, [clave]: null })); return; }
        this.api.get(`sesiones-practicas/${sesionId}`).subscribe({
          next: (s: any) => s?.rutaId
            ? this.api.get(`rutas/${s.rutaId}`).subscribe({
                next: (r: any) => this.referencias.update(x => ({ ...x, [clave]: r?.geoJson })),
                error: () => this.referencias.update(x => ({ ...x, [clave]: null }))
              })
            : this.referencias.update(x => ({ ...x, [clave]: null })),
          error: () => this.referencias.update(x => ({ ...x, [clave]: null }))
        });
      };
      const ctrl = this.form.get(clave);
      cargarRef(ctrl?.value || inicial[clave] || null);
      ctrl?.valueChanges.subscribe(v => cargarRef(v || null));
    }

    // Carga las opciones de los selects.
    const cargas: Record<string, Observable<any>> = {};
    for (const f of this.data.fields) {
      if (f.type !== 'select') continue;
      if (f.options) this.opciones.update(o => ({ ...o, [f.key]: f.options!.map(x => ({ value: x, label: x.replaceAll('_', ' ') })) }));
      else if (f.lookup) cargas[f.key] = this.api.list(f.lookup.path);
    }
    if (Object.keys(cargas).length) {
      forkJoin(cargas).subscribe(res => {
        for (const f of this.data.fields) {
          if (f.lookup && res[f.key]) {
            this.opciones.update(o => ({ ...o, [f.key]: res[f.key].map((x: any) => ({ value: x.id, label: f.lookup!.label(x) })) }));
          }
        }
      });
    }
  }

  inputType(f: Field): string {
    return f.type === 'datetime' ? 'datetime-local' : f.type ?? 'text';
  }

  guardar() {
    if (this.form.invalid) return;
    const raw = this.form.getRawValue();
    const body: Record<string, unknown> = {};
    for (const f of this.data.fields) {
      let v = raw[f.key];
      if (v === '' || v === undefined) v = null;
      if (v !== null) {
        if (f.type === 'datetime') v = new Date(v).toISOString();
        else if (f.type === 'json' || f.type === 'ruta') v = JSON.parse(v);
        else if (f.type === 'number') v = Number(v);
      }
      body[f.key] = v;
    }
    this.enviando.set(true);
    this.error.set('');
    this.data.submit(body).subscribe({
      next: () => this.ref.close(true),
      error: (e: HttpErrorResponse) => { this.error.set(mensajeError(e)); this.enviando.set(false); }
    });
  }
}
