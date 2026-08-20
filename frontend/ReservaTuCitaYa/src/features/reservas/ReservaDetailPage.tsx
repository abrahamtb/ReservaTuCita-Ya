import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import * as api from '../../api/reservasApi'
import { consultarDisponibilidad, type HorarioDisponible } from '../../api/disponibilidadApi'
import { useAuth } from '../../auth/useAuth'
import { ErrorAlert, Loading } from '../../components/common/Feedback'
import type { ReservaDetalle } from '../../types'

export function ReservaDetailPage() {
  const { user } = useAuth()
  const permissions = new Set(user?.permisos ?? [])
  const { id = '' } = useParams()
  const navigate = useNavigate()
  const [item, setItem] = useState<ReservaDetalle>()
  const [error, setError] = useState<unknown>()
  const [busy, setBusy] = useState(false)
  const [mode, setMode] = useState<'none' | 'reschedule' | 'cancel'>('none')
  const [fechaNueva, setFechaNueva] = useState('')
  const [horaNueva, setHoraNueva] = useState('')
  const [comentario, setComentario] = useState('')
  const [slots, setSlots] = useState<HorarioDisponible[]>([])
  const [loadingSlots, setLoadingSlots] = useState(false)

  const load = useCallback(() => api.obtenerReserva(id).then(value => { setItem(value); setFechaNueva(value.fecha); setHoraNueva(value.horaInicio.slice(0, 5)) }).catch(setError), [id])
  useEffect(() => { void load() }, [load])

  useEffect(() => {
    if (mode !== 'reschedule' || !item || !fechaNueva) return
    const controller = new AbortController()
    setLoadingSlots(true)
    consultarDisponibilidad({ sedeId: item.sede.id, servicioId: item.servicio.id, fechaDesde: fechaNueva, fechaHasta: fechaNueva, profesionalId: item.profesional?.id, recursoId: item.recurso?.id }, controller.signal)
      .then(result => {
        const available = result.dias[0]?.horarios ?? []
        setSlots(available)
        setHoraNueva(current => available.some(slot => slot.horaInicio === current || slot.horaInicio.slice(0, 5) === current) ? current : '')
      })
      .catch(caught => { if (!controller.signal.aborted && (caught as Error).name !== 'AbortError') setError(caught) })
      .finally(() => { if (!controller.signal.aborted) setLoadingSlots(false) })
    return () => controller.abort()
  }, [fechaNueva, item, mode])

  async function reschedule(event: FormEvent) {
    event.preventDefault(); if (!item || !horaNueva) return; setBusy(true); setError(undefined)
    try { await api.reprogramarReserva(item.organizacionId, id, { fechaNueva, horaInicioNueva: horaNueva, motivo: 'SolicitudCliente', observacion: comentario || undefined }); setMode('none'); await load() }
    catch (caught) { setError(caught) } finally { setBusy(false) }
  }
  async function cancel(event: FormEvent) {
    event.preventDefault(); if (!item) return; setBusy(true); setError(undefined)
    try { await api.cancelarReserva(item.organizacionId, id, { motivo: 'SolicitudCliente', comentario: comentario || undefined, confirmacion: true }); setMode('none'); await load() }
    catch (caught) { setError(caught) } finally { setBusy(false) }
  }

  if (error && !item) return <ErrorAlert error={error} />
  if (!item) return <Loading />
  const canChange = !['Cancelada', 'Atendida', 'NoAsistio'].includes(item.estado)
    && (permissions.has('reservas.reprogramar') || permissions.has('reservas.cancelar'))
  return <section>
    <div className="d-flex justify-content-between align-items-start"><div><h1>Reserva {item.codigo}</h1><span className="badge text-bg-primary">{item.estado}</span></div><button className="btn btn-outline-secondary" onClick={() => navigate('/reservas')}>Volver</button></div>
    {error ? <div className="mt-3"><ErrorAlert error={error} /></div> : null}
    <div className="card card-body my-3"><dl className="row mb-0"><dt className="col-sm-3">Cliente</dt><dd className="col-sm-9">{item.cliente.nombre}</dd><dt className="col-sm-3">Servicio</dt><dd className="col-sm-9">{item.servicio.nombre}</dd><dt className="col-sm-3">Sede</dt><dd className="col-sm-9">{item.sede.nombre}</dd><dt className="col-sm-3">Fecha y hora</dt><dd className="col-sm-9">{item.fecha} · {item.horaInicio.slice(0, 5)}–{item.horaFinServicio.slice(0, 5)}</dd><dt className="col-sm-3">Profesional</dt><dd className="col-sm-9">{item.profesional?.nombre ?? 'Sin asignar'}</dd><dt className="col-sm-3">Participantes</dt><dd className="col-sm-9">{item.cantidadParticipantes}</dd></dl></div>
    <div className="d-flex flex-wrap gap-2 mb-4">{permissions.has('pagos.ver') && <Link className="btn btn-outline-success" to={`/pagos/${id}`}>Pagos</Link>}{permissions.has('atenciones.ver') && <Link className="btn btn-outline-primary" to={`/organizaciones/${item.organizacionId}/reservas/${id}/atencion`}>Atención</Link>}{canChange ? <>{permissions.has('reservas.reprogramar') && <button className="btn btn-outline-warning" onClick={() => { setMode('reschedule'); setComentario('') }}>Reprogramar</button>}{permissions.has('reservas.cancelar') && <button className="btn btn-outline-danger" onClick={() => { setMode('cancel'); setComentario('') }}>Cancelar</button>}</> : null}</div>
    {mode === 'reschedule' ? <form className="card card-body" onSubmit={reschedule}><h2 className="h5">Reprogramar reserva</h2><p className="text-secondary">Antes: {item.fecha} · {item.horaInicio.slice(0, 5)}. Elige una nueva disponibilidad validada.</p><div className="row g-3"><div className="col-md-4"><label className="form-label">Nueva fecha</label><input required min={new Date().toISOString().slice(0, 10)} type="date" className="form-control" value={fechaNueva} onChange={e => setFechaNueva(e.target.value)} /></div><div className="col-md-8"><span className="form-label d-block">Horarios disponibles</span>{loadingSlots ? <span className="text-secondary">Consultando disponibilidad…</span> : <div className="d-flex flex-wrap gap-2">{slots.map((slot, index) => <button type="button" key={`${slot.horaInicio}-${index}`} className={`btn ${horaNueva === slot.horaInicio || horaNueva === slot.horaInicio.slice(0, 5) ? 'btn-primary' : 'btn-outline-primary'}`} onClick={() => setHoraNueva(slot.horaInicio)}>{slot.horaInicio.slice(0, 5)}</button>)}</div>}{!loadingSlots && !slots.length ? <small className="text-secondary d-block mt-2">No hay horarios disponibles para esta fecha.</small> : null}</div><div className="col-12"><label className="form-label">Observación</label><input className="form-control" value={comentario} onChange={e => setComentario(e.target.value)} /></div></div><div className="mt-3 d-flex gap-2"><button disabled={busy || !horaNueva} className="btn btn-warning">Confirmar reprogramación</button><button type="button" className="btn btn-outline-secondary" onClick={() => setMode('none')}>Cerrar</button></div></form> : null}
    {mode === 'cancel' ? <form className="card card-body border-danger" onSubmit={cancel}><h2 className="h5 text-danger">Cancelar reserva</h2><p>La operación quedará registrada en el historial.</p><label className="form-label">Comentario</label><textarea className="form-control" value={comentario} onChange={e => setComentario(e.target.value)} /><div className="mt-3 d-flex gap-2"><button disabled={busy} className="btn btn-danger">Confirmar cancelación</button><button type="button" className="btn btn-outline-secondary" onClick={() => setMode('none')}>Cerrar</button></div></form> : null}
  </section>
}
