import { apiRequest, queryString } from './apiClient'
import type { EstadoFiltro, ModalidadServicio, PageResult, SedeAsignacion, Servicio, ServicioRequest } from '../types'
import { PaginaResultado } from "../features/empleados/types/Empleado";

export const listServices = (
  organizationId: string,
  filters: {
    busqueda?: string;
    categoriaServicioId?: string;
    modalidad?: ModalidadServicio | "";
    estado?: EstadoFiltro;
    pagina?: number;
    tamanoPagina?: number;
  },
  signal?: AbortSignal
) =>
  apiRequest<PaginaResultado<Servicio>>(
    `/api/organizaciones/${organizationId}/servicios${queryString(filters)}`,
    { signal }
  );

export const listServiceSedes = (organizationId: string, serviceId?: string) => apiRequest<SedeAsignacion[]>(`/api/organizaciones/${organizationId}/servicios/sedes-opciones${queryString({ servicioId: serviceId })}`)
export const getService = (id: string, signal?: AbortSignal) => apiRequest<Servicio>(`/api/servicios/${id}`, { signal })
export const createService = (organizationId: string, data: ServicioRequest) => apiRequest<Servicio>(`/api/organizaciones/${organizationId}/servicios`, { method: 'POST', body: JSON.stringify(data) })
export const updateService = (id: string, data: ServicioRequest) => apiRequest<void>(`/api/servicios/${id}`, { method: 'PUT', body: JSON.stringify(data) })
export const toggleService = (id: string) => apiRequest<void>(`/api/servicios/${id}/estado`, { method: 'PATCH' })
export const deleteService = (id: string) => apiRequest<void>(`/api/servicios/${id}`, { method: 'DELETE' })
