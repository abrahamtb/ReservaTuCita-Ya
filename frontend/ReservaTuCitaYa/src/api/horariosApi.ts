import { apiRequest, queryString } from './apiClient'
import type { PageResult } from '../types'

export type DiaSemana = 'Lunes' | 'Martes' | 'Miercoles' | 'Jueves' | 'Viernes' | 'Sabado' | 'Domingo'
export type TipoExcepcionHorario = 'CerradoTodoElDia' | 'HorarioEspecial' | 'NoDisponibleParcial'
export interface IntervaloHorario { id?: string; diaSemana: DiaSemana; horaInicio: string; horaFin: string }
export interface HorarioSemanal { intervalos: IntervaloHorario[] }
export interface ExcepcionHorario { id: string; fecha: string; tipoExcepcion: TipoExcepcionHorario; horaInicio?: string | null; horaFin?: string | null; motivo: string; observaciones?: string | null }
export interface ExcepcionRequest { fecha: string; tipoExcepcion: TipoExcepcionHorario; horaInicio?: string | null; horaFin?: string | null; motivo: string; observaciones?: string }

const time = (value?: string | null) => value && value.length === 5 ? `${value}:00` : value
const scheduleBody = (intervalos: IntervaloHorario[]) => JSON.stringify({
  intervalos: intervalos.map(({ diaSemana, horaInicio, horaFin }) => ({ diaSemana, horaInicio: time(horaInicio), horaFin: time(horaFin) })),
})
const exceptionBody = (data: ExcepcionRequest) => JSON.stringify({ ...data, horaInicio: time(data.horaInicio), horaFin: time(data.horaFin) })

export const obtenerHorarioSede = (sedeId: string, signal?: AbortSignal) => apiRequest<HorarioSemanal>(`/api/sedes/${sedeId}/horarios`, { signal })
export const actualizarHorarioSede = (sedeId: string, intervalos: IntervaloHorario[]) => apiRequest<void>(`/api/sedes/${sedeId}/horarios`, { method: 'PUT', body: scheduleBody(intervalos) })
export const listarExcepcionesSede = (sedeId: string, pagina = 1, signal?: AbortSignal) => apiRequest<PageResult<ExcepcionHorario>>(`/api/sedes/${sedeId}/excepciones-horario${queryString({ pagina, tamanoPagina: 100 })}`, { signal })
export const crearExcepcionSede = (sedeId: string, data: ExcepcionRequest) => apiRequest<string>(`/api/sedes/${sedeId}/excepciones-horario`, { method: 'POST', body: exceptionBody(data) })
export const eliminarExcepcionSede = (id: string) => apiRequest<void>(`/api/excepciones-horario-sede/${id}`, { method: 'DELETE' })

export const obtenerHorarioProfesional = (profesionalId: string, sedeId: string, signal?: AbortSignal) =>
  apiRequest<HorarioSemanal>(`/api/profesionales/${profesionalId}/horarios${queryString({ sedeId })}`, { signal })
export const actualizarHorarioProfesional = (profesionalId: string, sedeId: string, intervalos: IntervaloHorario[]) =>
  apiRequest<void>(`/api/profesionales/${profesionalId}/sedes/${sedeId}/horarios`, { method: 'PUT', body: scheduleBody(intervalos) })
export const listarExcepcionesProfesional = (profesionalId: string, signal?: AbortSignal) =>
  apiRequest<PageResult<ExcepcionHorario>>(`/api/profesionales/${profesionalId}/excepciones-horario${queryString({ pagina: 1, tamanoPagina: 100 })}`, { signal })
export const crearExcepcionProfesional = (profesionalId: string, sedeId: string, data: ExcepcionRequest) =>
  apiRequest<string>(`/api/profesionales/${profesionalId}/sedes/${sedeId}/excepciones-horario`, { method: 'POST', body: exceptionBody(data) })
export const eliminarExcepcionProfesional = (id: string) => apiRequest<void>(`/api/excepciones-horario-profesional/${id}`, { method: 'DELETE' })

export const obtenerHorarioRecurso = (recursoId: string, signal?: AbortSignal) =>
  apiRequest<HorarioSemanal>(`/api/recursos/${recursoId}/horarios`, { signal })
export const actualizarHorarioRecurso = (recursoId: string, intervalos: IntervaloHorario[]) =>
  apiRequest<void>(`/api/recursos/${recursoId}/horarios`, { method: 'PUT', body: scheduleBody(intervalos) })
export const listarExcepcionesRecurso = (recursoId: string, signal?: AbortSignal) =>
  apiRequest<PageResult<ExcepcionHorario>>(`/api/recursos/${recursoId}/excepciones-horario${queryString({ pagina: 1, tamanoPagina: 100 })}`, { signal })
export const crearExcepcionRecurso = (recursoId: string, data: ExcepcionRequest) =>
  apiRequest<string>(`/api/recursos/${recursoId}/excepciones-horario`, { method: 'POST', body: exceptionBody(data) })
export const eliminarExcepcionRecurso = (id: string) => apiRequest<void>(`/api/excepciones-horario-recurso/${id}`, { method: 'DELETE' })
