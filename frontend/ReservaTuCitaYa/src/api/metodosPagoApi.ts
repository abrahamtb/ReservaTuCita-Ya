import { apiRequest } from './apiClient'
import type { MetodoPagoOpcion } from '../types'

export const listarMetodosPago = (signal?: AbortSignal) =>
  apiRequest<MetodoPagoOpcion[]>('/api/metodospago', { signal })
