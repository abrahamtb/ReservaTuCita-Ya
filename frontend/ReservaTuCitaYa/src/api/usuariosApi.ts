import { apiRequest } from './apiClient'

export interface UsuarioDto {
  id: string
  email: string
  nombres: string
  apellidos: string
  estaActivo: boolean
  roles: string[]
  organizacionId?: string | null
}

export interface CrearUsuarioRequest {
  email: string
  password: string
  nombres: string
  apellidos: string
  numeroDocumento: string
  telefono: string
  rol: string
  organizacionId?: string | null
  clienteId?: string | null
  empleadoId?: string | null
}

export const listarUsuarios = (signal?: AbortSignal) =>
  apiRequest<UsuarioDto[]>('/api/usuarios', { signal })

export const crearUsuario = (request: CrearUsuarioRequest) =>
  apiRequest<UsuarioDto>('/api/usuarios', {
    method: 'POST',
    body: JSON.stringify(request),
  })

export const cambiarEstadoUsuario = (id: string) =>
  apiRequest<void>(`/api/usuarios/${id}/estado`, { method: 'PATCH' })

export const asignarRolUsuario = (id: string, rol: string) =>
  apiRequest<void>(`/api/usuarios/${id}/roles`, {
    method: 'PUT',
    body: JSON.stringify({ rol }),
  })
