import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DatePipe } from '@angular/common';
import { ApiService } from '../../core/crud.service';
import { FormDialogComponent } from '../crud/form-dialog.component';

@Component({
  selector: 'app-mi-cuenta',
  imports: [MatCardModule, MatButtonModule, DatePipe],
  template: `
    <h1>Mi cuenta</h1>
    @if (yo(); as e) {
      <mat-card>
        <mat-card-content>
          <p><strong>Nombre:</strong> {{ e.nombre }} {{ e.apellidos }}</p>
          <p><strong>Cédula:</strong> {{ e.cedula }}</p>
          <p><strong>Email:</strong> {{ e.email }}</p>
          <p><strong>Registro:</strong> {{ e.fechaRegistro | date: 'dd/MM/yyyy' }}</p>
        </mat-card-content>
        <mat-card-actions><button mat-flat-button (click)="editar()">Editar email / contraseña</button></mat-card-actions>
      </mat-card>
    }
  `
})
export class MiCuentaComponent {
  private api = inject(ApiService);
  private dialog = inject(MatDialog);
  private snack = inject(MatSnackBar);
  yo = signal<any>(null);

  constructor() { this.cargar(); }

  cargar() { this.api.get('estudiantes/me').subscribe(e => this.yo.set(e)); }

  editar() {
    this.dialog.open(FormDialogComponent, {
      data: {
        title: 'Actualizar mi cuenta',
        value: this.yo(),
        fields: [
          { key: 'email', label: 'Email', type: 'email', maxLength: 150 },
          { key: 'passwordActual', label: 'Contraseña actual', type: 'password', required: true },
          { key: 'passwordNueva', label: 'Nueva contraseña (opcional)', type: 'password', minLength: 8, maxLength: 100 }
        ],
        submit: (b: any) => this.api.put('estudiantes/me/cuenta', b)
      }
    }).afterClosed().subscribe(ok => { if (ok) { this.snack.open('Cuenta actualizada', 'OK', { duration: 3000 }); this.cargar(); } });
  }
}
