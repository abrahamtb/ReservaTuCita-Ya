import { useCallback, useEffect, useMemo, useState, type FormEvent } from 'react'
import { Link, useParams } from 'react-router-dom'
import * as attentionApi from '../../api/atencionesApi'
import { ApiError } from '../../api/apiClient'
import { obtenerReserva } from '../../api/reservasApi'
import { listServices } from '../../api/serviciosApi'
import { useAuth } from '../../auth/useAuth'
import { ErrorAlert, Loading, SuccessAlert } from '../../components/common/Feedback'
import type {
  AtencionDetalle,
  FinalizarAtencionRequest,
  ReservaDetalle,
  ResultadoAtencion,
  Servicio,
} from '../../types'
import {
  ConfirmationModal,
  ReservationStatus,
  displayDate,
  displayDateTime,
  displayTime,
  statusLabel,
  type AttentionAction,
} from './AttentionShared'
import './atenciones.css'

const results: ResultadoAtencion[] = ['Completada', 'Parcial', 'Interrumpida']

const historyLabels: Record<string, string> = {
  Creada: 'Reserva creada',
  Confirmada: 'Reserva confirmada',
  Reprogramada: 'Reserva reprogramada',
  Cancelada: 'Reserva cancelada',
  MarcadaPresente: 'Cliente presente',
  AtencionIniciada: 'Atención iniciada',
  AtencionFinalizada: 'Atención finalizada',
  NoAsistio: 'Cliente no asistió',
}

export function AttentionDetailPage() {
  const { organizationId = '', reservationId = '' } = useParams()
  const { user } = useAuth()
  const permissions = new Set(user?.permisos ?? [])
  const canFinish = permissions.has('atenciones.finalizar')
  const [reservation, setReservation] = useState<ReservaDetalle>()
  const [attention, setAttention] = useState<AtencionDetalle>()
  const [services, setServices] = useState<Servicio[]>([])
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<unknown>()
  const [success, setSuccess] = useState('')
  const [conflict, setConflict] = useState('')
  const [selectedAction, setSelectedAction] = useState<AttentionAction>()
  const [result, setResult] = useState<ResultadoAtencion | ''>('')
  const [observations, setObservations] = useState('')
  const [recommendations, setRecommendations] = useState('')
  const [nextServiceId, setNextServiceId] = useState('')
  const [nextDate, setNextDate] = useState('')
  const [validation, setValidation] = useState('')

  const load = useCallback(async () => {
    if (!organizationId || !reservationId) return
    setLoading(true)
    setError(undefined)
    try {
      const currentReservation = await obtenerReserva(reservationId)
      setReservation(currentReservation)
      if (['Presente', 'EnAtencion', 'Atendida'].includes(currentReservation.estado)) {
        try {
          setAttention(await attentionApi.obtenerAtencionReserva(organizationId, reservationId))
        } catch (caught) {
          if (!(caught instanceof ApiError) || caught.status !== 404) throw caught
          setAttention(undefined)
        }
      } else {
        setAttention(undefined)
      }
    } catch (caught) {
      setError(caught)
    } finally {
      setLoading(false)
    }
  }, [organizationId, reservationId])

  useEffect(() => { void load() }, [load])

  useEffect(() => {
    if (reservation?.estado !== 'EnAtencion' || !canFinish) return
    const controller = new AbortController()
    listServices(
      organizationId,
      { estado: 'Activos', pagina: 1, tamanoPagina: 100 },
      controller.signal,
    ).then(response => setServices(response.elementos)).catch(caught => {
      if (!controller.signal.aborted && (caught as Error).name !== 'AbortError') setError(caught)
    })
    return () => controller.abort()
  }, [canFinish, organizationId, reservation?.estado])

  useEffect(() => {
    if (!attention || attention.estadoReserva !== 'Atendida') return
    setResult(attention.resultado ?? '')
    setObservations(attention.observaciones ?? '')
    setRecommendations(attention.recomendaciones ?? '')
    setNextServiceId(attention.proximoServicio?.id ?? '')
    setNextDate(attention.proximaFechaSugerida ?? '')
  }, [attention])

  const delayMinutes = useMemo(() => {
    if (!attention?.fechaHoraPresencia) return null
    const arrival = new Date(attention.fechaHoraPresencia)
    const scheduled = new Date(`${attention.fecha}T${attention.horaInicioProgramada}`)
    return Math.max(0, Math.round((arrival.getTime() - scheduled.getTime()) / 60_000))
  }, [attention])

  async function confirmAction() {
    if (!reservation || !selectedAction) return
    setBusy(true)
    setError(undefined)
    setSuccess('')
    setConflict('')
    try {
      if (selectedAction === 'present') {
        await attentionApi.marcarPresente(organizationId, reservation.id)
        setSuccess('Cliente marcado como presente.')
      } else if (selectedAction === 'start') {
        await attentionApi.iniciarAtencion(organizationId, reservation.id)
        setSuccess('Atención iniciada.')
      } else {
        await attentionApi.marcarNoAsistio(organizationId, reservation.id)
        setSuccess('Reserva marcada como no asistida.')
      }
      setSelectedAction(undefined)
      await load()
    } catch (caught) {
      setSelectedAction(undefined)
      if (caught instanceof ApiError && caught.status === 409) {
        setConflict('El estado de la reserva cambió. La información será actualizada.')
        await load()
      } else {
        setError(caught)
      }
    } finally {
      setBusy(false)
    }
  }

  async function finish(event: FormEvent) {
    event.preventDefault()
    setValidation('')
    setError(undefined)
    setSuccess('')
    setConflict('')
    if (!result) {
      setValidation('Selecciona el resultado de la atención.')
      return
    }
    if (observations.length > 1000 || recommendations.length > 1000) {
      setValidation('Las observaciones y recomendaciones no pueden superar 1000 caracteres.')
      return
    }
    const request: FinalizarAtencionRequest = {
      resultado: result,
      observaciones: observations.trim() || null,
      recomendaciones: recommendations.trim() || null,
      proximoServicioId: nextServiceId || null,
      proximaFechaSugerida: nextDate || null,
    }
    setBusy(true)
    try {
      await attentionApi.finalizarAtencion(organizationId, reservationId, request)
      setSuccess('Atención finalizada correctamente.')
      await load()
    } catch (caught) {
      if (caught instanceof ApiError && caught.status === 409) {
        setConflict('El estado de la reserva cambió. La información será actualizada.')
        await load()
      } else {
        setError(caught)
      }
    } finally {
      setBusy(false)
    }
  }

  if (loading) return <Loading />
  if (error && !reservation) return <ErrorAlert error={error} />
  if (!reservation) return <ErrorAlert error={new Error('No se pudo obtener la reserva.')} />

  return <section className="attention-page">
    <div className="d-flex flex-wrap justify-content-between align-items-start gap-3 mb-4">
      <div>
        <p className="attention-eyebrow mb-1">Reserva {reservation.codigo}</p>
        <h1 className="mb-2">{reservation.estado === 'Atendida' ? 'Atención finalizada' : 'Detalle de atención'}</h1>
        <ReservationStatus status={reservation.estado} />
      </div>
      <Link className="btn btn-outline-secondary" to="/atenciones/agenda">Volver a agenda</Link>
    </div>

    <SuccessAlert message={success} />
    {conflict ? <div className="alert alert-warning" role="status">{conflict}</div> : null}
    {error ? <ErrorAlert error={error} /> : null}

    <div className="row g-4">
      <div className={reservation.estado === 'EnAtencion' ? 'col-xl-5' : 'col-12'}>
        <article className="card border-0 shadow-sm h-100" id="datos-reserva">
          <div className="card-body p-4">
            <h2 className="h4 mb-4">Datos de la reserva</h2>
            <div className="attention-detail-grid">
              <Detail label="Cliente" value={reservation.cliente.nombre} />
              <Detail label="Servicio" value={reservation.servicio.nombre} />
              <Detail label="Profesional" value={reservation.profesional?.nombre ?? 'Sin asignar'} />
              <Detail label="Sede" value={reservation.sede.nombre} />
              <Detail label="Fecha" value={displayDate(reservation.fecha)} />
              <Detail label="Hora programada" value={`${displayTime(reservation.horaInicio)} – ${displayTime(reservation.horaFinServicio)}`} />
              <Detail label="Duración programada" value={`${reservation.duracionMinutos} minutos`} />
              <Detail label="Participantes" value={String(reservation.cantidadParticipantes)} />
              {attention ? <>
                <Detail label="Hora de llegada" value={displayDateTime(attention.fechaHoraPresencia)} />
                <Detail label="Inicio real" value={displayDateTime(attention.fechaHoraInicioReal)} />
                <Detail label="Fin real" value={displayDateTime(attention.fechaHoraFinReal)} />
                <Detail label="Duración real" value={attention.duracionRealMinutos == null ? '—' : `${attention.duracionRealMinutos} minutos`} />
                {delayMinutes && delayMinutes > 0 ? <Detail label="Retraso" value={`${delayMinutes} minutos`} /> : null}
              </> : null}
            </div>

            <div className="d-flex flex-wrap gap-2 mt-4">
              {['Confirmada', 'Reprogramada'].includes(reservation.estado) ? <>
                {permissions.has('atenciones.marcarPresente') && <button className="btn btn-primary" onClick={() => setSelectedAction('present')}>Marcar presente</button>}
                {canFinish && <button className="btn btn-outline-danger" onClick={() => setSelectedAction('no-show')}>Marcar no asistencia</button>}
              </> : null}
              {reservation.estado === 'Presente' && permissions.has('atenciones.iniciar') ? <button className="btn btn-primary" onClick={() => setSelectedAction('start')}>Iniciar atención</button> : null}
            </div>
          </div>
        </article>
      </div>

      {reservation.estado === 'EnAtencion' && canFinish ? <div className="col-xl-7">
        <FinishForm
          result={result}
          observations={observations}
          recommendations={recommendations}
          nextServiceId={nextServiceId}
          nextDate={nextDate}
          services={services}
          validation={validation}
          busy={busy}
          onResult={setResult}
          onObservations={setObservations}
          onRecommendations={setRecommendations}
          onNextService={setNextServiceId}
          onNextDate={setNextDate}
          onSubmit={finish}
        />
      </div> : reservation.estado === 'EnAtencion' ? <div className="col-xl-7"><article className="card border-0 shadow-sm h-100"><div className="card-body p-4"><h2 className="h4">Atención en curso</h2><p className="text-secondary mb-0">Esta atención está siendo realizada por el profesional asignado.</p></div></article></div> : null}
    </div>

    {reservation.estado === 'Atendida' && attention ? <CompletedAttention attention={attention} /> : null}

    {reservation.historial.length ? <article className="card border-0 shadow-sm mt-4">
      <div className="card-body p-4">
        <h2 className="h4 mb-4">Historial de la reserva</h2>
        <ol className="attention-history">
          {[...reservation.historial].sort((a, b) => a.fechaAccion.localeCompare(b.fechaAccion)).map(item => <li key={item.id}>
            <span className="attention-history-dot" />
            <div>
              <strong>{historyLabels[item.tipoAccion] ?? item.tipoAccion}</strong>
              <div className="small text-secondary">{displayDateTime(item.fechaAccion)} · Estado: {statusLabel(item.estadoNuevo)}</div>
              {item.motivo || item.observacion ? <p className="mb-0 mt-1 small">{item.motivo || item.observacion}</p> : null}
            </div>
          </li>)}
        </ol>
      </div>
    </article> : null}

    {selectedAction ? <ConfirmationModal
      action={selectedAction}
      client={reservation.cliente.nombre}
      service={reservation.servicio.nombre}
      scheduledTime={reservation.horaInicio}
      arrivalTime={attention?.fechaHoraPresencia}
      busy={busy}
      onCancel={() => setSelectedAction(undefined)}
      onConfirm={() => void confirmAction()}
    /> : null}
  </section>
}

function Detail({ label, value }: { label: string; value: string }) {
  return <div><span>{label}</span><strong>{value}</strong></div>
}

function FinishForm({
  result,
  observations,
  recommendations,
  nextServiceId,
  nextDate,
  services,
  validation,
  busy,
  onResult,
  onObservations,
  onRecommendations,
  onNextService,
  onNextDate,
  onSubmit,
}: {
  result: ResultadoAtencion | ''
  observations: string
  recommendations: string
  nextServiceId: string
  nextDate: string
  services: Servicio[]
  validation: string
  busy: boolean
  onResult: (value: ResultadoAtencion | '') => void
  onObservations: (value: string) => void
  onRecommendations: (value: string) => void
  onNextService: (value: string) => void
  onNextDate: (value: string) => void
  onSubmit: (event: FormEvent) => void
}) {
  return <form className="card border-0 shadow-sm attention-finish-form" onSubmit={onSubmit}>
    <div className="card-body p-4">
      <h2 className="h4 mb-1">Finalizar atención</h2>
      <p className="text-secondary mb-4">Registra el resultado y las indicaciones entregadas al cliente.</p>
      {validation ? <div className="alert alert-danger" role="alert">{validation}</div> : null}
      <div className="mb-3">
        <label className="form-label" htmlFor="attention-result">Resultado *</label>
        <select id="attention-result" className="form-select" required value={result} onChange={event => onResult(event.target.value as ResultadoAtencion | '')}>
          <option value="">Selecciona un resultado</option>
          {results.map(item => <option key={item} value={item}>{item}</option>)}
        </select>
      </div>
      <div className="mb-3">
        <label className="form-label" htmlFor="attention-observations">Observaciones</label>
        <textarea id="attention-observations" className="form-control" rows={4} maxLength={1000} value={observations} onChange={event => onObservations(event.target.value)} />
        <small className="text-secondary">{observations.length}/1000</small>
      </div>
      <div className="mb-3">
        <label className="form-label" htmlFor="attention-recommendations">Recomendaciones</label>
        <textarea id="attention-recommendations" className="form-control" rows={4} maxLength={1000} value={recommendations} onChange={event => onRecommendations(event.target.value)} />
        <small className="text-secondary">{recommendations.length}/1000</small>
      </div>
      <div className="row g-3">
        <div className="col-md-7">
          <label className="form-label" htmlFor="attention-next-service">Próximo servicio</label>
          <select id="attention-next-service" className="form-select" value={nextServiceId} onChange={event => onNextService(event.target.value)}>
            <option value="">Sin recomendación</option>
            {services.map(item => <option key={item.id} value={item.id}>{item.nombre}</option>)}
          </select>
        </div>
        <div className="col-md-5">
          <label className="form-label" htmlFor="attention-next-date">Próxima fecha sugerida</label>
          <input id="attention-next-date" className="form-control" type="date" value={nextDate} onChange={event => onNextDate(event.target.value)} />
        </div>
      </div>
    </div>
    <div className="card-footer bg-white border-0 p-4 pt-0">
      <button className="btn btn-primary btn-lg w-100" disabled={busy}>{busy ? 'Finalizando atención…' : 'Finalizar atención'}</button>
    </div>
  </form>
}

function CompletedAttention({ attention }: { attention: AtencionDetalle }) {
  return <article className="card border-0 shadow-sm mt-4">
    <div className="card-body p-4">
      <div className="d-flex flex-wrap justify-content-between gap-2 mb-4">
        <div><p className="attention-eyebrow mb-1">Resultado</p><h2 className="h4 mb-0">{attention.resultado ?? '—'}</h2></div>
        <a className="btn btn-outline-secondary align-self-start" href="#datos-reserva">Ver reserva</a>
      </div>
      <div className="attention-completed-grid">
        <section><h3 className="h6">Observaciones</h3><p>{attention.observaciones || 'Sin observaciones.'}</p></section>
        <section><h3 className="h6">Recomendaciones</h3><p>{attention.recomendaciones || 'Sin recomendaciones.'}</p></section>
        <section><h3 className="h6">Próximo servicio</h3><p>{attention.proximoServicio?.nombre || 'No indicado.'}</p></section>
        <section><h3 className="h6">Próxima fecha sugerida</h3><p>{displayDate(attention.proximaFechaSugerida)}</p></section>
      </div>
    </div>
  </article>
}
