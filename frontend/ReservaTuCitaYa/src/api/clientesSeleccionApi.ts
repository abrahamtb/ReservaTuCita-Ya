import { apiRequest, queryString } from './apiClient'
import type { PageResult } from '../types'

export interface ClienteOpcion { id: string; nombreCompleto: string; numeroDocumento: string; estaActivo: boolean }
export const listarClientesOrganizacion = (organizacionId: string, busqueda = '', signal?: AbortSignal) => apiRequest<PageResult<ClienteOpcion>>(`/api/organizaciones/${organizacionId}/clientes${queryString({ busqueda, estado: 'Activos', pagina: 1, tamanoPagina: 100 })}`, { signal })
