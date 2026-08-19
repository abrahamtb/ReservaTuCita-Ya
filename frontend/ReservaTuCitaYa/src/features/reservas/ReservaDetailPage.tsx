import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import * as api from '../../api/reservasApi'
import { ErrorAlert, Loading } from '../../components/common/Feedback'
import type { ReservaDetalle } from '../../types'

export function ReservaDetailPage() {
  const { id = '' } = useParams()
  const navigate = useNavigate()
  const [item, setItem] = useState<ReservaDetalle>()
  const [error, setError] = useState<unknown>()
  const [busy, setBusy] = useState(false)
  const [mode, setMode] = useState<'none' | 'reschedule' | 'cancel'>('none')
  const [fechaNueva, setFechaNueva] = useState('')
  const [horaNueva, setHoraNueva] = useState('')
  const [comentario, setComentario] = useState('')

  const load = useCallback(() => api.obtenerReserva(id).then(value => { setItem(value); setFechaNueva(value.fecha); setHoraNueva(value.horaInicio.slice(0, 5)) }).catch(setError), [id])
  useEffect(() => { void load() }, [load])

  async function reschedule(event: FormEvent) {
    event.preventDefault(); if (!item) return; setBusy(true); setError(undefined)
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
  return <section>
    <div className="d-flex justify-content-between align-items-start"><div><h1>Reserva {item.codigo}</h1><span className="badge text-bg-primary">{item.estado}</span></div><button className="btn btn-outline-secondary" onClick={() => navigate('/reservas')}>Volver</button></div>
    {error ? <div className="mt-3"><ErrorAlert error={error} /></div> : null}
    <div className="card card-body my-3"><dl className="row mb-0"><dt className="col-sm-3">Cliente</dt><dd className="col-sm-9">{item.cliente.nombre}</dd><dt className="col-sm-3">Servicio</dt><dd className="col-sm-9">{item.servicio.nombre}</dd><dt className="col-sm-3">Sede</dt><dd className="col-sm-9">{item.sede.nombre}</dd><dt className="col-sm-3">Fecha y hora</dt><dd className="col-sm-9">{item.fecha} · {item.horaInicio.slice(0, 5)}–{item.horaFinServicio.slice(0, 5)}</dd><dt className="col-sm-3">Profesional</dt><dd className="col-sm-9">{item.profesional?.nombre ?? 'Sin asignar'}</dd><dt className="col-sm-3">Participantes</dt><dd className="col-sm-9">{item.cantidadParticipantes}</dd></dl></div>
    <div className="d-flex flex-wrap gap-2 mb-4"><Link className="btn btn-outline-success" to={`/pagos/${id}`}>Pagos</Link><Link className="btn btn-outline-primary" to={`/organizaciones/${item.organizacionId}/reservas/${id}/atencion`}>Atención</Link>{canChange ? <><button className="btn btn-outline-warning" onClick={() => { setMode('reschedule'); setComentario('') }}>Reprogramar</button><button className="btn btn-outline-danger" onClick={() => { setMode('cancel'); setComentario('') }}>Cancelar</button></> : null}</div>
    {mode === 'reschedule' ? <form className="card card-body" onSubmit={reschedule}><h2 className="h5">Reprogramar reserva</h2><div className="row g-3"><div className="col-md-4"><label className="form-label">Nueva fecha</label><input required type="date" className="form-control" value={fechaNueva} onChange={e => setFechaNueva(e.target.value)} /></div><div className="col-md-4"><label className="form-label">Nueva hora</label><input required type="time" className="form-control" value={horaNueva} onChange={e => setHoraNueva(e.target.value)} /></div><div className="col-md-4"><label className="form-label">Observación</label><input className="form-control" value={comentario} onChange={e => setComentario(e.target.value)} /></div></div><div className="mt-3 d-flex gap-2"><button disabled={busy} className="btn btn-warning">Confirmar reprogramación</button><button type="button" className="btn btn-outline-secondary" onClick={() => setMode('none')}>Cerrar</button></div></form> : null}
    {mode === 'cancel' ? <form className="card card-body border-danger" onSubmit={cancel}><h2 className="h5 text-danger">Cancelar reserva</h2><p>La operación quedará registrada en el historial.</p><label className="form-label">Comentario</label><textarea className="form-control" value={comentario} onChange={e => setComentario(e.target.value)} /><div className="mt-3 d-flex gap-2"><button disabled={busy} className="btn btn-danger">Confirmar cancelación</button><button type="button" className="btn btn-outline-secondary" onClick={() => setMode('none')}>Cerrar</button></div></form> : null}
  </section>
}
