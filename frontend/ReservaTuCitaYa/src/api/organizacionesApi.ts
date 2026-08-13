import { apiRequest, queryString } from './apiClient'
import type { EstadoFiltro, Option, Organization, OrganizationRequest, PageResult } from '../types'

export const listOrganizations = (filters: { busqueda?: string; estado?: EstadoFiltro; pagina?: number; tamanoPagina?: number }, signal?: AbortSignal) =>
  apiRequest<PageResult<Organization>>(`/api/organizaciones${queryString(filters)}`, { signal })
export const getOrganization = (id: string, signal?: AbortSignal) => apiRequest<Organization>(`/api/organizaciones/${id}`, { signal })
export const listOrganizationTypes = () => apiRequest<Option[]>('/api/organizaciones/tipos')
export const createOrganization = (data: OrganizationRequest) => apiRequest<Organization>('/api/organizaciones', { method: 'POST', body: JSON.stringify(data) })
export const updateOrganization = (id: string, data: OrganizationRequest) => apiRequest<void>(`/api/organizaciones/${id}`, { method: 'PUT', body: JSON.stringify(data) })
export const toggleOrganization = (id: string) => apiRequest<void>(`/api/organizaciones/${id}/estado`, { method: 'PATCH' })
export const deleteOrganization = (id: string) => apiRequest<void>(`/api/organizaciones/${id}`, { method: 'DELETE' })
