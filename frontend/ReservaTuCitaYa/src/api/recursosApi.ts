import { apiRequest, queryString } from './apiClient'
import type { EstadoFiltro, PageResult } from '../types'

export interface RecursoLista { id: string; sedeId: string; nombre: string; codigo?: string | null; tipoRecurso: string; capacidad: number; ubicacionInterna?: string | null; serviciosCount: number; estaActivo: boolean }
export interface RecursoServicio { id: string; servicioId: string; servicioNombre: string; esObligatorio: boolean; cantidadRequerida: number; estaActivo: boolean }
export interface RecursoDetalle extends RecursoLista { organizacionId: string; sedeNombre: string; descripcion?: string | null; observaciones?: string | null; fechaCreacion: string; fechaModificacion?: string | null; servicios: RecursoServicio[] }
export interface AsignacionServicioRecurso { servicioId: string; esObligatorio: boolean; cantidadRequerida: number }
export interface RecursoRequest { nombre: string; codigo?: string; descripcion?: string; tipoRecurso: string; capacidad: number; ubicacionInterna?: string; observaciones?: string; servicios?: AsignacionServicioRecurso[] }

export const listarRecursos = (sedeId: string, filtros: { busqueda?: string; tipoRecurso?: string; estado?: EstadoFiltro; servicioId?: string; pagina?: number; tamanoPagina?: number }, signal?: AbortSignal) => apiRequest<PageResult<RecursoLista>>(`/api/sedes/${sedeId}/recursos${queryString(filtros)}`, { signal })
export const obtenerRecurso = (id: string, signal?: AbortSignal) => apiRequest<RecursoDetalle>(`/api/recursos/${id}`, { signal })
export const crearRecurso = (sedeId: string, data: RecursoRequest) => apiRequest<RecursoDetalle>(`/api/sedes/${sedeId}/recursos`, { method: 'POST', body: JSON.stringify({ ...data, servicios: data.servicios ?? [] }) })
export const actualizarRecurso = (id: string, data: RecursoRequest) => apiRequest<void>(`/api/recursos/${id}`, { method: 'PUT', body: JSON.stringify(data) })
export const cambiarEstadoRecurso = (id: string, estaActivo: boolean) => apiRequest<void>(`/api/recursos/${id}/estado`, { method: 'PATCH', body: JSON.stringify({ estaActivo }) })
export const eliminarRecurso = (id: string) => apiRequest<void>(`/api/recursos/${id}`, { method: 'DELETE' })
export const reemplazarServiciosRecurso = (id: string, servicios: AsignacionServicioRecurso[]) => apiRequest<void>(`/api/recursos/${id}/servicios`, { method: 'PUT', body: JSON.stringify({ servicios }) })
