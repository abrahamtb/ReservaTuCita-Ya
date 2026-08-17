import { apiRequest, queryString } from './apiClient'
import type { EmpleadoOpcion, PageResult } from '../types'

export const listarProfesionales = (organizacionId: string, signal?: AbortSignal) =>
  apiRequest<PageResult<EmpleadoOpcion>>(`/api/organizaciones/${organizacionId}/empleados${queryString({
    esProfesional: true,
    estado: 'Activos',
    pagina: 1,
    tamanoPagina: 100,
  })}`, { signal })
