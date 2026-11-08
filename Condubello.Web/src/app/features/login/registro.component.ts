import { Component, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService } from '../../core/crud.service';
import { mensajeError } from '../../core/interceptors';

const clavesIguales = (g: AbstractControl): ValidationErrors | null =>
  g.get('password')?.value === g.get('confirmar')?.value ? null : { noCoinciden: true };

@Component({
  selector: 'app-registro',
  imports: [ReactiveFormsModule, RouterLink, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  template: `
    <div class="fondo">
      <mat-card class="tarjeta">
        @if (enviado()) {
          <div class="cabecera">
            <mat-icon class="logo">hourglass_top</mat-icon>
            <h1>Solicitud enviada</h1>
            <p>Su registro quedó pendiente. Un administrativo debe aprobarlo antes de que pueda ingresar.</p>
          </div>
          <a mat-flat-button routerLink="/login" class="boton">Volver al inicio de sesión</a>
        } @else {
        <div class="cabecera">
          <mat-icon class="logo">person_add</mat-icon>
          <h1>Crear cuenta</h1>
          <p>Registro de estudiantes (requiere aprobación)</p>
        </div>
        <form [formGroup]="form" (ngSubmit)="registrar()">
          <mat-form-field appearance="outline">
            <mat-label>Cédula</mat-label>
            <input matInput formControlName="cedula" inputmode="numeric">
            @if (c.cedula.touched && c.cedula.invalid) { <mat-error>Entre 5 y 20 dígitos</mat-error> }
          </mat-form-field>
          <div class="fila">
            <mat-form-field appearance="outline">
              <mat-label>Nombre</mat-label>
              <input matInput formControlName="nombre" autocomplete="given-name">
              @if (c.nombre.touched && c.nombre.invalid) { <mat-error>Obligatorio (máx. 100)</mat-error> }
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Apellidos</mat-label>
              <input matInput formControlName="apellidos" autocomplete="family-name">
              @if (c.apellidos.touched && c.apellidos.invalid) { <mat-error>Obligatorio (máx. 100)</mat-error> }
            </mat-form-field>
          </div>
          <mat-form-field appearance="outline">
            <mat-label>Correo electrónico</mat-label>
            <input matInput type="email" formControlName="email" autocomplete="email">
            @if (c.email.touched && c.email.invalid) { <mat-error>Ingrese un correo válido</mat-error> }
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Contraseña</mat-label>
            <input matInput [type]="verClave() ? 'text' : 'password'" formControlName="password" autocomplete="new-password">
            <button mat-icon-button matSuffix type="button" (click)="verClave.set(!verClave())"
                    [attr.aria-label]="verClave() ? 'Ocultar contraseña' : 'Mostrar contraseña'">
              <mat-icon>{{ verClave() ? 'visibility_off' : 'visibility' }}</mat-icon>
            </button>
            @if (c.password.touched && c.password.invalid) { <mat-error>Entre 8 y 100 caracteres</mat-error> }
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Confirmar contraseña</mat-label>
            <input matInput [type]="verClave() ? 'text' : 'password'" formControlName="confirmar" autocomplete="new-password">
            @if (c.confirmar.touched && form.hasError('noCoinciden')) { <mat-error>Las contraseñas no coinciden</mat-error> }
          </mat-form-field>
          @if (error()) { <p class="error" role="alert"><mat-icon>error</mat-icon> {{ error() }}</p> }
          <button mat-flat-button type="submit" class="boton" [disabled]="form.invalid || cargando()">
            @if (cargando()) { <mat-spinner diameter="20" /> } @else { Enviar solicitud }
          </button>
          <a mat-button routerLink="/login" class="volver">¿Ya tiene cuenta? Ingresar</a>
        </form>
        }
      </mat-card>
    </div>
  `,
  styles: `
    .fondo { min-height: 100vh; display: grid; place-items: center; padding: 16px; background: linear-gradient(135deg, var(--mat-sys-primary-container), var(--mat-sys-surface-container)); }
    .tarjeta { width: min(480px, 100%); padding: 28px; }
    .cabecera { text-align: center; margin-bottom: 16px; }
    .cabecera h1 { margin: 8px 0 0; }
    .cabecera p { margin: 4px 0 0; opacity: .7; }
    .logo { font-size: 44px; width: 44px; height: 44px; color: var(--mat-sys-primary); }
    form { display: flex; flex-direction: column; }
    .fila { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    @media (max-width: 480px) { .fila { grid-template-columns: 1fr; } }
    .error { display: flex; align-items: center; gap: 8px; color: var(--mat-sys-error); margin: 4px 0 0; }
    .boton { margin-top: 12px; height: 48px; }
    .volver { margin-top: 8px; }
  `
})
export class RegistroComponent {
  private fb = inject(FormBuilder);
  private api = inject(ApiService);

  form = this.fb.nonNullable.group({
    cedula: ['', [Validators.required, Validators.pattern(/^\d{5,20}$/)]],
    nombre: ['', [Validators.required, Validators.maxLength(100)]],
    apellidos: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(150)]],
    password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(100)]],
    confirmar: ['', Validators.required]
  }, { validators: clavesIguales });

  get c() { return this.form.controls; }

  error = signal('');
  cargando = signal(false);
  verClave = signal(false);
  enviado = signal(false);

  registrar() {
    if (this.form.invalid || this.cargando()) return;
    this.cargando.set(true);
    this.error.set('');
    const { confirmar, ...datos } = this.form.getRawValue();
    datos.cedula = datos.cedula.trim();
    datos.nombre = datos.nombre.trim();
    datos.apellidos = datos.apellidos.trim();
    datos.email = datos.email.trim();

    // La cuenta queda pendiente: un administrativo debe aprobarla antes de poder ingresar.
    this.api.create('auth/registro', datos).subscribe({
      next: () => { this.enviado.set(true); this.cargando.set(false); },
      error: (e: HttpErrorResponse) => { this.error.set(mensajeError(e)); this.cargando.set(false); }
    });
  }
}
