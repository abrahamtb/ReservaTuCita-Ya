import { apiRequest, queryString } from './apiClient'

export interface CalificacionDto {
  id: string
  reservaId: string
  puntuacion: number
  comentario?: string | null
  fechaCalificacion: string
}

export interface DistribucionEstrellasDto {
  estrellas: number
  cantidad: number
}

export interface ResumenProfesionalDto {
  profesionalId: string
  profesionalNombre: string
  promedio?: number | null
  totalCalificaciones: number
  distribucion: DistribucionEstrellasDto[]
}

export const crearCalificacion = (reservaId: string, puntuacion: number, comentario?: string) =>
  apiRequest<CalificacionDto>(`/api/calificaciones/reservas/${reservaId}`, {
    method: 'POST',
    body: JSON.stringify({ puntuacion, comentario }),
  })

export const obtenerCalificacionReserva = (reservaId: string, signal?: AbortSignal) =>
  apiRequest<CalificacionDto>(`/api/calificaciones/reservas/${reservaId}`, { signal })

export const obtenerResumenProfesional = (profesionalId: string, signal?: AbortSignal) =>
  apiRequest<ResumenProfesionalDto>(`/api/calificaciones/profesionales/${profesionalId}/resumen`, { signal })

export const listarCalificacionesProfesional = (
  profesionalId: string,
  params: { pagina?: number; tamanoPagina?: number; puntuacion?: number } = {},
  signal?: AbortSignal,
) => apiRequest<CalificacionDto[]>(
  `/api/calificaciones/profesionales/${profesionalId}${queryString(params)}`,
  { signal },
)
