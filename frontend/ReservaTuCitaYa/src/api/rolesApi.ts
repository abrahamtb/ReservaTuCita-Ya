import { apiRequest } from './apiClient'

export interface PermisoDto {
  id: string
  codigo: string
  nombre: string
  descripcion?: string | null
}

export const listarRoles = (signal?: AbortSignal) =>
  apiRequest<string[]>('/api/roles', { signal })

export const listarPermisos = (signal?: AbortSignal) =>
  apiRequest<PermisoDto[]>('/api/roles/permisos', { signal })

export const obtenerPermisosRol = (rol: string, signal?: AbortSignal) =>
  apiRequest<string[]>(`/api/roles/${encodeURIComponent(rol)}/permisos`, { signal })

export const guardarPermisosRol = (rol: string, permisos: string[]) =>
  apiRequest<void>(`/api/roles/${encodeURIComponent(rol)}/permisos`, {
    method: 'PUT',
    body: JSON.stringify({ permisos }),
  })
