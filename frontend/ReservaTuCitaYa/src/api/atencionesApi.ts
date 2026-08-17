import { apiRequest, queryString } from './apiClient'
import type {
  AgendaProfesional,
  AtencionDetalle,
  FinalizarAtencionRequest,
  FinalizarAtencionRespuesta,
  IniciarAtencionRespuesta,
  MarcarNoAsistioRespuesta,
  MarcarPresenteRespuesta,
} from '../types'

const base = (organizacionId: string, reservaId: string) =>
  `/api/organizaciones/${organizacionId}/reservas/${reservaId}/atencion`

export const obtenerAgendaProfesional = (
  organizacionId: string,
  profesionalId: string,
  fecha: string,
  signal?: AbortSignal,
) => apiRequest<AgendaProfesional>(
  `/api/organizaciones/${organizacionId}/profesionales/${profesionalId}/agenda${queryString({ fecha })}`,
  { signal },
)

export const obtenerAtencionReserva = (
  organizacionId: string,
  reservaId: string,
  signal?: AbortSignal,
) => apiRequest<AtencionDetalle>(base(organizacionId, reservaId), { signal })

export const marcarPresente = (organizacionId: string, reservaId: string) =>
  apiRequest<MarcarPresenteRespuesta>(`${base(organizacionId, reservaId)}/presencia`, { method: 'POST' })

export const iniciarAtencion = (organizacionId: string, reservaId: string) =>
  apiRequest<IniciarAtencionRespuesta>(`${base(organizacionId, reservaId)}/iniciar`, { method: 'POST' })

export const finalizarAtencion = (
  organizacionId: string,
  reservaId: string,
  request: FinalizarAtencionRequest,
) => apiRequest<FinalizarAtencionRespuesta>(`${base(organizacionId, reservaId)}/finalizar`, {
  method: 'POST',
  body: JSON.stringify(request),
})

export const marcarNoAsistio = (
  organizacionId: string,
  reservaId: string,
) => apiRequest<MarcarNoAsistioRespuesta>(`${base(organizacionId, reservaId)}/no-asistio`, { method: 'POST' })
