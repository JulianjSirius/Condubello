import { Routes } from '@angular/router';
import { authGuard, rolGuard } from './core/guards';
import { CrudPageComponent } from './features/crud/crud-page.component';
import { MODULES } from './features/crud/modules';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent) },
  { path: 'registro', loadComponent: () => import('./features/login/registro.component').then(m => m.RegistroComponent) },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/shell.component').then(m => m.ShellComponent),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'inicio' },
      { path: 'inicio', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      { path: 'actividades', loadComponent: () => import('./features/actividades/actividades.component').then(m => m.ActividadesComponent) },
      { path: 'resultados', loadComponent: () => import('./features/resultados/resultados.component').then(m => m.ResultadosComponent) },
      { path: 'mapa', loadComponent: () => import('./features/mapa/mapa.component').then(m => m.MapaComponent) },
      {
        path: 'mi-cuenta',
        canActivate: [rolGuard('Estudiante')],
        loadComponent: () => import('./features/cuenta/mi-cuenta.component').then(m => m.MiCuentaComponent)
      },
      ...MODULES.map(def => ({
        path: def.path,
        canActivate: [rolGuard(...def.leer)],
        component: CrudPageComponent,
        data: { def }
      }))
    ]
  },
  { path: '**', redirectTo: '' }
];
