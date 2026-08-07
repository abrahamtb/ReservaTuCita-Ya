import { apiRequest, queryString } from './apiClient'
import type { Categoria, CategoriaRequest, EstadoFiltro, Option, PageResult } from '../types'

export const listCategories = (organizationId: string, filters: { busqueda?: string; estado?: EstadoFiltro; pagina?: number; tamanoPagina?: number }, signal?: AbortSignal) =>
  apiRequest<PageResult<Categoria>>(`/api/organizaciones/${organizationId}/categorias${queryString(filters)}`, { signal })
export const listCategoryOptions = (organizationId: string) => apiRequest<Option[]>(`/api/organizaciones/${organizationId}/categorias/opciones`)
export const getCategory = (id: string, signal?: AbortSignal) => apiRequest<Categoria>(`/api/categorias/${id}`, { signal })
export const createCategory = (organizationId: string, data: CategoriaRequest) => apiRequest<Categoria>(`/api/organizaciones/${organizationId}/categorias`, { method: 'POST', body: JSON.stringify(data) })
export const updateCategory = (id: string, data: CategoriaRequest) => apiRequest<void>(`/api/categorias/${id}`, { method: 'PUT', body: JSON.stringify(data) })
export const toggleCategory = (id: string, confirmarServiciosActivos: boolean) => apiRequest<void>(`/api/categorias/${id}/estado`, { method: 'PATCH', body: JSON.stringify({ confirmarServiciosActivos }) })
export const deleteCategory = (id: string) => apiRequest<void>(`/api/categorias/${id}`, { method: 'DELETE' })
