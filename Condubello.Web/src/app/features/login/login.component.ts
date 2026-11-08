import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../core/auth.service';
import { mensajeError } from '../../core/interceptors';

const KEY_EMAIL = 'condubello.email';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatCheckboxModule, MatProgressSpinnerModule],
  template: `
    <div class="fondo">
      <mat-card class="tarjeta">
        <div class="cabecera">
          <mat-icon class="logo">directions_car</mat-icon>
          <h1>Condubello</h1>
          <p>Escuela de conducción</p>
        </div>
        <form [formGroup]="form" (ngSubmit)="entrar()">
          <mat-form-field appearance="outline">
            <mat-label>Correo electrónico</mat-label>
            <mat-icon matPrefix>mail</mat-icon>
            <input matInput type="email" formControlName="email" autocomplete="username" autofocus>
            @if (form.controls.email.touched && form.controls.email.invalid) { <mat-error>Ingrese un correo válido</mat-error> }
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Contraseña</mat-label>
            <mat-icon matPrefix>lock</mat-icon>
            <input matInput [type]="verClave() ? 'text' : 'password'" formControlName="password" autocomplete="current-password">
            <button mat-icon-button matSuffix type="button" (click)="verClave.set(!verClave())"
                    [attr.aria-label]="verClave() ? 'Ocultar contraseña' : 'Mostrar contraseña'">
              <mat-icon>{{ verClave() ? 'visibility_off' : 'visibility' }}</mat-icon>
            </button>
            @if (form.controls.password.touched && form.controls.password.invalid) { <mat-error>La contraseña es obligatoria</mat-error> }
          </mat-form-field>
          <mat-checkbox formControlName="recordar">Recordar mi correo</mat-checkbox>
          @if (error()) { <p class="error" role="alert"><mat-icon>error</mat-icon> {{ error() }}</p> }
          <button mat-flat-button type="submit" class="boton" [disabled]="form.invalid || cargando()">
            @if (cargando()) { <mat-spinner diameter="20" /> } @else { Ingresar }
          </button>
          <a mat-button routerLink="/registro" class="registro">¿No tiene cuenta? Regístrese</a>
        </form>
      </mat-card>
    </div>
  `,
  styles: `
    .fondo { min-height: 100vh; display: grid; place-items: center; padding: 16px; background: linear-gradient(135deg, var(--mat-sys-primary-container), var(--mat-sys-surface-container)); }
    .tarjeta { width: min(420px, 100%); padding: 32px 28px; }
    .cabecera { text-align: center; margin-bottom: 24px; }
    .cabecera h1 { margin: 8px 0 0; }
    .cabecera p { margin: 4px 0 0; opacity: .7; }
    .logo { font-size: 48px; width: 48px; height: 48px; color: var(--mat-sys-primary); }
    form { display: flex; flex-direction: column; }
    .error { display: flex; align-items: center; gap: 8px; color: var(--mat-sys-error); margin: 12px 0 0; }
    .boton { margin-top: 16px; height: 48px; }
    .registro { margin-top: 8px; }
  `
})
export class LoginComponent {
  private auth = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private fb = inject(FormBuilder);

  form = this.fb.nonNullable.group({
    email: [this.emailGuardado(), [Validators.required, Validators.email]],
    password: ['', Validators.required],
    recordar: [!!this.emailGuardado()]
  });
  error = signal('');
  cargando = signal(false);
  verClave = signal(false);

  constructor() {
    // Si ya hay una sesión vigente, entra directo.
    if (this.auth.autenticado()) this.router.navigateByUrl('/');
  }

  entrar() {
    if (this.form.invalid || this.cargando()) return;
    this.cargando.set(true);
    this.error.set('');
    const { email, password, recordar } = this.form.getRawValue();
    this.auth.login(email.trim(), password).subscribe({
      next: () => {
        try { recordar ? localStorage.setItem(KEY_EMAIL, email.trim()) : localStorage.removeItem(KEY_EMAIL); } catch { /* sin storage */ }
        // Vuelve a la página que se intentó abrir antes de iniciar sesión.
        const destino = this.route.snapshot.queryParamMap.get('returnUrl');
        this.router.navigateByUrl(destino && destino.startsWith('/') ? destino : '/');
      },
      error: (e: HttpErrorResponse) => {
        this.error.set(e.status === 401 ? 'Correo o contraseña incorrectos.' : mensajeError(e));
        this.form.controls.password.reset('');
        this.cargando.set(false);
      }
    });
  }

  private emailGuardado(): string {
    try { return localStorage.getItem(KEY_EMAIL) ?? ''; } catch { return ''; }
  }
}
