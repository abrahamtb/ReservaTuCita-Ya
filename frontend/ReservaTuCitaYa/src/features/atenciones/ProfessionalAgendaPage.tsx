import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import * as attentionApi from '../../api/atencionesApi'
import { ApiError } from '../../api/apiClient'
import { listarProfesionales } from '../../api/empleadosApi'
import { listOrganizations } from '../../api/organizacionesApi'
import { useAuth } from '../../auth/useAuth'
import { Empty, ErrorAlert, Loading, SuccessAlert } from '../../components/common/Feedback'
import type { AgendaProfesional, AgendaReserva, EmpleadoLista, Organization } from '../../types'
import {
  ConfirmationModal,
  ReservationStatus,
  displayDate,
  displayTime,
  minutesBetween,
  type AttentionAction,
} from './AttentionShared'
import './atenciones.css'

function todayInput() {
  const today = new Date()
  const offset = today.getTimezoneOffset()
  return new Date(today.getTime() - offset * 60_000).toISOString().slice(0, 10)
}

export function ProfessionalAgendaPage() {
  const { user } = useAuth()
  const [organizations, setOrganizations] = useState<Organization[]>([])
  const [organizationId, setOrganizationId] = useState(user?.organizacion?.id ?? '')
  const [professionals, setProfessionals] = useState<EmpleadoLista[]>([])
  const [professionalId, setProfessionalId] = useState(user?.empleadoId ?? '')
  const [date, setDate] = useState(todayInput)
  const [siteId, setSiteId] = useState('')
  const [agenda, setAgenda] = useState<AgendaProfesional>()
  const [loadingFilters, setLoadingFilters] = useState(true)
  const [loadingAgenda, setLoadingAgenda] = useState(false)
  const [error, setError] = useState<unknown>()
  const [success, setSuccess] = useState('')
  const [conflict, setConflict] = useState('')
  const [selected, setSelected] = useState<{ item: AgendaReserva; action: AttentionAction }>()
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    if (user?.organizacion) {
      setOrganizations([{
        id: user.organizacion.id,
        nombreComercial: user.organizacion.nombre,
        tipoOrganizacion: '',
        numeroDocumento: '',
        estaActivo: true,
      }])
      setOrganizationId(user.organizacion.id)
      setLoadingFilters(false)
      return
    }
    const controller = new AbortController()
    listOrganizations({ estado: 'Activos', pagina: 1, tamanoPagina: 100 }, controller.signal)
      .then(result => {
        setOrganizations(result.elementos)
        setOrganizationId(current => current || result.elementos[0]?.id || '')
      })
      .catch(caught => {
        if ((caught as Error).name !== 'AbortError') setError(caught)
      })
      .finally(() => setLoadingFilters(false))
    return () => controller.abort()
  }, [user?.organizacion])

  useEffect(() => {
    if (!organizationId) {
      setProfessionals([])
      setProfessionalId('')
      return
    }
    const controller = new AbortController()
    setAgenda(undefined)
    setSiteId('')
    listarProfesionales(organizationId, controller.signal)
      .then(result => {
        setProfessionals(result.elementos)
        setProfessionalId(current => {
          if (result.elementos.some(item => item.id === current)) return current
          if (user?.empleadoId && result.elementos.some(item => item.id === user.empleadoId)) return user.empleadoId
          return result.elementos[0]?.id ?? ''
        })
      })
      .catch(caught => {
        if ((caught as Error).name !== 'AbortError') setError(caught)
      })
    return () => controller.abort()
  }, [organizationId, user?.empleadoId])

  const loadAgenda = useCallback(async () => {
    if (!organizationId || !professionalId || !date) {
      setAgenda(undefined)
      return
    }
    setLoadingAgenda(true)
    setError(undefined)
    try {
      const result = await attentionApi.obtenerAgendaProfesional(organizationId, professionalId, date)
      setAgenda({
        ...result,
        reservas: [...result.reservas].sort((left, right) => left.horaInicio.localeCompare(right.horaInicio)),
      })
    } catch (caught) {
      setError(caught)
    } finally {
      setLoadingAgenda(false)
    }
  }, [date, organizationId, professionalId])

  useEffect(() => { void loadAgenda() }, [loadAgenda])

  const sites = useMemo(() => {
    const unique = new Map<string, string>()
    agenda?.reservas.forEach(item => unique.set(item.sedeId, item.sedeNombre))
    return [...unique].map(([id, name]) => ({ id, name })).sort((a, b) => a.name.localeCompare(b.name))
  }, [agenda])

  const reservations = useMemo(
    () => agenda?.reservas.filter(item => !siteId || item.sedeId === siteId) ?? [],
    [agenda, siteId],
  )

  const professional = professionals.find(item => item.id === professionalId)
  const scheduledMinutes = reservations.reduce(
    (total, item) => total + Math.max(0, minutesBetween(item.horaInicio, item.horaFin)),
    0,
  )
  const nextAttention = reservations.find(item => {
    if (['Atendida', 'Cancelada', 'NoAsistio'].includes(item.estado)) return false
    return date !== todayInput() || item.horaInicio.slice(0, 5) >= new Date().toTimeString().slice(0, 5)
  })

  async function confirmAction() {
    if (!selected) return
    setBusy(true)
    setError(undefined)
    setSuccess('')
    setConflict('')
    try {
      if (selected.action === 'present') {
        await attentionApi.marcarPresente(organizationId, selected.item.reservaId)
        setSuccess('Cliente marcado como presente.')
      } else if (selected.action === 'start') {
        await attentionApi.iniciarAtencion(organizationId, selected.item.reservaId)
        setSuccess('Atención iniciada.')
      } else {
        await attentionApi.marcarNoAsistio(organizationId, selected.item.reservaId)
        setSuccess('Reserva marcada como no asistida.')
      }
      setSelected(undefined)
      await loadAgenda()
    } catch (caught) {
      setSelected(undefined)
      if (caught instanceof ApiError && caught.status === 409) {
        setConflict('El estado de la reserva cambió. La información será actualizada.')
        await loadAgenda()
      } else {
        setError(caught)
      }
    } finally {
      setBusy(false)
    }
  }

  return <section className="attention-page">
    <div className="d-flex flex-wrap justify-content-between align-items-start gap-3 mb-4">
      <div>
        <p className="attention-eyebrow mb-1">Atenciones</p>
        <h1 className="mb-1">Agenda del profesional</h1>
        <p className="text-secondary mb-0">Consulta el día y ejecuta las acciones disponibles de cada reserva.</p>
      </div>
      <div className="attention-date-title">{displayDate(date)}</div>
    </div>

    <SuccessAlert message={success} />
    {conflict ? <div className="alert alert-warning" role="status">{conflict}</div> : null}
    {error ? <ErrorAlert error={error} /> : null}

    <div className="card border-0 shadow-sm mb-4">
      <div className="card-body">
        {loadingFilters ? <Loading /> : <div className="row g-3">
          <div className="col-lg-4">
            <label className="form-label" htmlFor="agenda-organization">Organización</label>
            <select
              id="agenda-organization"
              className="form-select"
              value={organizationId}
              disabled={Boolean(user?.organizacion)}
              onChange={event => setOrganizationId(event.target.value)}
            >
              <option value="">Selecciona una organización</option>
              {organizations.map(item => <option key={item.id} value={item.id}>{item.nombreComercial}</option>)}
            </select>
          </div>
          <div className="col-lg-4">
            <label className="form-label" htmlFor="agenda-professional">Profesional</label>
            <select id="agenda-professional" className="form-select" value={professionalId} onChange={event => setProfessionalId(event.target.value)}>
              <option value="">Selecciona un profesional</option>
              {professionals.map(item => <option key={item.id} value={item.id}>{item.nombreCompleto}</option>)}
            </select>
          </div>
          <div className="col-sm-6 col-lg-2">
            <label className="form-label" htmlFor="agenda-date">Fecha</label>
            <input id="agenda-date" className="form-control" type="date" value={date} onChange={event => setDate(event.target.value)} />
          </div>
          <div className="col-sm-6 col-lg-2">
            <label className="form-label" htmlFor="agenda-site">Sede</label>
            <select id="agenda-site" className="form-select" value={siteId} disabled={!sites.length} onChange={event => setSiteId(event.target.value)}>
              <option value="">Todas</option>
              {sites.map(item => <option key={item.id} value={item.id}>{item.name}</option>)}
            </select>
          </div>
        </div>}
      </div>
    </div>

    {professional ? <div className="attention-summary-grid mb-4">
      <article className="card border-0 shadow-sm"><div className="card-body">
        <span className="attention-summary-label">Profesional</span>
        <strong>{professional.nombreCompleto}</strong>
        <small>{professional.especialidad || professional.cargo}</small>
      </div></article>
      <article className="card border-0 shadow-sm"><div className="card-body">
        <span className="attention-summary-label">Reservas</span>
        <strong>{reservations.length}</strong>
        <small>para la fecha seleccionada</small>
      </div></article>
      <article className="card border-0 shadow-sm"><div className="card-body">
        <span className="attention-summary-label">Tiempo programado</span>
        <strong>{Math.floor(scheduledMinutes / 60)} h {scheduledMinutes % 60} min</strong>
        <small>según las reservas mostradas</small>
      </div></article>
      <article className="card border-0 shadow-sm"><div className="card-body">
        <span className="attention-summary-label">Próxima atención</span>
        <strong>{nextAttention ? displayTime(nextAttention.horaInicio) : '—'}</strong>
        <small>{nextAttention?.clienteNombre ?? 'Sin atención pendiente'}</small>
      </div></article>
    </div> : null}

    {!organizationId || !professionalId
      ? <Empty message="Selecciona una organización y un profesional para consultar la agenda." />
      : loadingAgenda
        ? <Loading />
        : reservations.length === 0
          ? <Empty message="No hay reservas programadas para esta fecha." />
          : <AgendaList organizationId={organizationId} reservations={reservations} onAction={(item, action) => setSelected({ item, action })} />}

    {selected ? <ConfirmationModal
      action={selected.action}
      client={selected.item.clienteNombre}
      service={selected.item.servicioNombre}
      scheduledTime={selected.item.horaInicio}
      arrivalTime={selected.item.fechaHoraPresencia}
      busy={busy}
      onCancel={() => setSelected(undefined)}
      onConfirm={() => void confirmAction()}
    /> : null}
  </section>
}

function AgendaList({
  organizationId,
  reservations,
  onAction,
}: {
  organizationId: string
  reservations: AgendaReserva[]
  onAction: (item: AgendaReserva, action: AttentionAction) => void
}) {
  return <div className="card border-0 shadow-sm overflow-hidden">
    <div className="table-responsive attention-agenda-table">
      <table className="table align-middle mb-0">
        <thead><tr><th>Hora</th><th>Cliente y servicio</th><th>Sede</th><th>Estado</th><th className="text-end">Acciones</th></tr></thead>
        <tbody>{reservations.map(item => <tr key={item.reservaId}>
          <td><strong>{displayTime(item.horaInicio)}</strong><small className="d-block text-secondary">hasta {displayTime(item.horaFin)}</small></td>
          <td><strong>{item.clienteNombre}</strong><small className="d-block text-secondary">{item.servicioNombre} · {item.codigoReserva}</small></td>
          <td>{item.sedeNombre}</td>
          <td><ReservationStatus status={item.estado} /></td>
          <td className="text-end"><AgendaActions organizationId={organizationId} item={item} onAction={onAction} /></td>
        </tr>)}</tbody>
      </table>
    </div>
    <div className="attention-agenda-cards">
      {reservations.map(item => <article className="attention-reservation-card" key={item.reservaId}>
        <div className="d-flex justify-content-between gap-3"><strong className="attention-card-time">{displayTime(item.horaInicio)}</strong><ReservationStatus status={item.estado} /></div>
        <h2 className="h5 mt-3 mb-1">{item.clienteNombre}</h2>
        <p className="mb-1">{item.servicioNombre}</p>
        <p className="small text-secondary">{item.sedeNombre} · {item.codigoReserva}</p>
        <AgendaActions organizationId={organizationId} item={item} onAction={onAction} />
      </article>)}
    </div>
  </div>
}

function AgendaActions({
  organizationId,
  item,
  onAction,
}: {
  organizationId: string
  item: AgendaReserva
  onAction: (item: AgendaReserva, action: AttentionAction) => void
}) {
  return <div className="d-inline-flex flex-wrap justify-content-end gap-2">
    {['Confirmada', 'Reprogramada'].includes(item.estado) ? <>
      <button className="btn btn-sm btn-primary" onClick={() => onAction(item, 'present')}>Marcar presente</button>
      <button className="btn btn-sm btn-outline-danger" onClick={() => onAction(item, 'no-show')}>No asistió</button>
    </> : null}
    {item.estado === 'Presente' ? <button className="btn btn-sm btn-primary" onClick={() => onAction(item, 'start')}>Iniciar atención</button> : null}
    <Link className="btn btn-sm btn-outline-secondary" to={`/organizaciones/${organizationId}/reservas/${item.reservaId}/atencion`}>
      {item.estado === 'EnAtencion' ? 'Finalizar atención' : item.estado === 'Atendida' ? 'Ver atención' : 'Ver detalle'}
    </Link>
  </div>
}
