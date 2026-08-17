import { apiDownload, apiRequest, queryString } from './apiClient'
import type {
  ReporteAtencionesFiltros,
  ReporteAtencionesResponse,
  ReporteIngresosFiltros,
  ReporteIngresosResponse,
  ReporteReservasFiltros,
  ReporteReservasResponse,
} from '../types'

const parametrosBase = (filtros: ReporteReservasFiltros | ReporteIngresosFiltros | ReporteAtencionesFiltros) => ({
  fechaDesde: filtros.fechaDesde,
  fechaHasta: filtros.fechaHasta,
  sedeId: filtros.sedeId,
  organizacionId: filtros.organizacionId,
})

const parametrosReservas = (filtros: ReporteReservasFiltros, paginar: boolean) => ({
  ...parametrosBase(filtros),
  profesionalId: filtros.profesionalId,
  servicioId: filtros.servicioId,
  estado: filtros.estado,
  clienteId: filtros.clienteId,
  pagina: paginar ? filtros.pagina : undefined,
  tamanoPagina: paginar ? filtros.tamanoPagina : undefined,
})

const parametrosIngresos = (filtros: ReporteIngresosFiltros, paginar: boolean) => ({
  ...parametrosBase(filtros),
  metodoPagoId: filtros.metodoPagoId,
  pagina: paginar ? filtros.pagina : undefined,
  tamanoPagina: paginar ? filtros.tamanoPagina : undefined,
})

const parametrosAtenciones = (filtros: ReporteAtencionesFiltros, paginar: boolean) => ({
  ...parametrosBase(filtros),
  profesionalId: filtros.profesionalId,
  servicioId: filtros.servicioId,
  estado: filtros.estado,
  resultado: filtros.resultado,
  pagina: paginar ? filtros.pagina : undefined,
  tamanoPagina: paginar ? filtros.tamanoPagina : undefined,
})

export const obtenerReporteReservas = (filtros: ReporteReservasFiltros, signal?: AbortSignal) =>
  apiRequest<ReporteReservasResponse>(`/api/reportes/reservas${queryString(parametrosReservas(filtros, true))}`, { signal })

export const exportarReporteReservas = (filtros: ReporteReservasFiltros, signal?: AbortSignal) =>
  apiDownload(`/api/reportes/reservas/exportar${queryString(parametrosReservas(filtros, false))}`, signal)

export const obtenerReporteIngresos = (filtros: ReporteIngresosFiltros, signal?: AbortSignal) =>
  apiRequest<ReporteIngresosResponse>(`/api/reportes/ingresos${queryString(parametrosIngresos(filtros, true))}`, { signal })

export const exportarReporteIngresos = (filtros: ReporteIngresosFiltros, signal?: AbortSignal) =>
  apiDownload(`/api/reportes/ingresos/exportar${queryString(parametrosIngresos(filtros, false))}`, signal)

export const obtenerReporteAtenciones = (filtros: ReporteAtencionesFiltros, signal?: AbortSignal) =>
  apiRequest<ReporteAtencionesResponse>(`/api/reportes/atenciones${queryString(parametrosAtenciones(filtros, true))}`, { signal })

export const exportarReporteAtenciones = (filtros: ReporteAtencionesFiltros, signal?: AbortSignal) =>
  apiDownload(`/api/reportes/atenciones/exportar${queryString(parametrosAtenciones(filtros, false))}`, signal)

export function descargarArchivo(blob: Blob, filename: string) {
  const url = URL.createObjectURL(blob)
  const enlace = document.createElement('a')
  enlace.href = url
  enlace.download = filename
  document.body.appendChild(enlace)
  enlace.click()
  enlace.remove()
  URL.revokeObjectURL(url)
}
