import { useEffect, useMemo, useState } from 'react'
import { ApiError } from '../../api/apiClient'
import {
  crearCalificacion,
  obtenerCalificacionReserva,
  obtenerResumenProfesional,
  type CalificacionDto,
  type ResumenProfesionalDto,
} from '../../api/calificacionesApi'
import { listarReservas, obtenerReserva, type ReservaLista } from '../../api/reservasApi'
import { useAuth } from '../../auth/useAuth'
import type { ReservaDetalle } from '../../types'

function Stars({ value, onChange, disabled = false }: { value: number; onChange?: (value: number) => void; disabled?: boolean }) {
  return <div className="d-flex gap-1" aria-label={`${value} de 5 estrellas`}>
    {[1, 2, 3, 4, 5].map(star => <button
      key={star}
      type="button"
      className={`btn btn-sm ${star <= value ? 'btn-warning' : 'btn-outline-secondary'}`}
      disabled={disabled}
      aria-label={`${star} estrellas`}
      onClick={() => onChange?.(star)}
    >★</button>)}
  </div>
}

export function CalificacionesPage() {
  const { user } = useAuth()
  const organizacionId = user?.organizacion?.id ?? ''
  const clienteId = user?.clienteId ?? ''
  const [reservas, setReservas] = useState<ReservaLista[]>([])
  const [reservaId, setReservaId] = useState('')
  const [detalle, setDetalle] = useState<ReservaDetalle | null>(null)
  const [calificacion, setCalificacion] = useState<CalificacionDto | null>(null)
  const [resumen, setResumen] = useState<ResumenProfesionalDto | null>(null)
  const [puntuacion, setPuntuacion] = useState(0)
  const [comentario, setComentario] = useState('')
  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  useEffect(() => {
    if (!organizacionId || !clienteId) return
    const controller = new AbortController()
    setLoading(true)
    listarReservas(organizacionId, { clienteId, estado: 'Atendida', pagina: 1, tamanoPagina: 100 }, controller.signal)
      .then(result => {
        setReservas(result.elementos)
        setReservaId(current => current || result.elementos[0]?.id || '')
      })
      .catch(caught => {
        if (!controller.signal.aborted) setError(caught instanceof Error ? caught.message : 'No se pudieron cargar tus atenciones.')
      })
      .finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [clienteId, organizacionId])

  useEffect(() => {
    if (!reservaId) { setDetalle(null); setCalificacion(null); setResumen(null); return }
    const controller = new AbortController()
    setError(''); setSuccess(''); setPuntuacion(0); setComentario('')
    obtenerReserva(reservaId, controller.signal).then(async item => {
      setDetalle(item)
      if (item.profesional?.id) {
        obtenerResumenProfesional(item.profesional.id, controller.signal).then(setResumen).catch(() => setResumen(null))
      } else setResumen(null)
      try {
        const actual = await obtenerCalificacionReserva(reservaId, controller.signal)
        setCalificacion(actual)
        setPuntuacion(actual.puntuacion)
        setComentario(actual.comentario ?? '')
      } catch (caught) {
        if (caught instanceof ApiError && caught.status === 404) setCalificacion(null)
        else if (!controller.signal.aborted) throw caught
      }
    }).catch(caught => {
      if (!controller.signal.aborted) setError(caught instanceof Error ? caught.message : 'No se pudo cargar la atención.')
    })
    return () => controller.abort()
  }, [reservaId])

  const distribucion = useMemo(() => {
    const map = new Map((resumen?.distribucion ?? []).map(item => [item.estrellas, item.cantidad]))
    return [5, 4, 3, 2, 1].map(estrellas => ({ estrellas, cantidad: map.get(estrellas) ?? 0 }))
  }, [resumen])

  async function guardar() {
    if (!detalle || puntuacion < 1 || puntuacion > 5 || calificacion) return
    setSaving(true); setError(''); setSuccess('')
    try {
      const created = await crearCalificacion(detalle.id, puntuacion, comentario.trim() || undefined)
      setCalificacion(created)
      setSuccess('Calificación enviada correctamente.')
      if (detalle.profesional?.id) setResumen(await obtenerResumenProfesional(detalle.profesional.id))
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : 'No se pudo guardar la calificación.')
    } finally { setSaving(false) }
  }

  if (!clienteId) return <div className="alert alert-info">Tu cuenta todavía no está vinculada a un cliente.</div>

  return <section>
    <div className="mb-4"><h1>Calificaciones</h1><p className="text-secondary">Califica tus atenciones finalizadas.</p></div>
    {error && <div className="alert alert-danger">{error}</div>}
    {success && <div className="alert alert-success">{success}</div>}
    <div className="card card-body mb-3">
      <label className="form-label fw-semibold">Atención atendida</label>
      <select className="form-select" value={reservaId} disabled={loading} onChange={event => setReservaId(event.target.value)}>
        <option value="">Selecciona una atención</option>
        {reservas.map(item => <option key={item.id} value={item.id}>{item.fecha} · {item.horaInicio.slice(0, 5)} · {item.servicioNombre} · {item.profesionalNombre ?? 'Sin profesional'}</option>)}
      </select>
      {!loading && reservas.length === 0 && <small className="text-secondary mt-2">Aún no tienes reservas atendidas disponibles para calificar.</small>}
    </div>

    {detalle && <div className="row g-3">
      <div className="col-lg-7"><div className="card card-body h-100">
        <h2 className="h5">Califica tu atención</h2>
        <p className="text-secondary">{detalle.servicio.nombre} · {detalle.profesional?.nombre ?? 'Sin profesional'} · {detalle.fecha} · {detalle.horaInicio.slice(0, 5)}</p>
        <Stars value={puntuacion} onChange={setPuntuacion} disabled={Boolean(calificacion)} />
        <label className="form-label mt-3">Comentario</label>
        <textarea className="form-control" rows={4} maxLength={1000} value={comentario} disabled={Boolean(calificacion)} onChange={event => setComentario(event.target.value)} placeholder="Cuéntanos cómo fue tu atención." />
        <div className="d-flex justify-content-between align-items-center mt-3">
          <small className="text-secondary">Solo las reservas Atendidas pueden calificarse. Una calificación por reserva.</small>
          <button className="btn btn-primary" disabled={saving || puntuacion === 0 || Boolean(calificacion)} onClick={() => void guardar()}>{calificacion ? 'Calificación registrada' : saving ? 'Enviando…' : 'Enviar calificación'}</button>
        </div>
      </div></div>
      <div className="col-lg-5"><div className="card card-body h-100">
        <h2 className="h5">Resumen del profesional</h2>
        {resumen ? <>
          <div className="display-6 fw-bold">{resumen.promedio?.toFixed(1) ?? '—'}</div>
          <div className="text-warning fs-4">★★★★★</div>
          <p className="text-secondary">{resumen.totalCalificaciones} calificaciones</p>
          {distribucion.map(item => <div className="d-flex justify-content-between border-bottom py-2" key={item.estrellas}><span>{item.estrellas} ★</span><strong>{item.cantidad}</strong></div>)}
        </> : <p className="text-secondary mb-0">Sin resumen disponible.</p>}
      </div></div>
    </div>}
  </section>
}
