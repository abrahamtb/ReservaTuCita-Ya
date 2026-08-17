import type { EstadoReserva } from '../../types'

export type AttentionAction = 'present' | 'start' | 'no-show'

const statusLabels: Record<EstadoReserva, string> = {
  NoDefinido: 'No definido',
  Pendiente: 'Pendiente',
  Confirmada: 'Confirmada',
  Presente: 'Presente',
  EnAtencion: 'En atención',
  Atendida: 'Atendida',
  Reprogramada: 'Reprogramada',
  Cancelada: 'Cancelada',
  NoAsistio: 'No asistió',
}

const statusClasses: Record<EstadoReserva, string> = {
  NoDefinido: 'text-bg-secondary',
  Pendiente: 'text-bg-warning',
  Confirmada: 'text-bg-primary',
  Presente: 'text-bg-info',
  EnAtencion: 'text-bg-warning',
  Atendida: 'text-bg-success',
  Reprogramada: 'text-bg-primary',
  Cancelada: 'text-bg-secondary',
  NoAsistio: 'text-bg-dark',
}

export function ReservationStatus({ status }: { status: EstadoReserva }) {
  return <span className={`badge ${statusClasses[status]}`}>{statusLabels[status]}</span>
}

export function statusLabel(status: EstadoReserva) {
  return statusLabels[status]
}

export function displayTime(value?: string | null) {
  return value ? value.slice(0, 5) : '—'
}

export function displayDate(value?: string | null) {
  if (!value) return '—'
  return new Intl.DateTimeFormat('es-PE', {
    day: '2-digit',
    month: 'long',
    year: 'numeric',
  }).format(new Date(`${value}T00:00:00`))
}

export function displayDateTime(value?: string | null) {
  if (!value) return '—'
  return new Intl.DateTimeFormat('es-PE', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(new Date(value))
}

export function minutesBetween(start: string, end: string) {
  const [startHour, startMinute] = start.split(':').map(Number)
  const [endHour, endMinute] = end.split(':').map(Number)
  return (endHour * 60 + endMinute) - (startHour * 60 + startMinute)
}

const actionContent: Record<AttentionAction, {
  title: string
  message: string
  confirm: string
  busy: string
}> = {
  present: {
    title: 'Marcar presente',
    message: 'Confirma que el cliente llegó a su atención.',
    confirm: 'Marcar presente',
    busy: 'Marcando presente…',
  },
  start: {
    title: 'Iniciar atención',
    message: 'La hora de inicio será registrada automáticamente.',
    confirm: 'Iniciar atención',
    busy: 'Iniciando atención…',
  },
  'no-show': {
    title: 'Marcar no asistencia',
    message: 'Esta reserva será marcada como no asistida.',
    confirm: 'Marcar no asistencia',
    busy: 'Marcando no asistencia…',
  },
}

export function ConfirmationModal({
  action,
  client,
  service,
  scheduledTime,
  arrivalTime,
  busy,
  onCancel,
  onConfirm,
}: {
  action: AttentionAction
  client: string
  service: string
  scheduledTime: string
  arrivalTime?: string | null
  busy: boolean
  onCancel: () => void
  onConfirm: () => void
}) {
  const content = actionContent[action]
  return <div className="attention-modal-backdrop" role="presentation">
    <section className="attention-modal card shadow" role="dialog" aria-modal="true" aria-labelledby="attention-modal-title">
      <div className="card-body p-4">
        <h2 className="h4" id="attention-modal-title">{content.title}</h2>
        <p className="text-secondary">{content.message}</p>
        <dl className="row small mb-4">
          <dt className="col-4">Cliente</dt><dd className="col-8">{client}</dd>
          <dt className="col-4">Servicio</dt><dd className="col-8">{service}</dd>
          <dt className="col-4">Hora</dt><dd className="col-8">{displayTime(scheduledTime)}</dd>
          {arrivalTime ? <><dt className="col-4">Llegada</dt><dd className="col-8">{displayDateTime(arrivalTime)}</dd></> : null}
        </dl>
        <div className="d-flex flex-column-reverse flex-sm-row justify-content-end gap-2">
          <button type="button" className="btn btn-outline-secondary" disabled={busy} onClick={onCancel}>Cancelar</button>
          <button type="button" className={`btn ${action === 'no-show' ? 'btn-danger' : 'btn-primary'}`} disabled={busy} onClick={onConfirm}>
            {busy ? content.busy : content.confirm}
          </button>
        </div>
      </div>
    </section>
  </div>
}
