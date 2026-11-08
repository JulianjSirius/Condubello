export type Rol = 'Estudiante' | 'Instructor' | 'Administrativo';

export interface ApiError { field: string; message: string; }

export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data: T;
  errors?: ApiError[];
  warnings?: string[];
}

export interface LoginResponse {
  token: string;
  expiraEn: string;
  usuarioId: string;
  nombreCompleto: string;
  email: string;
  rol: Rol;
}

export interface Sesion {
  token: string;
  expiraEn: string;
  usuarioId: string;
  nombreCompleto: string;
  email: string;
  rol: Rol;
}

export const ESTADOS_PRUEBA = ['PROGRAMADA', 'EN_CURSO', 'COMPLETADA', 'CANCELADA', 'FRAUDE_DETECTADO'];
export const ESTADOS_VEHICULO = ['DISPONIBLE', 'EN_CLASE', 'EN_MANTENIMIENTO'];
export const TIPOS_LICENCIA = ['A1', 'A2', 'B1', 'B2', 'B3', 'C1', 'C2', 'C3'];
