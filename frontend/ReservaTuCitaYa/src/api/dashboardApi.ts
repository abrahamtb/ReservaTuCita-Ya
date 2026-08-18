import { apiRequest, queryString } from './apiClient'
import type { DashboardFiltros, DashboardResumen } from '../types'

export const obtenerDashboardResumen = (
  params: DashboardFiltros,
  signal?: AbortSignal,
) => apiRequest<DashboardResumen>(`/api/dashboard${queryString({
  fechaDesde: params.fechaDesde,
  fechaHasta: params.fechaHasta,
  sedeId: params.sedeId,
  organizacionId: params.organizacionId,
})}`, { signal })
