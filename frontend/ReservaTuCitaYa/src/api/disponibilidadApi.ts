import { apiRequest, queryString } from './apiClient'

export interface HorarioDisponible {
  horaInicio: string
  horaFinServicio: string
  horaFinOcupacion: string
  profesionalId?: string | null
  profesionalNombre?: string | null
  recursoId?: string | null
  recursoNombre?: string | null
  capacidadDisponible?: number | null
}
export interface DiaDisponible { fecha: string; estaDisponible: boolean; horarios: HorarioDisponible[] }
export interface DisponibilidadRespuesta {
  sedeId: string
  servicioId: string
  duracionMinutos: number
  tiempoPreparacionMinutos: number
  tiempoPosteriorMinutos: number
  dias: DiaDisponible[]
}
export interface ProfesionalDisponible { id: string; nombreCompleto: string }
export interface RecursoDisponible { id: string; nombre: string }

export function consultarDisponibilidad(params: { sedeId: string; servicioId: string; fechaDesde: string; fechaHasta: string; profesionalId?: string; recursoId?: string }, signal?: AbortSignal) {
  return apiRequest<DisponibilidadRespuesta>(`/api/disponibilidad${queryString(params)}`, { signal })
}
export function profesionalesCompatibles(sedeId: string, servicioId: string, fecha?: string, signal?: AbortSignal) {
  return apiRequest<ProfesionalDisponible[]>(`/api/disponibilidad/profesionales${queryString({ sedeId, servicioId, fecha })}`, { signal })
}
export function recursosCompatibles(sedeId: string, servicioId: string, fecha?: string, signal?: AbortSignal) {
  return apiRequest<RecursoDisponible[]>(`/api/disponibilidad/recursos${queryString({ sedeId, servicioId, fecha })}`, { signal })
}
