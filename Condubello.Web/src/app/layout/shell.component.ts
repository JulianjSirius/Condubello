import { Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { AuthService } from '../core/auth.service';
import { MODULES } from '../features/crud/modules';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatSidenavModule, MatToolbarModule, MatListModule, MatIconModule, MatButtonModule],
  template: `
    <mat-sidenav-container class="contenedor">
      <mat-sidenav mode="side" opened class="menu">
        <div class="marca"><mat-icon>directions_car</mat-icon> Condubello</div>
        <mat-nav-list>
          <a mat-list-item routerLink="/inicio" routerLinkActive="activo"><mat-icon matListItemIcon>dashboard</mat-icon><span matListItemTitle>Inicio</span></a>
          <a mat-list-item routerLink="/actividades" routerLinkActive="activo"><mat-icon matListItemIcon>event_note</mat-icon><span matListItemTitle>{{ auth.tieneRol('Instructor') ? 'Tareas' : 'Actividades' }}</span></a>
          <a mat-list-item routerLink="/resultados" routerLinkActive="activo"><mat-icon matListItemIcon>insights</mat-icon><span matListItemTitle>Resultados</span></a>
          <a mat-list-item routerLink="/mapa" routerLinkActive="activo"><mat-icon matListItemIcon>map</mat-icon><span matListItemTitle>Mapa</span></a>
          @if (auth.tieneRol('Estudiante')) {
            <a mat-list-item routerLink="/mi-cuenta" routerLinkActive="activo"><mat-icon matListItemIcon>account_circle</mat-icon><span matListItemTitle>Mi cuenta</span></a>
          }
          @for (m of menu(); track m.path) {
            <a mat-list-item [routerLink]="['/', m.path]" routerLinkActive="activo"><mat-icon matListItemIcon>{{ m.icon }}</mat-icon><span matListItemTitle>{{ m.title }}</span></a>
          }
        </mat-nav-list>
      </mat-sidenav>
      <mat-sidenav-content>
        <mat-toolbar>
          <span class="espacio"></span>
          <span class="usuario">{{ auth.sesion()?.nombreCompleto }} · {{ auth.rol() }}</span>
          <button mat-icon-button (click)="auth.logout()" aria-label="Cerrar sesión"><mat-icon>logout</mat-icon></button>
        </mat-toolbar>
        <main class="principal"><router-outlet /></main>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: `
    .contenedor { height: 100vh; }
    .menu { width: 240px; }
    .marca { display: flex; align-items: center; gap: 8px; padding: 20px 16px; font-size: 20px; font-weight: 500; }
    .espacio { flex: 1; }
    .usuario { margin-right: 8px; font-size: 14px; }
    .principal { padding: 24px; }
    .activo { background: var(--mat-sys-secondary-container); }
    @media (max-width: 720px) { .menu { width: 64px; } .menu span[matListItemTitle], .marca { font-size: 0; } }
  `
})
export class ShellComponent {
  auth = inject(AuthService);
  menu = computed(() => MODULES.filter(m => this.auth.tieneRol(...m.leer)));
}
