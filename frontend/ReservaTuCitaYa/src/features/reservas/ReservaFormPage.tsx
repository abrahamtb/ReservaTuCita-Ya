import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { listarClientesOrganizacion, type ClienteOpcion } from '../../api/clientesSeleccionApi'
import { consultarDisponibilidad, profesionalesCompatibles, recursosCompatibles, type HorarioDisponible, type ProfesionalDisponible, type RecursoDisponible } from '../../api/disponibilidadApi'
import { crearReserva, type CrearReservaRequest } from '../../api/reservasApi'
import { listSedes } from '../../api/sedesApi'
import { listServices } from '../../api/serviciosApi'
import { useAuth } from '../../auth/useAuth'
import { ErrorAlert } from '../../components/common/Feedback'
import type { Sede, Servicio } from '../../types'

const today = new Date().toISOString().slice(0, 10)
const steps = ['Cliente', 'Servicio', 'Sede y recursos', 'Fecha y hora', 'Información', 'Confirmar']

export function ReservaFormPage() {
  const { organizationId = '' } = useParams()
  const [query] = useSearchParams()
  const navigate = useNavigate()
  const { user } = useAuth()
  const [step, setStep] = useState(1)
  const [sedes, setSedes] = useState<Sede[]>([])
  const [servicios, setServicios] = useState<Servicio[]>([])
  const [clientes, setClientes] = useState<ClienteOpcion[]>([])
  const [profesionales, setProfesionales] = useState<ProfesionalDisponible[]>([])
  const [recursos, setRecursos] = useState<RecursoDisponible[]>([])
  const [slots, setSlots] = useState<HorarioDisponible[]>([])
  const [loadingOptions, setLoadingOptions] = useState(true)
  const [loadingSlots, setLoadingSlots] = useState(false)
  const [error, setError] = useState<unknown>()
  const [busy, setBusy] = useState(false)
  const [acompanantes, setAcompanantes] = useState<string[]>([])
  const [form, setForm] = useState<CrearReservaRequest>({
    clienteId: user?.clienteId ?? '',
    servicioId: query.get('servicioId') ?? '',
    sedeId: query.get('sedeId') ?? '',
    profesionalId: query.get('profesionalId') || null,
    recursoId: query.get('recursoId') || null,
    fecha: query.get('fecha') ?? today,
    horaInicio: query.get('horaInicio') ?? '',
    cantidadParticipantes: 1,
    participantes: [],
    observaciones: '',
  })

  useEffect(() => {
    if (user?.clienteId) setForm(current => ({ ...current, clienteId: user.clienteId ?? current.clienteId }))
  }, [user?.clienteId])

  useEffect(() => {
    const controller = new AbortController()
    setLoadingOptions(true)
    const clientsPromise = user?.clienteId
      ? Promise.resolve({ elementos: [] as ClienteOpcion[] })
      : listarClientesOrganizacion(organizationId, '', controller.signal)
    Promise.all([
      listSedes(organizationId, { estado: 'Activos' }, controller.signal),
      listServices(organizationId, { estado: 'Activos', pagina: 1, tamanoPagina: 100 }, controller.signal),
      clientsPromise,
    ]).then(([siteItems, servicePage, clientPage]) => {
      setSedes(siteItems)
      setServicios(servicePage.elementos)
      setClientes(clientPage.elementos)
      setForm(current => ({
        ...current,
        sedeId: current.sedeId || siteItems[0]?.id || '',
        servicioId: current.servicioId || servicePage.elementos[0]?.id || '',
        clienteId: current.clienteId || user?.clienteId || clientPage.elementos[0]?.id || '',
      }))
    }).catch(caught => { if (!controller.signal.aborted && (caught as Error).name !== 'AbortError') setError(caught) }).finally(() => { if (!controller.signal.aborted) setLoadingOptions(false) })
    return () => controller.abort()
  }, [organizationId, user?.clienteId])

  const servicio = useMemo(() => servicios.find(item => item.id === form.servicioId), [form.servicioId, servicios])
  const cliente = useMemo(() => clientes.find(item => item.id === form.clienteId), [clientes, form.clienteId])
  const sede = useMemo(() => sedes.find(item => item.id === form.sedeId), [form.sedeId, sedes])
  const profesional = useMemo(() => profesionales.find(item => item.id === form.profesionalId), [form.profesionalId, profesionales])
  const recurso = useMemo(() => recursos.find(item => item.id === form.recursoId), [form.recursoId, recursos])
  const requiereProfesional = Boolean(servicio?.requiereProfesional)
  const requiereRecurso = Boolean(servicio?.requiereRecurso)

  const sedesDisponibles = useMemo(() => {
    if (!servicio?.sedes?.length) return sedes
    const ids = new Set(servicio.sedes.filter(item => item.estaAsignada !== false && item.sedeActiva !== false).map(item => item.sedeId))
    return ids.size ? sedes.filter(item => ids.has(item.id)) : sedes
  }, [sedes, servicio])

  useEffect(() => {
    if (!form.sedeId || !form.servicioId || !form.fecha) return
    const controller = new AbortController()
    setLoadingSlots(true)
    Promise.all([
      profesionalesCompatibles(form.sedeId, form.servicioId, form.fecha, controller.signal),
      recursosCompatibles(form.sedeId, form.servicioId, form.fecha, controller.signal),
      consultarDisponibilidad({
        sedeId: form.sedeId,
        servicioId: form.servicioId,
        fechaDesde: form.fecha,
        fechaHasta: form.fecha,
        profesionalId: form.profesionalId || undefined,
        recursoId: form.recursoId || undefined,
      }, controller.signal),
    ]).then(([professionalItems, resourceItems, availability]) => {
      setProfesionales(professionalItems)
      setRecursos(resourceItems)
      setSlots(availability.dias[0]?.horarios ?? [])
    }).catch(caught => { if (!controller.signal.aborted && (caught as Error).name !== 'AbortError') setError(caught) }).finally(() => { if (!controller.signal.aborted) setLoadingSlots(false) })
    return () => controller.abort()
  }, [form.sedeId, form.servicioId, form.fecha, form.profesionalId, form.recursoId])

  useEffect(() => {
    if (form.sedeId && !sedesDisponibles.some(item => item.id === form.sedeId) && sedesDisponibles.length) {
      setForm(current => ({ ...current, sedeId: sedesDisponibles[0].id, horaInicio: '', profesionalId: null, recursoId: null }))
    }
  }, [form.sedeId, sedesDisponibles])

  function next() {
    setError(undefined)
    if (step === 1 && !form.clienteId) return setError(new Error('Selecciona un cliente.'))
    if (step === 2 && !form.servicioId) return setError(new Error('Selecciona un servicio.'))
    if (step === 3 && !form.sedeId) return setError(new Error('Selecciona una sede.'))
    if (step === 3 && requiereProfesional && profesionales.length === 0) return setError(new Error('No hay profesionales compatibles disponibles.'))
    if (step === 3 && requiereRecurso && recursos.length === 0) return setError(new Error('No hay recursos compatibles disponibles.'))
    if (step === 4 && !form.horaInicio) return setError(new Error('Selecciona un horario disponible.'))
    if (step === 5 && form.cantidadParticipantes < 1) return setError(new Error('La cantidad de participantes debe ser al menos 1.'))
    if (step === 5 && acompanantes.some(nombre => !nombre.trim())) return setError(new Error('Ingresa el nombre de cada participante adicional.'))
    setStep(current => Math.min(6, current + 1))
  }

  async function submit(event: FormEvent) {
    event.preventDefault()
    if (step !== 6) return next()
    setBusy(true); setError(undefined)
    try {
      const participantes = [
        { clienteId: form.clienteId, nombreCompleto: cliente?.nombreCompleto ?? '', esTitular: true },
        ...acompanantes.map(nombreCompleto => ({ nombreCompleto: nombreCompleto.trim(), esTitular: false })),
      ]
      const created = await crearReserva(organizationId, {
        ...form,
        profesionalId: form.profesionalId || null,
        recursoId: form.recursoId || null,
        participantes,
      })
      navigate(`/reservas/${created.id}`)
    } catch (caught) { setError(caught) }
    finally { setBusy(false) }
  }

  if (loadingOptions) return <div className="py-5 text-center">Cargando opciones de reserva…</div>

  return <section className="booking-flow">
    <div className="d-flex justify-content-between align-items-start mb-3"><div><h1>Nueva reserva</h1><p className="text-secondary">Completa los pasos para confirmar una cita disponible.</p></div><Link className="btn btn-outline-secondary" to="/reservas">Cancelar</Link></div>
    <div className="booking-steps">{steps.map((label, index) => { const number = index + 1; return <div key={label} className={`booking-step ${number === step ? 'is-current' : number < step ? 'is-complete' : ''}`}><span>{number}</span><strong>{label}</strong></div> })}</div>
    {error ? <ErrorAlert error={error} /> : null}

    <form className="card card-body" onSubmit={submit}>
      {step === 1 && <div><h2 className="h5">1. Cliente</h2><p className="text-secondary">Selecciona quién recibirá la atención.</p>
        {user?.clienteId ? <div className="alert alert-light border mb-0"><strong>Cliente vinculado a tu cuenta</strong><div className="small text-secondary">La reserva se registrará a tu nombre.</div></div> : <label className="form-label w-100">Cliente<select required className="form-select" value={form.clienteId} onChange={e => setForm({ ...form, clienteId: e.target.value })}><option value="">Selecciona un cliente</option>{clientes.map(item => <option key={item.id} value={item.id}>{item.nombreCompleto} · {item.numeroDocumento}</option>)}</select></label>}
      </div>}

      {step === 2 && <div><h2 className="h5">2. Servicio</h2><p className="text-secondary">Elige el servicio que deseas reservar.</p><div className="row g-3">{servicios.map(item => <div className="col-md-6 col-xl-4" key={item.id}><button type="button" className={`card card-body text-start w-100 h-100 ${form.servicioId === item.id ? 'border-primary border-2' : ''}`} onClick={() => setForm({ ...form, servicioId: item.id, horaInicio: '', profesionalId: null, recursoId: null })}><strong>{item.nombre}</strong><span className="small text-secondary mt-1">{item.duracionMinutos} min · S/ {item.precio.toFixed(2)}</span><span className="small text-secondary">{item.modalidad}</span></button></div>)}</div></div>}

      {step === 3 && <div><h2 className="h5">3. Sede y recursos</h2><p className="text-secondary">Selecciona la sede y, cuando el servicio lo requiera, profesional o recurso.</p><div className="row g-3">
        <div className="col-md-6"><label className="form-label">Sede *</label><select required className="form-select" value={form.sedeId} onChange={e => setForm({ ...form, sedeId: e.target.value, horaInicio: '', profesionalId: null, recursoId: null })}><option value="">Selecciona una sede</option>{sedesDisponibles.map(item => <option key={item.id} value={item.id}>{item.nombre}</option>)}</select></div>
        {requiereProfesional && <div className="col-md-6"><label className="form-label">Profesional</label><select className="form-select" value={form.profesionalId ?? ''} onChange={e => setForm({ ...form, profesionalId: e.target.value || null, horaInicio: '' })}><option value="">Cualquiera disponible</option>{profesionales.map(item => <option key={item.id} value={item.id}>{item.nombreCompleto}</option>)}</select><small className="text-secondary">Puedes dejar “Cualquiera disponible”.</small></div>}
        {requiereRecurso && <div className="col-md-6"><label className="form-label">Recurso</label><select className="form-select" value={form.recursoId ?? ''} onChange={e => setForm({ ...form, recursoId: e.target.value || null, horaInicio: '' })}><option value="">Cualquiera disponible</option>{recursos.map(item => <option key={item.id} value={item.id}>{item.nombre}</option>)}</select></div>}
        {!requiereProfesional && !requiereRecurso && <div className="col-12"><div className="alert alert-info mb-0">Este servicio no requiere seleccionar profesional ni recurso.</div></div>}
      </div></div>}

      {step === 4 && <div><h2 className="h5">4. Fecha y hora</h2><p className="text-secondary">Solo se muestran horarios calculados como disponibles.</p><div className="row g-3"><div className="col-md-4"><label className="form-label">Fecha *</label><input required min={today} type="date" className="form-control" value={form.fecha} onChange={e => setForm({ ...form, fecha: e.target.value, horaInicio: '' })} /></div><div className="col-md-8"><label className="form-label d-block">Horarios disponibles *</label>{loadingSlots ? <span className="text-secondary">Consultando disponibilidad…</span> : <div className="d-flex flex-wrap gap-2">{slots.map((slot, index) => <button type="button" key={`${slot.horaInicio}-${index}`} className={`btn ${form.horaInicio === slot.horaInicio ? 'btn-primary' : 'btn-outline-primary'}`} onClick={() => setForm({ ...form, horaInicio: slot.horaInicio, profesionalId: slot.profesionalId ?? form.profesionalId, recursoId: slot.recursoId ?? form.recursoId })}>{slot.horaInicio.slice(0, 5)}–{slot.horaFinServicio.slice(0, 5)}</button>)}</div>}{!loadingSlots && slots.length === 0 && <small className="text-secondary d-block mt-2">No hay horarios disponibles para la selección actual.</small>}</div></div></div>}

      {step === 5 && <div><h2 className="h5">5. Información</h2><p className="text-secondary">Agrega datos adicionales para la atención.</p><div className="row g-3"><div className="col-md-4"><label className="form-label">Participantes *</label><input required min="1" max={servicio?.capacidadMaxima || undefined} type="number" className="form-control" value={form.cantidadParticipantes} onChange={e => { const cantidad = Math.max(1, Number(e.target.value)); setForm({ ...form, cantidadParticipantes: cantidad }); setAcompanantes(actual => Array.from({ length: cantidad - 1 }, (_, index) => actual[index] ?? '')) }} /><small className="text-secondary">Capacidad máxima: {servicio?.capacidadMaxima ?? '—'}</small></div><div className="col-md-8"><label className="form-label">Observaciones</label><textarea rows={4} className="form-control" value={form.observaciones ?? ''} onChange={e => setForm({ ...form, observaciones: e.target.value })} placeholder="Indicaciones o comentarios para la atención" /></div>{acompanantes.map((nombre, index) => <div className="col-md-6" key={index}><label className="form-label">Participante adicional {index + 1}<input required className="form-control" value={nombre} onChange={e => setAcompanantes(actual => actual.map((item, position) => position === index ? e.target.value : item))} /></label></div>)}</div></div>}

      {step === 6 && <div><h2 className="h5">6. Confirmar</h2><p className="text-secondary">Revisa la información antes de crear la reserva.</p><div className="row g-3"><div className="col-lg-8"><dl className="row border rounded p-3 mb-0"><dt className="col-sm-4">Cliente</dt><dd className="col-sm-8">{user?.clienteId ? 'Cliente vinculado a tu cuenta' : cliente?.nombreCompleto ?? '—'}</dd><dt className="col-sm-4">Servicio</dt><dd className="col-sm-8">{servicio?.nombre ?? '—'} · {servicio?.duracionMinutos ?? 0} min</dd><dt className="col-sm-4">Sede</dt><dd className="col-sm-8">{sede?.nombre ?? '—'}</dd><dt className="col-sm-4">Profesional</dt><dd className="col-sm-8">{profesional?.nombreCompleto ?? (requiereProfesional ? 'Cualquiera disponible' : 'No requerido')}</dd><dt className="col-sm-4">Recurso</dt><dd className="col-sm-8">{recurso?.nombre ?? (requiereRecurso ? 'Cualquiera disponible' : 'No requerido')}</dd><dt className="col-sm-4">Fecha y hora</dt><dd className="col-sm-8">{form.fecha} · {form.horaInicio.slice(0, 5)}</dd><dt className="col-sm-4">Participantes</dt><dd className="col-sm-8">{form.cantidadParticipantes}</dd><dt className="col-sm-4">Observaciones</dt><dd className="col-sm-8">{form.observaciones || '—'}</dd></dl></div><div className="col-lg-4"><div className="card card-body bg-light"><span className="text-secondary">Precio estimado</span><strong className="fs-3">S/ {servicio?.precio.toFixed(2) ?? '0.00'}</strong>{(servicio?.montoAdelanto ?? 0) > 0 && <small>Adelanto requerido: S/ {servicio?.montoAdelanto.toFixed(2)}</small>}</div></div></div></div>}

      <div className="d-flex justify-content-between mt-4"><button type="button" className="btn btn-outline-secondary" disabled={step === 1 || busy} onClick={() => setStep(current => Math.max(1, current - 1))}>Anterior</button>{step < 6 ? <button type="button" className="btn btn-primary" onClick={next}>Continuar</button> : <button disabled={busy} className="btn btn-success">{busy ? 'Confirmando…' : 'Confirmar reserva'}</button>}</div>
    </form>
  </section>
}
