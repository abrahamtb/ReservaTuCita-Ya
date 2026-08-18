import { apiRequest, queryString } from './apiClient'
import type { EstadoReserva, PageResult, ReservaDetalle } from '../types'

export interface ReservaLista { id: string; codigo: string; clienteNombre: string; servicioNombre: string; sedeNombre: string; profesionalNombre?: string | null; fecha: string; horaInicio: string; horaFinServicio: string; estado: EstadoReserva; cantidadParticipantes: number }
export interface CrearReservaRequest { clienteId: string; servicioId: string; sedeId: string; profesionalId?: string | null; recursoId?: string | null; fecha: string; horaInicio: string; cantidadParticipantes: number; participantes: { clienteId?: string | null; nombreCompleto: string; esTitular: boolean; observaciones?: string | null }[]; observaciones?: string | null }
export interface ReservaCreada { id: string; codigo: string; estado: EstadoReserva; fecha: string; horaInicio: string }
export type MotivoReprogramacion = 'SolicitudCliente'|'CambioProfesional'|'CambioDisponibilidad'|'EventoInterno'|'Otro'
export type MotivoCancelacion = 'SolicitudCliente'|'CambioPlanes'|'NoDisponibilidad'|'ProblemaOperativo'|'Duplicada'|'Otro'

export const listarReservas = (organizacionId: string, filtros: { sedeId?: string; clienteId?: string; profesionalId?: string; servicioId?: string; estado?: EstadoReserva | ''; desde?: string; hasta?: string; pagina?: number; tamanoPagina?: number }, signal?: AbortSignal) => apiRequest<PageResult<ReservaLista>>(`/api/organizaciones/${organizacionId}/reservas${queryString(filtros)}`, { signal })
export const obtenerReserva = (id: string, signal?: AbortSignal) => apiRequest<ReservaDetalle>(`/api/reservas/${id}`, { signal })
export const crearReserva = (organizacionId: string, data: CrearReservaRequest) => apiRequest<ReservaCreada>(`/api/organizaciones/${organizacionId}/reservas`, { method: 'POST', body: JSON.stringify(data) })
export const reprogramarReserva = (organizacionId: string, id: string, data: { fechaNueva: string; horaInicioNueva: string; profesionalId?: string | null; recursoId?: string | null; motivo: MotivoReprogramacion; observacion?: string }) => apiRequest(`/api/organizaciones/${organizacionId}/reservas/${id}/reprogramacion`, { method: 'PUT', body: JSON.stringify(data) })
export const cancelarReserva = (organizacionId: string, id: string, data: { motivo: MotivoCancelacion; comentario?: string; confirmacion: boolean }) => apiRequest(`/api/organizaciones/${organizacionId}/reservas/${id}/cancelacion`, { method: 'POST', body: JSON.stringify(data) })
