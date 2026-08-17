import { apiRequest, queryString } from './apiClient'
import type { EmpleadoLista, PageResult } from '../types'

export const listarProfesionales = (organizacionId: string, signal?: AbortSignal) =>
  apiRequest<PageResult<EmpleadoLista>>(
    `/api/organizaciones/${organizacionId}/empleados${queryString({
      esProfesional: true,
      estado: 'Activos',
      pagina: 1,
      tamanoPagina: 100,
    })}`,
    { signal },
  )
