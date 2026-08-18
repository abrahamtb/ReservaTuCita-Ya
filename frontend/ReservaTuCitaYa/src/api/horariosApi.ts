import { apiRequest, queryString } from './apiClient'
import type { PageResult } from '../types'

export type DiaSemana = 'Lunes' | 'Martes' | 'Miercoles' | 'Jueves' | 'Viernes' | 'Sabado' | 'Domingo'
export type TipoExcepcionHorario = 'CerradoTodoElDia' | 'HorarioEspecial' | 'NoDisponibleParcial'
export interface IntervaloHorario { id?: string; diaSemana: DiaSemana; horaInicio: string; horaFin: string }
export interface HorarioSemanal { intervalos: IntervaloHorario[] }
export interface ExcepcionHorario { id: string; fecha: string; tipoExcepcion: TipoExcepcionHorario; horaInicio?: string | null; horaFin?: string | null; motivo: string; observaciones?: string | null }
export interface ExcepcionRequest { fecha: string; tipoExcepcion: TipoExcepcionHorario; horaInicio?: string | null; horaFin?: string | null; motivo: string; observaciones?: string }

const time = (value?: string | null) => value && value.length === 5 ? `${value}:00` : value

export const obtenerHorarioSede = (sedeId: string, signal?: AbortSignal) => apiRequest<HorarioSemanal>(`/api/sedes/${sedeId}/horarios`, { signal })
export const actualizarHorarioSede = (sedeId: string, intervalos: IntervaloHorario[]) => apiRequest<void>(`/api/sedes/${sedeId}/horarios`, { method: 'PUT', body: JSON.stringify({ intervalos: intervalos.map(({ diaSemana, horaInicio, horaFin }) => ({ diaSemana, horaInicio: time(horaInicio), horaFin: time(horaFin) })) }) })
export const listarExcepcionesSede = (sedeId: string, pagina = 1, signal?: AbortSignal) => apiRequest<PageResult<ExcepcionHorario>>(`/api/sedes/${sedeId}/excepciones-horario${queryString({ pagina, tamanoPagina: 20 })}`, { signal })
export const crearExcepcionSede = (sedeId: string, data: ExcepcionRequest) => apiRequest<string>(`/api/sedes/${sedeId}/excepciones-horario`, { method: 'POST', body: JSON.stringify({ ...data, horaInicio: time(data.horaInicio), horaFin: time(data.horaFin) }) })
export const eliminarExcepcionSede = (id: string) => apiRequest<void>(`/api/excepciones-horario-sede/${id}`, { method: 'DELETE' })
