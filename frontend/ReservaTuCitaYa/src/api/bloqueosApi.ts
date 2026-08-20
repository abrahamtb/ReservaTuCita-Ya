import { apiRequest } from './apiClient'

export type TipoBloqueo = 'Feriado' | 'Mantenimiento' | 'Vacaciones' | 'Personal'

export interface BloqueoRecursoDto {
  id: string
  recursoId: string
  fechaHoraInicio: string
  fechaHoraFin: string
  tipoBloqueo: TipoBloqueo
  motivo: string
  observaciones?: string | null
}

export interface BloqueoRecursoRequest {
  fechaHoraInicio: string
  fechaHoraFin: string
  tipoBloqueo: TipoBloqueo
  motivo: string
  observaciones?: string | null
}

export const listarBloqueosRecurso = (recursoId: string, signal?: AbortSignal) =>
  apiRequest<BloqueoRecursoDto[]>(`/api/recursos/${recursoId}/bloqueos`, { signal })

export const crearBloqueoRecurso = (recursoId: string, request: BloqueoRecursoRequest) =>
  apiRequest<string>(`/api/recursos/${recursoId}/bloqueos`, {
    method: 'POST',
    body: JSON.stringify(request),
  })

export const actualizarBloqueoRecurso = (id: string, request: BloqueoRecursoRequest) =>
  apiRequest<void>(`/api/bloqueos/${id}`, {
    method: 'PUT',
    body: JSON.stringify(request),
  })

export const eliminarBloqueoRecurso = (id: string) =>
  apiRequest<void>(`/api/bloqueos/${id}`, { method: 'DELETE' })
