// Listado de empleados (para tabla y paginación)
export interface EmpleadoListado {
  id: string;
  nombres: string;
  apellidos: string;
  cargo: string;
  estado: string;
  especialidad?: string;
  tipoDocumento: string;
  numeroDocumento: string;
  correo: string;
  telefono: string;
  cantidadSedes: number;
  cantidadServicios: number;
  esProfesional: boolean;
  sedeId?: string; // para filtros
  servicioId?: string; // para filtros
  estaActivo?: boolean; // para filtros
}


// Detalle completo de un empleado/profesional
export interface EmpleadoDetalle {
  id: string;
  nombres: string;
  apellidos: string;
  tipoDocumento: string;
  numeroDocumento: string;
  correo: string;
  telefono: string;
  direccion?: string;
  fechaNacimiento?: string; // ISO date
  cargo: string;
  especialidad?: string;
  esProfesional: boolean;
  numeroColegiatura?: string;
  observaciones?: string;
  sedes: SedeAsignada[];
  servicios: ServicioAsignado[];
  estado: string;
  organizacionId: string;
  auditoria: {
    creadoPor: string;
    creadoEn: string;
    actualizadoPor?: string;
    actualizadoEn?: string;
  };
}

// Request para crear empleado
export interface CrearEmpleadoRequest {
  tipoDocumento: string;
  numeroDocumento: string;
  nombres: string;
  apellidos: string;
  correo: string;
  telefono: string;
  direccion?: string;
  fechaNacimiento?: string;
  cargo: string;
  especialidad?: string;
  esProfesional: boolean;
  estado: string;
  numeroColegiatura?: string;
  observaciones?: string;
  sedes: { sedeId: string; activa: boolean }[];   // 👈 igual que en edición
  servicios: { servicioId: string; activo: boolean }[];
}

// Request para actualizar empleado
export interface ActualizarEmpleadoRequest extends CrearEmpleadoRequest {
  id: string;
  estado: string;
}

// Filtros para listado
export interface EmpleadoFiltros {
  busqueda: string;
  estado?: string; // activo/inactivo
  esProfesional?: boolean;
  sedeId?: string;
  servicioId?: string;
  pagina: number;
  tamañoPagina: number;
}

// Selector de profesionales (para asignaciones)
export interface ProfesionalSelector {
  id: string;
  nombres: string;
  apellidos: string;
  especialidad?: string;
  estado: string;
}

// Sede asignada
export interface SedeAsignada {
  sedeId: string;
  nombre: string;
  activa: boolean;
}

// Servicio asignado
export interface ServicioAsignado {
  servicioId: string;
  nombre: string;
  activo: boolean;
}

// Resultado paginado
export interface PaginaResultado<T> {
  totalRegistros: number;
  paginaActual: number;
  tamanoPagina: number;
  registros: T[];
}
