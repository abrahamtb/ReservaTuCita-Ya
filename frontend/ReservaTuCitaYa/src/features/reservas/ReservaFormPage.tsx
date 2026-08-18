import { useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { listarClientesOrganizacion, type ClienteOpcion } from '../../api/clientesSeleccionApi'
import { consultarDisponibilidad, profesionalesCompatibles, recursosCompatibles, type HorarioDisponible, type ProfesionalDisponible, type RecursoDisponible } from '../../api/disponibilidadApi'
import { crearReserva, type CrearReservaRequest } from '../../api/reservasApi'
import { listSedes } from '../../api/sedesApi'
import { listServices } from '../../api/serviciosApi'
import { ErrorAlert } from '../../components/common/Feedback'
import type { Sede, Servicio } from '../../types'

const today = new Date().toISOString().slice(0, 10)

export function ReservaFormPage() {
  const { organizationId = '' } = useParams()
  const [query] = useSearchParams()
  const navigate = useNavigate()
  const [sedes, setSedes] = useState<Sede[]>([])
  const [servicios, setServicios] = useState<Servicio[]>([])
  const [clientes, setClientes] = useState<ClienteOpcion[]>([])
  const [profesionales, setProfesionales] = useState<ProfesionalDisponible[]>([])
  const [recursos, setRecursos] = useState<RecursoDisponible[]>([])
  const [slots, setSlots] = useState<HorarioDisponible[]>([])
  const [error, setError] = useState<unknown>()
  const [busy, setBusy] = useState(false)
  const [form, setForm] = useState<CrearReservaRequest>({
    clienteId: '', servicioId: query.get('servicioId') ?? '', sedeId: query.get('sedeId') ?? '',
    profesionalId: query.get('profesionalId') || null, recursoId: query.get('recursoId') || null,
    fecha: query.get('fecha') ?? today, horaInicio: query.get('horaInicio') ?? '', cantidadParticipantes: 1,
    participantes: [], observaciones: ''
  })

  useEffect(() => {
    const controller = new AbortController()
    Promise.all([
      listSedes(organizationId, { estado: 'Activos' }, controller.signal),
      listServices(organizationId, { estado: 'Activos', pagina: 1, tamanoPagina: 100 }, controller.signal),
      listarClientesOrganizacion(organizationId, '', controller.signal),
    ]).then(([siteItems, servicePage, clientPage]) => {
      setSedes(siteItems); setServicios(servicePage.elementos); setClientes(clientPage.elementos)
      setForm(current => ({ ...current, sedeId: current.sedeId || siteItems[0]?.id || '', servicioId: current.servicioId || servicePage.elementos[0]?.id || '', clienteId: current.clienteId || clientPage.elementos[0]?.id || '' }))
    }).catch(setError)
    return () => controller.abort()
  }, [organizationId])

  useEffect(() => {
    if (!form.sedeId || !form.servicioId || !form.fecha) return
    const controller = new AbortController()
    Promise.all([
      profesionalesCompatibles(form.sedeId, form.servicioId, form.fecha, controller.signal),
      recursosCompatibles(form.sedeId, form.servicioId, form.fecha, controller.signal),
      consultarDisponibilidad({ sedeId: form.sedeId, servicioId: form.servicioId, fechaDesde: form.fecha, fechaHasta: form.fecha, profesionalId: form.profesionalId || undefined, recursoId: form.recursoId || undefined }, controller.signal),
    ]).then(([professionalItems, resourceItems, availability]) => {
      setProfesionales(professionalItems); setRecursos(resourceItems); setSlots(availability.dias[0]?.horarios ?? [])
    }).catch(setError)
    return () => controller.abort()
  }, [form.sedeId, form.servicioId, form.fecha, form.profesionalId, form.recursoId])

  async function submit(event: FormEvent) {
    event.preventDefault(); setBusy(true); setError(undefined)
    try {
      const created = await crearReserva(organizationId, { ...form, profesionalId: form.profesionalId || null, recursoId: form.recursoId || null, participantes: [] })
      navigate(`/reservas/${created.id}`)
    } catch (caught) { setError(caught) } finally { setBusy(false) }
  }

  return <section><h1>Nueva reserva</h1><p className="text-secondary">Selecciona cliente, servicio y un horario realmente disponible.</p>{error ? <ErrorAlert error={error} /> : null}
    <form className="card card-body" onSubmit={submit}><div className="row g-3">
      <div className="col-md-6"><label className="form-label">Cliente</label><select required className="form-select" value={form.clienteId} onChange={e => setForm({ ...form, clienteId: e.target.value })}><option value="">Selecciona</option>{clientes.map(item => <option key={item.id} value={item.id}>{item.nombreCompleto} · {item.numeroDocumento}</option>)}</select></div>
      <div className="col-md-3"><label className="form-label">Sede</label><select required className="form-select" value={form.sedeId} onChange={e => setForm({ ...form, sedeId: e.target.value, horaInicio: '' })}>{sedes.map(item => <option key={item.id} value={item.id}>{item.nombre}</option>)}</select></div>
      <div className="col-md-3"><label className="form-label">Servicio</label><select required className="form-select" value={form.servicioId} onChange={e => setForm({ ...form, servicioId: e.target.value, horaInicio: '' })}>{servicios.map(item => <option key={item.id} value={item.id}>{item.nombre}</option>)}</select></div>
      <div className="col-md-3"><label className="form-label">Fecha</label><input required type="date" className="form-control" value={form.fecha} onChange={e => setForm({ ...form, fecha: e.target.value, horaInicio: '' })} /></div>
      <div className="col-md-3"><label className="form-label">Profesional</label><select className="form-select" value={form.profesionalId ?? ''} onChange={e => setForm({ ...form, profesionalId: e.target.value || null, horaInicio: '' })}><option value="">Cualquiera</option>{profesionales.map(item => <option key={item.id} value={item.id}>{item.nombreCompleto}</option>)}</select></div>
      <div className="col-md-3"><label className="form-label">Recurso</label><select className="form-select" value={form.recursoId ?? ''} onChange={e => setForm({ ...form, recursoId: e.target.value || null, horaInicio: '' })}><option value="">Cualquiera</option>{recursos.map(item => <option key={item.id} value={item.id}>{item.nombre}</option>)}</select></div>
      <div className="col-md-3"><label className="form-label">Participantes</label><input required min="1" type="number" className="form-control" value={form.cantidadParticipantes} onChange={e => setForm({ ...form, cantidadParticipantes: Number(e.target.value) })} /></div>
      <div className="col-12"><label className="form-label d-block">Horarios disponibles</label><div className="d-flex flex-wrap gap-2">{slots.map((slot, index) => <button type="button" key={`${slot.horaInicio}-${index}`} className={`btn ${form.horaInicio === slot.horaInicio ? 'btn-primary' : 'btn-outline-primary'}`} onClick={() => setForm({ ...form, horaInicio: slot.horaInicio, profesionalId: slot.profesionalId ?? form.profesionalId, recursoId: slot.recursoId ?? form.recursoId })}>{slot.horaInicio.slice(0, 5)}–{slot.horaFinServicio.slice(0, 5)}</button>)}</div>{slots.length === 0 ? <small className="text-secondary">No hay horarios disponibles para la selección actual.</small> : null}</div>
      <div className="col-12"><label className="form-label">Observaciones</label><textarea className="form-control" value={form.observaciones ?? ''} onChange={e => setForm({ ...form, observaciones: e.target.value })} /></div>
    </div><div className="d-flex gap-2 mt-4"><button disabled={busy || !form.horaInicio} className="btn btn-primary">{busy ? 'Guardando…' : 'Crear reserva'}</button><Link className="btn btn-outline-secondary" to="/reservas">Cancelar</Link></div></form>
  </section>
}
