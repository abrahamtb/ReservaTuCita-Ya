import { apiRequest, queryString } from './apiClient'
import type { EstadoFiltro, Sede, SedeRequest } from '../types'

import { PaginaResultado } from "../features/empleados/types/Empleado";

export const listSedes = (
  organizationId: string,
  filters: { busqueda?: string; estado?: EstadoFiltro; pagina?: number; tamanoPagina?: number },
  signal?: AbortSignal
) =>
  apiRequest<PaginaResultado<Sede>>(
    `/api/organizaciones/${organizationId}/sedes${queryString(filters)}`,
    { signal }
  );

export const getSede = (id: string, signal?: AbortSignal) => apiRequest<Sede>(`/api/sedes/${id}`, { signal })
export const createSede = (organizationId: string, data: SedeRequest) => apiRequest<Sede>(`/api/organizaciones/${organizationId}/sedes`, { method: 'POST', body: JSON.stringify(data) })
export const updateSede = (id: string, data: SedeRequest) => apiRequest<void>(`/api/sedes/${id}`, { method: 'PUT', body: JSON.stringify(data) })
export const toggleSede = (id: string) => apiRequest<void>(`/api/sedes/${id}/estado`, { method: 'PATCH' })
export const deleteSede = (id: string) => apiRequest<void>(`/api/sedes/${id}`, { method: 'DELETE' })
