import { apiRequest, queryString } from './apiClient'
import type { EmpleadoLista, EstadoFiltro, PageResult } from '../types'
export type { EmpleadoLista } from '../types'

export type TipoDocumentoEmpleado = 'NoDefinido' | 'DNI' | 'CarnetExtranjeria' | 'Pasaporte' | 'RUC'

export interface EmpleadoSede {
  id: string
  sedeId: string
  nombre: string
  estaActivo: boolean
}

export interface ProfesionalServicio {
  id: string
  servicioId: string
  nombre: string
  estaActivo: boolean
}

export interface EmpleadoDetalle extends EmpleadoLista {
  tipoDocumento: TipoDocumentoEmpleado
  direccion?: string | null
  fechaNacimiento?: string | null
  numeroColegiatura?: string | null
  observaciones?: string | null
  fechaCreacion: string
  fechaModificacion?: string | null
  creadoPorUsuarioId?: string | null
  modificadoPorUsuarioId?: string | null
  sedes: EmpleadoSede[]
  servicios: ProfesionalServicio[]
}

export interface EmpleadoRequest {
  tipoDocumento: TipoDocumentoEmpleado
  numeroDocumento: string
  nombres: string
  apellidos: string
  correo?: string | null
  telefono?: string | null
  direccion?: string | null
  fechaNacimiento?: string | null
  cargo: string
  especialidad?: string | null
  esProfesional: boolean
  numeroColegiatura?: string | null
  observaciones?: string | null
}

export interface CrearEmpleadoRequest extends EmpleadoRequest {
  sedeIds: string[]
  servicioIds: string[]
}

export interface EmpleadoFiltros {
  busqueda?: string
  tipoDocumento?: TipoDocumentoEmpleado | ''
  esProfesional?: boolean
  estado?: EstadoFiltro
  sedeId?: string
  servicioId?: string
  pagina?: number
  tamanoPagina?: number
}

export const listarEmpleados = (
  organizacionId: string,
  filtros: EmpleadoFiltros = {},
  signal?: AbortSignal,
) => apiRequest<PageResult<EmpleadoLista>>(
  `/api/organizaciones/${organizacionId}/empleados${queryString({
    busqueda: filtros.busqueda,
    tipoDocumento: filtros.tipoDocumento,
    esProfesional: filtros.esProfesional,
    estado: filtros.estado,
    sedeId: filtros.sedeId,
    servicioId: filtros.servicioId,
    pagina: filtros.pagina,
    tamanoPagina: filtros.tamanoPagina,
  })}`,
  { signal },
)

export const listarProfesionales = (organizacionId: string, signal?: AbortSignal) =>
  listarEmpleados(organizacionId, {
    esProfesional: true,
    estado: 'Activos',
    pagina: 1,
    tamanoPagina: 100,
  }, signal)

export const obtenerEmpleado = (id: string, signal?: AbortSignal) =>
  apiRequest<EmpleadoDetalle>(`/api/empleados/${id}`, { signal })

export const crearEmpleado = (organizacionId: string, request: CrearEmpleadoRequest) =>
  apiRequest<EmpleadoDetalle>(`/api/organizaciones/${organizacionId}/empleados`, {
    method: 'POST',
    body: JSON.stringify(request),
  })

export const actualizarEmpleado = (id: string, request: EmpleadoRequest) =>
  apiRequest<void>(`/api/empleados/${id}`, {
    method: 'PUT',
    body: JSON.stringify(request),
  })

export const cambiarEstadoEmpleado = (id: string, estaActivo: boolean) =>
  apiRequest<void>(`/api/empleados/${id}/estado`, {
    method: 'PATCH',
    body: JSON.stringify({ estaActivo }),
  })

export const eliminarEmpleado = (id: string) =>
  apiRequest<void>(`/api/empleados/${id}`, { method: 'DELETE' })

export const listarSedesEmpleado = (id: string, signal?: AbortSignal) =>
  apiRequest<EmpleadoSede[]>(`/api/empleados/${id}/sedes`, { signal })

export const reemplazarSedesEmpleado = (id: string, sedeIds: string[]) =>
  apiRequest<void>(`/api/empleados/${id}/sedes`, {
    method: 'PUT',
    body: JSON.stringify({ sedeIds }),
  })

export const listarServiciosProfesional = (id: string, signal?: AbortSignal) =>
  apiRequest<ProfesionalServicio[]>(`/api/empleados/${id}/servicios`, { signal })

export const reemplazarServiciosProfesional = (id: string, servicioIds: string[]) =>
  apiRequest<void>(`/api/empleados/${id}/servicios`, {
    method: 'PUT',
    body: JSON.stringify({ servicioIds }),
  })
