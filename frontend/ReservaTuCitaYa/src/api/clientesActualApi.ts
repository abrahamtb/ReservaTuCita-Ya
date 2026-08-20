import { apiRequest, queryString } from './apiClient'
import type { EstadoFiltro, PageResult } from '../types'

export type TipoDocumentoCliente = 'NoDefinido' | 'DNI' | 'CarnetExtranjeria' | 'Pasaporte' | 'RUC'
export interface ClienteLista { id: string; organizacionId: string; tipoDocumento: TipoDocumentoCliente; numeroDocumento: string; nombres: string; apellidos: string; nombreCompleto: string; correo?: string | null; telefono?: string | null; estaActivo: boolean }
export interface ClienteDetalle extends ClienteLista { direccion?: string | null; fechaNacimiento?: string | null; observaciones?: string | null; fechaCreacion: string; fechaModificacion?: string | null }
export interface ClienteRequest { tipoDocumento: TipoDocumentoCliente; numeroDocumento: string; nombres: string; apellidos: string; correo?: string | null; telefono?: string | null; direccion?: string | null; fechaNacimiento?: string | null; observaciones?: string | null }
export const listarClientesActual = (organizacionId: string, filtros: { busqueda?: string; estado?: EstadoFiltro; pagina?: number; tamanoPagina?: number }, signal?: AbortSignal) => apiRequest<PageResult<ClienteLista>>(`/api/organizaciones/${organizacionId}/clientes${queryString(filtros)}`, { signal })
export const obtenerClienteActual = (id: string, signal?: AbortSignal) => apiRequest<ClienteDetalle>(`/api/clientes/${id}`, { signal })
export const crearClienteActual = (organizacionId: string, data: ClienteRequest) => apiRequest<ClienteDetalle>(`/api/organizaciones/${organizacionId}/clientes`, { method: 'POST', body: JSON.stringify(data) })
export const actualizarClienteActual = (id: string, data: ClienteRequest) => apiRequest<void>(`/api/clientes/${id}`, { method: 'PUT', body: JSON.stringify(data) })
export const cambiarEstadoClienteActual = (id: string, estaActivo: boolean) => apiRequest<void>(`/api/clientes/${id}/estado`, { method: 'PATCH', body: JSON.stringify({ estaActivo }) })
export const eliminarClienteActual = (id: string) => apiRequest<void>(`/api/clientes/${id}`, { method: 'DELETE' })
