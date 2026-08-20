import { useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { ApiError } from '../../api/apiClient'
import { descargarArchivo, exportarReporteReservas, obtenerReporteReservas } from '../../api/reportesApi'
import type { EstadoReserva, ReporteReservasFiltros, ReporteReservasResponse } from '../../types'
import { DateRangeFilter, EmptyReport, money, parseDate, ReportKpi, ReportLoading, ReportPagination, ReservationBadge, SelectFilter, shortDate, validateRange, type SelectOption } from './ReportShared'

const states: { id: EstadoReserva; nombre: string }[] = [
  { id: 'Pendiente', nombre: 'Pendiente' }, { id: 'Confirmada', nombre: 'Confirmada' },
  { id: 'Reprogramada', nombre: 'Reprogramada' }, { id: 'Presente', nombre: 'Presente' },
  { id: 'EnAtencion', nombre: 'En atención' }, { id: 'Atendida', nombre: 'Atendida' },
  { id: 'Cancelada', nombre: 'Cancelada' }, { id: 'NoAsistio', nombre: 'No asistió' },
]

function defaultRange() {
  const hasta = new Date()
  const desde = new Date(hasta)
  desde.setDate(desde.getDate() - 29)
  const local = (date: Date) => new Date(date.getTime() - date.getTimezoneOffset() * 60_000).toISOString().slice(0, 10)
  return { fechaDesde: local(desde), fechaHasta: local(hasta) }
}

const initial = (organizacionId: string): ReporteReservasFiltros => ({ ...defaultRange(), organizacionId, pagina: 1, tamanoPagina: 10 })

export function ReservationsReport({ organizacionId, sedes, profesionales, servicios }: {
  organizacionId: string; sedes: SelectOption[]; profesionales: SelectOption[]; servicios: SelectOption[]
}) {
  const navigate = useNavigate()
  const [draft, setDraft] = useState(() => initial(organizacionId))
  const [applied, setApplied] = useState(() => initial(organizacionId))
  const [data, setData] = useState<ReporteReservasResponse>()
  const [loading, setLoading] = useState(true)
  const [exporting, setExporting] = useState(false)
  const [error, setError] = useState('')
  const [validation, setValidation] = useState('')

  useEffect(() => { const next = initial(organizacionId); setDraft(next); setApplied(next) }, [organizacionId])
  useEffect(() => {
    const controller = new AbortController()
    setLoading(true); setError('')
    obtenerReporteReservas(applied, controller.signal).then(setData).catch(caught => {
      if (controller.signal.aborted) return
      if (caught instanceof ApiError && caught.status === 403) return navigate('/acceso-denegado', { replace: true })
      setError(caught instanceof Error ? caught.message : 'No se pudo cargar el reporte de reservas.')
    }).finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [applied, navigate])

  function apply(event: FormEvent) {
    event.preventDefault()
    const message = validateRange(draft.fechaDesde, draft.fechaHasta)
    setValidation(message)
    if (!message) setApplied({ ...draft, pagina: 1 })
  }
  function clear() { const next = initial(organizacionId); setDraft(next); setValidation(''); setApplied(next) }
  async function exportCsv() {
    setExporting(true); setError('')
    try {
      const file = await exportarReporteReservas(applied)
      descargarArchivo(file.blob, file.filename ?? `reporte-reservas-${applied.fechaDesde}-${applied.fechaHasta}.csv`)
    } catch (caught) {
      if (caught instanceof ApiError && caught.status === 403) return navigate('/acceso-denegado', { replace: true })
      setError(caught instanceof ApiError && caught.status === 409 ? 'El reporte contiene demasiados registros. Reduce el rango o aplica más filtros.' : caught instanceof Error ? caught.message : 'No se pudo exportar el reporte.')
    } finally { setExporting(false) }
  }
  const set = <K extends keyof ReporteReservasFiltros>(key: K, value: ReporteReservasFiltros[K]) => setDraft(current => ({ ...current, [key]: value }))
  const max = Math.max(...(data?.reservasPorEstado.map(item => item.cantidad) ?? [1]), 1)

  return <div className="reports-section">
    <form className="report-filters" onSubmit={apply}>
      <DateRangeFilter desde={draft.fechaDesde} hasta={draft.fechaHasta} onDesde={value => set('fechaDesde', value)} onHasta={value => set('fechaHasta', value)} />
      <SelectFilter label="Sede" value={draft.sedeId} options={sedes} emptyLabel="Todas las sedes" onChange={value => set('sedeId', value || undefined)} />
      <SelectFilter label="Profesional" value={draft.profesionalId} options={profesionales} emptyLabel="Todos los profesionales" onChange={value => set('profesionalId', value || undefined)} />
      <SelectFilter label="Servicio" value={draft.servicioId} options={servicios} emptyLabel="Todos los servicios" onChange={value => set('servicioId', value || undefined)} />
      <SelectFilter label="Estado" value={draft.estado} options={states} emptyLabel="Todos los estados" onChange={value => set('estado', value as EstadoReserva || '')} />
      {validation ? <p className="report-validation">{validation}</p> : null}
      <div className="report-filter-actions"><button className="btn btn-primary" disabled={loading}>Aplicar filtros</button><button type="button" className="btn btn-outline-secondary" onClick={clear}>Limpiar</button><button type="button" className="btn btn-outline-success" disabled={exporting || loading} onClick={() => void exportCsv()}>{exporting ? 'Generando archivo...' : 'Exportar CSV'}</button></div>
    </form>
    {error ? <div className="alert alert-danger" role="alert">{error}</div> : null}
    {loading ? <ReportLoading /> : data ? <>
      <div className="report-kpis"><ReportKpi label="Total" value={data.indicadores.totalReservas} /><ReportKpi label="Confirmadas/reprogramadas" value={data.indicadores.confirmadasReprogramadas} /><ReportKpi label="Atendidas" value={data.indicadores.atendidas} /><ReportKpi label="Canceladas" value={data.indicadores.canceladas} /><ReportKpi label="No asistieron" value={data.indicadores.noAsistieron} /></div>
      {data.elementos.length === 0 ? <EmptyReport>No se encontraron reservas para los filtros seleccionados.</EmptyReport> : <div className="report-content-grid">
        <article className="report-card report-panel"><h2>Reservas por estado</h2><div className="report-status-list">{data.reservasPorEstado.map(item => <div className="report-status-row" key={item.estado}><ReservationBadge status={item.estado} /><div className="report-status-track"><div className="report-status-fill" style={{ width: `${item.cantidad / max * 100}%` }} /></div><strong>{item.cantidad}</strong></div>)}</div></article>
        <article className="report-card"><div className="report-table-wrap"><table className="table table-hover report-table"><thead><tr><th>Código</th><th>Fecha</th><th>Hora</th><th>Cliente</th><th>Servicio</th><th>Sede</th><th>Profesional</th><th>Estado</th><th>Participantes</th><th>Precio</th><th /></tr></thead><tbody>{data.elementos.map(item => <tr key={item.reservaId}><td>{item.codigo}</td><td>{shortDate.format(parseDate(item.fecha))}</td><td>{item.hora.slice(0, 5)}</td><td>{item.cliente}</td><td>{item.servicio}</td><td>{item.sede}</td><td>{item.profesional ?? '—'}</td><td><ReservationBadge status={item.estado} /></td><td>{item.cantidadParticipantes}</td><td>{money.format(item.precioTotal)}</td><td><Link className="btn btn-sm btn-outline-primary" to={`/organizaciones/${organizacionId}/reservas/${item.reservaId}`}>Ver</Link></td></tr>)}</tbody></table></div>
          <div className="report-mobile-list">{data.elementos.map(item => <article className="report-card report-mobile-card" key={item.reservaId}><header><strong>{item.codigo}</strong><ReservationBadge status={item.estado} /></header><p>{shortDate.format(parseDate(item.fecha))} · {item.hora.slice(0, 5)}</p><strong>{item.cliente}</strong><p>{item.servicio} · {item.sede}</p><footer><strong>{money.format(item.precioTotal)}</strong><Link className="btn btn-sm btn-outline-primary" to={`/organizaciones/${organizacionId}/reservas/${item.reservaId}`}>Ver</Link></footer></article>)}</div>
          <ReportPagination page={data.paginaActual} totalPages={data.totalPaginas} totalItems={data.totalElementos} pageSize={data.tamanoPagina} onPage={pagina => { setDraft(current => ({ ...current, pagina })); setApplied(current => ({ ...current, pagina })) }} onPageSize={tamanoPagina => { setDraft(current => ({ ...current, pagina: 1, tamanoPagina })); setApplied(current => ({ ...current, pagina: 1, tamanoPagina })) }} />
        </article>
      </div>}
    </> : null}
  </div>
}
