import type { ReactNode } from 'react'
import type { EstadoReserva } from '../../types'

export interface SelectOption { id: string; nombre: string }

export function DateRangeFilter({ desde, hasta, onDesde, onHasta }: {
  desde: string; hasta: string; onDesde: (value: string) => void; onHasta: (value: string) => void
}) {
  return <>
    <label>Desde<input className="form-control" type="date" value={desde} max={hasta} onChange={event => onDesde(event.target.value)} required /></label>
    <label>Hasta<input className="form-control" type="date" value={hasta} min={desde} onChange={event => onHasta(event.target.value)} required /></label>
  </>
}

export function SelectFilter({ label, value, options, emptyLabel = 'Todos', onChange, disabled = false }: {
  label: string; value?: string; options: SelectOption[]; emptyLabel?: string
  onChange: (value: string) => void; disabled?: boolean
}) {
  return <label>{label}<select className="form-select" value={value ?? ''} disabled={disabled} onChange={event => onChange(event.target.value)}>
    <option value="">{emptyLabel}</option>
    {options.map(option => <option key={option.id} value={option.id}>{option.nombre}</option>)}
  </select></label>
}

export function ReportKpi({ label, value, help }: { label: string; value: ReactNode; help?: string }) {
  return <article className="report-card report-kpi"><span>{label}</span><strong>{value}</strong>{help ? <small>{help}</small> : null}</article>
}

export function ReportLoading() {
  return <div aria-busy="true" aria-label="Cargando reporte"><div className="report-kpis">{Array.from({ length: 5 }, (_, index) => <div className="report-skeleton report-skeleton--kpi" key={index} />)}</div><div className="report-skeleton report-skeleton--table" /></div>
}

export function ReportPagination({ page, totalPages, totalItems, pageSize, onPage, onPageSize }: {
  page: number; totalPages: number; totalItems: number; pageSize: number
  onPage: (page: number) => void; onPageSize: (size: number) => void
}) {
  return <div className="report-pagination">
    <span>{totalItems.toLocaleString('es-PE')} registros · Página {page} de {Math.max(totalPages, 1)}</span>
    <label>Por página<select className="form-select form-select-sm" value={pageSize} onChange={event => onPageSize(Number(event.target.value))}>{[10, 25, 50, 100].map(size => <option key={size}>{size}</option>)}</select></label>
    <div className="btn-group"><button type="button" className="btn btn-sm btn-outline-primary" disabled={page <= 1} onClick={() => onPage(page - 1)}>Anterior</button><button type="button" className="btn btn-sm btn-outline-primary" disabled={page >= totalPages} onClick={() => onPage(page + 1)}>Siguiente</button></div>
  </div>
}

const statusClasses: Record<EstadoReserva, string> = {
  NoDefinido: 'text-bg-secondary', Pendiente: 'text-bg-warning', Confirmada: 'text-bg-primary',
  Presente: 'text-bg-info', EnAtencion: 'text-bg-info', Atendida: 'text-bg-success',
  Reprogramada: 'text-bg-warning', Cancelada: 'text-bg-danger', NoAsistio: 'text-bg-dark',
}
const statusLabels: Record<EstadoReserva, string> = {
  NoDefinido: 'No definido', Pendiente: 'Pendiente', Confirmada: 'Confirmada', Presente: 'Presente',
  EnAtencion: 'En atención', Atendida: 'Atendida', Reprogramada: 'Reprogramada', Cancelada: 'Cancelada', NoAsistio: 'No asistió',
}

export function ReservationBadge({ status }: { status: EstadoReserva }) {
  return <span className={`badge ${statusClasses[status]}`}>{statusLabels[status]}</span>
}

export function validateRange(desde: string, hasta: string) {
  if (!desde || !hasta) return 'Desde y hasta son obligatorios.'
  if (hasta < desde) return 'La fecha final debe ser igual o posterior a la fecha inicial.'
  return ''
}

export const money = new Intl.NumberFormat('es-PE', { style: 'currency', currency: 'PEN' })
export const shortDate = new Intl.DateTimeFormat('es-PE', { day: '2-digit', month: 'short', year: 'numeric' })
export const dateTime = new Intl.DateTimeFormat('es-PE', { dateStyle: 'short', timeStyle: 'short' })
export const parseDate = (value: string) => new Date(`${value.slice(0, 10)}T12:00:00`)

export function EmptyReport({ children }: { children: ReactNode }) {
  return <div className="report-empty">{children}</div>
}
