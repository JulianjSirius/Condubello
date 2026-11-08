import { ESTADOS_PRUEBA, ESTADOS_VEHICULO, Rol, TIPOS_LICENCIA } from '../../core/models';

export type FieldType = 'text' | 'email' | 'password' | 'number' | 'datetime' | 'select' | 'checkbox' | 'json' | 'url';

export interface Field {
  key: string;
  label: string;
  type?: FieldType;
  required?: boolean;
  maxLength?: number;
  minLength?: number;
  min?: number;
  max?: number;
  step?: number;
  options?: string[];
  /** Carga las opciones desde otro endpoint (select de relaciones). */
  lookup?: { path: string; label: (x: any) => string };
  /** Solo se muestra al crear. */
  soloCrear?: boolean;
  /** Solo se muestra al editar. */
  soloEditar?: boolean;
}

export interface Column {
  key: string;
  label: string;
  /** Formato de la celda. */
  format?: 'fecha' | 'bool' | 'json' | 'lookup' | 'enum';
  lookup?: { path: string; label: (x: any) => string };
}

export interface RowAction {
  label: string;
  icon: string;
  roles: Rol[];
  /** Ruta PATCH/PUT relativa al módulo; {id} se reemplaza. */
  path: string;
  method: 'PATCH' | 'PUT';
  fields: Field[];
  /** Si se define, la acción solo aparece en las filas que cumplan la condición. */
  visible?: (row: any) => boolean;
  /** Pide confirmación y envía el PATCH/PUT sin formulario. */
  confirmar?: string;
}

/** Botón de la cabecera que dispara un POST sobre todo el módulo (no sobre una fila). */
export interface GlobalAction {
  label: string;
  icon: string;
  roles: Rol[];
  /** Ruta POST relativa al módulo. */
  path: string;
  confirmar: string;
}

export interface ModuleDef {
  path: string;      // ruta de la API y de la app
  title: string;
  singular: string;
  icon: string;
  leer: Rol[];
  crear: Rol[];
  editar: Rol[];
  borrar: Rol[];
  columns: Column[];
  fields: Field[];
  actions?: RowAction[];
  accionGlobal?: GlobalAction;
}

const ADMIN: Rol[] = ['Administrativo'];
const ADMIN_INSTR: Rol[] = ['Administrativo', 'Instructor'];
const TODOS: Rol[] = ['Estudiante', 'Instructor', 'Administrativo'];

const nombre = (x: any) => `${x.nombre} ${x.apellidos} (${x.cedula})`;
const estudiantes = { path: 'estudiantes', label: nombre };
const instructores = { path: 'instructores', label: nombre };
const rutas = { path: 'rutas', label: (x: any) => x.nombre };
const sesiones = {
  path: 'sesiones-practicas',
  label: (x: any) => `${new Date(x.fechaHoraInicio).toLocaleString('es-CO')} · ${x.estadoPrueba}`
};

const persona = (conCargo: boolean): Field[] => [
  { key: 'cedula', label: 'Cédula', required: true, minLength: 5, maxLength: 20 },
  { key: 'nombre', label: 'Nombre', required: true, maxLength: 100 },
  { key: 'apellidos', label: 'Apellidos', required: true, maxLength: 100 },
  { key: 'email', label: 'Email', type: 'email', required: true, maxLength: 150 },
  ...(conCargo ? [{ key: 'cargo', label: 'Cargo', required: true, maxLength: 100 } as Field] : []),
  { key: 'password', label: 'Contraseña', type: 'password', required: true, minLength: 8, maxLength: 100, soloCrear: true },
  { key: 'password', label: 'Nueva contraseña (opcional)', type: 'password', minLength: 8, maxLength: 100, soloEditar: true }
];

const personaCols = (conCargo: boolean): Column[] => [
  { key: 'cedula', label: 'Cédula' },
  { key: 'nombre', label: 'Nombre' },
  { key: 'apellidos', label: 'Apellidos' },
  { key: 'email', label: 'Email' },
  ...(conCargo ? [{ key: 'cargo', label: 'Cargo' } as Column] : [{ key: 'fechaRegistro', label: 'Registro', format: 'fecha' } as Column])
];

export const MODULES: ModuleDef[] = [
  {
    path: 'estudiantes', title: 'Estudiantes', singular: 'estudiante', icon: 'school',
    leer: ADMIN_INSTR, crear: ADMIN, editar: [], borrar: ADMIN,
    columns: [...personaCols(false), { key: 'aprobado', label: 'Aprobado', format: 'bool' }],
    fields: persona(false),
    actions: [{
      label: 'Aprobar registro', icon: 'how_to_reg', roles: ADMIN, path: '{id}/aprobar', method: 'PATCH',
      fields: [], visible: r => !r.aprobado, confirmar: '¿Aprobar este registro? El estudiante podrá iniciar sesión.'
    }, {
      label: 'Restablecer contraseña', icon: 'lock_reset', roles: ADMIN, path: '{id}/password', method: 'PATCH',
      fields: [{ key: 'passwordNueva', label: 'Nueva contraseña', type: 'password', required: true, minLength: 8, maxLength: 100 }]
    }]
  },
  {
    path: 'instructores', title: 'Instructores', singular: 'instructor', icon: 'badge',
    leer: ADMIN_INSTR, crear: ADMIN, editar: ADMIN, borrar: ADMIN,
    columns: personaCols(true), fields: persona(true)
  },
  {
    path: 'administrativos', title: 'Administrativos', singular: 'administrativo', icon: 'admin_panel_settings',
    leer: ADMIN, crear: ADMIN, editar: ADMIN, borrar: ADMIN,
    columns: personaCols(true), fields: persona(true)
  },
  {
    path: 'vehiculos', title: 'Vehículos', singular: 'vehículo', icon: 'directions_car',
    leer: TODOS, crear: ADMIN, editar: ADMIN, borrar: ADMIN,
    columns: [
      { key: 'placa', label: 'Placa' },
      { key: 'modelo', label: 'Modelo' },
      { key: 'estadoActual', label: 'Estado', format: 'enum' }
    ],
    fields: [
      { key: 'placa', label: 'Placa (ABC123 o ABC12D)', required: true, maxLength: 6 },
      { key: 'modelo', label: 'Modelo', required: true, maxLength: 100 },
      { key: 'estadoActual', label: 'Estado', type: 'select', options: ESTADOS_VEHICULO, required: true }
    ],
    actions: [{
      label: 'Cambiar estado', icon: 'sync_alt', roles: ADMIN_INSTR, path: '{id}/estado', method: 'PATCH',
      fields: [{ key: 'estadoActual', label: 'Estado', type: 'select', options: ESTADOS_VEHICULO, required: true }]
    }]
  },
  {
    path: 'rutas', title: 'Rutas', singular: 'ruta', icon: 'alt_route',
    leer: TODOS, crear: ADMIN, editar: ADMIN, borrar: ADMIN,
    columns: [
      { key: 'nombre', label: 'Nombre' },
      { key: 'descripcion', label: 'Descripción' },
      { key: 'geoJson', label: 'GeoJSON', format: 'json' },
      { key: 'activa', label: 'Activa', format: 'bool' }
    ],
    fields: [
      { key: 'nombre', label: 'Nombre', required: true, maxLength: 150 },
      { key: 'descripcion', label: 'Descripción', maxLength: 500 },
      { key: 'geoJson', label: 'Ruta autorizada (GeoJSON)', type: 'json' },
      { key: 'activa', label: 'Activa', type: 'checkbox' }
    ]
  },
  {
    path: 'sesiones-practicas', title: 'Sesiones prácticas', singular: 'sesión práctica', icon: 'event',
    leer: TODOS, crear: ADMIN, editar: ADMIN, borrar: ADMIN,
    columns: [
      { key: 'estudianteId', label: 'Estudiante', format: 'lookup', lookup: estudiantes },
      { key: 'instructorId', label: 'Instructor', format: 'lookup', lookup: instructores },
      { key: 'rutaId', label: 'Ruta', format: 'lookup', lookup: rutas },
      { key: 'fechaHoraInicio', label: 'Inicio', format: 'fecha' },
      { key: 'fechaHoraFin', label: 'Fin', format: 'fecha' },
      { key: 'duracionHoras', label: 'Horas' },
      { key: 'estadoPrueba', label: 'Estado', format: 'enum' },
      { key: 'calificacionInstructor', label: 'Calif. instructor' },
      { key: 'calificacionAlumnoAInstructor', label: 'Calif. alumno' }
    ],
    fields: [
      { key: 'estudianteId', label: 'Estudiante', type: 'select', required: true, lookup: estudiantes },
      { key: 'instructorId', label: 'Instructor', type: 'select', required: true, lookup: instructores },
      { key: 'rutaId', label: 'Ruta autorizada', type: 'select', lookup: rutas },
      { key: 'fechaHoraInicio', label: 'Inicio', type: 'datetime', required: true },
      { key: 'fechaHoraFin', label: 'Fin', type: 'datetime' },
      { key: 'estadoPrueba', label: 'Estado', type: 'select', options: ESTADOS_PRUEBA },
      { key: 'calificacionInstructor', label: 'Calificación del instructor (1-5)', type: 'number', min: 1, max: 5 },
      { key: 'calificacionAlumnoAInstructor', label: 'Calificación alumno → instructor (1-5)', type: 'number', min: 1, max: 5 }
    ]
  },
  {
    path: 'telemetria', title: 'Telemetría', singular: 'telemetría', icon: 'route',
    leer: TODOS, crear: ADMIN, editar: ADMIN, borrar: ADMIN,
    columns: [
      { key: 'sesionId', label: 'Sesión', format: 'lookup', lookup: sesiones },
      { key: 'resumenTelemetria', label: 'Resumen', format: 'json' },
      { key: 'rutaGps', label: 'Ruta GPS', format: 'json' },
      { key: 'alertaDesviacion', label: 'Alerta desviación', format: 'bool' }
    ],
    fields: [
      { key: 'sesionId', label: 'Sesión', type: 'select', required: true, lookup: sesiones, soloCrear: true },
      { key: 'resumenTelemetria', label: 'Resumen (JSON)', type: 'json' },
      { key: 'rutaGps', label: 'Ruta GPS (GeoJSON)', type: 'json' },
      { key: 'alertaDesviacion', label: 'Alerta de desviación', type: 'checkbox' }
    ],
    actions: [{
      label: 'Actualizar ruta GPS', icon: 'map', roles: ADMIN_INSTR, path: '{id}/ruta-gps', method: 'PATCH',
      fields: [{ key: 'rutaGps', label: 'Ruta GPS (GeoJSON)', type: 'json' }]
    }],
    accionGlobal: {
      label: 'Sincronizar GPS', icon: 'gps_fixed', roles: ADMIN, path: 'sincronizar',
      confirmar: '¿Sincronizar el GPS de todas las sesiones en curso y completadas? Se actualizarán la ruta GPS y la alerta de desviación.'
    }
  },
  {
    path: 'validaciones-antifraude', title: 'Antifraude', singular: 'validación', icon: 'verified_user',
    leer: TODOS, crear: ADMIN, editar: ADMIN, borrar: ADMIN,
    columns: [
      { key: 'sesionId', label: 'Sesión', format: 'lookup', lookup: sesiones },
      { key: 'timestamp', label: 'Fecha', format: 'fecha' },
      { key: 'urlFotoCapturada', label: 'Foto' },
      { key: 'estadoValidacion', label: 'Válida', format: 'bool' }
    ],
    fields: [
      { key: 'sesionId', label: 'Sesión', type: 'select', required: true, lookup: sesiones, soloCrear: true },
      { key: 'timestamp', label: 'Fecha y hora', type: 'datetime' },
      { key: 'urlFotoCapturada', label: 'URL de la foto', type: 'url', required: true, maxLength: 500 },
      { key: 'estadoValidacion', label: 'Validación aprobada', type: 'checkbox' }
    ]
  },
  {
    path: 'control-horas', title: 'Control de horas', singular: 'control de horas', icon: 'timer',
    leer: TODOS, crear: ADMIN, editar: ADMIN_INSTR, borrar: ADMIN,
    columns: [
      { key: 'estudianteId', label: 'Estudiante', format: 'lookup', lookup: estudiantes },
      { key: 'tipoLicencia', label: 'Licencia' },
      { key: 'horasRequeridas', label: 'Requeridas' },
      { key: 'horasRealizadas', label: 'Realizadas' },
      { key: 'horasPendientes', label: 'Pendientes' },
      { key: 'completado', label: 'Completado', format: 'bool' }
    ],
    fields: [
      { key: 'estudianteId', label: 'Estudiante', type: 'select', required: true, lookup: estudiantes, soloCrear: true },
      { key: 'tipoLicencia', label: 'Tipo de licencia', type: 'select', options: TIPOS_LICENCIA, required: true },
      { key: 'horasRequeridas', label: 'Horas requeridas (1-500)', type: 'number', required: true, min: 1, max: 500 },
      { key: 'horasRealizadas', label: 'Horas realizadas (0-1000)', type: 'number', required: true, min: 0, max: 1000, step: 0.5 }
    ]
  }
];
