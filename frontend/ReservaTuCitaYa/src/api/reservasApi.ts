import { apiRequest } from './apiClient'
import type { ReservaDetalle } from '../types'

export const obtenerReserva = (id: string, signal?: AbortSignal) =>
  apiRequest<ReservaDetalle>(`/api/reservas/${id}`, { signal })
