import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import * as empleadosApi from '../../api/empleadosApi'
import { listSedes } from '../../api/sedesApi'
import { listServices } from '../../api/serviciosApi'
import { Empty, ErrorAlert, Loading, StatusBadge, SuccessAlert } from '../../components/common/Feedback'
import { Pagination } from '../../components/tables/Pagination'
import type { EmpleadoLista, EstadoFiltro, PageResult, Sede, Servicio } from '../../types'
import type { EmpleadoDetalle, EmpleadoRequest, TipoDocumentoEmpleado } from '../../api/empleadosApi'

const tiposDocumento: { value: TipoDocumentoEmpleado; label: string }[] = [
  { value: 'DNI', label: 'DNI' },
  { value: 'CarnetExtranjeria', label: 'Carnet de extranjería' },
  { value: 'Pasaporte', label: 'Pasaporte' },
  { value: 'RUC', label: 'RUC' },
]

export function EmpleadosPage() {
  const { organizationId = '' } = useParams()
  const [data, setData] = useState<PageResult<EmpleadoLista>>()
  const [error, setError] = useState<unknown>()
  const [search, setSearch] = useState('')
  const [estado, setEstado] = useState<EstadoFiltro>('Todos')
  const [profesional, setProfesional] = useState('')
  const [page, setPage] = useState(1)

  const load = useCallback((signal?: AbortSignal) => {
    if (!organizationId) return Promise.resolve()
    setError(undefined)
    return empleadosApi.listarEmpleados(organizationId, {
      busqueda: search,
      estado,
      esProfesional: profesional === '' ? undefined : profesional === 'true',
      pagina: page,
      tamanoPagina: 10,
    }, signal).then(setData).catch((caught: unknown) => {
      if (!(caught instanceof DOMException && caught.name === 'AbortError')) setError(caught)
    })
  }, [organizationId, search, estado, profesional, page])

  useEffect(() => {
    const controller = new AbortController()
    void load(controller.signal)
    return () => controller.abort()
  }, [load])

  async function toggle(item: EmpleadoLista) {
    try {
      await empleadosApi.cambiarEstadoEmpleado(item.id, !item.estaActivo)
      await load()
    } catch (caught) { setError(caught) }
  }

  async function remove(item: EmpleadoLista) {
    if (!confirm(`¿Eliminar lógicamente a ${item.nombreCompleto}?`)) return
    try {
      await empleadosApi.eliminarEmpleado(item.id)
      await load()
    } catch (caught) { setError(caught) }
  }

  return <>
    <div className="d-flex justify-content-between align-items-center mb-3">
      <div><h1>Empleados y profesionales</h1><p className="text-secondary mb-0">Gestiona el personal, sus sedes y servicios asignados.</p></div>
      <Link className="btn btn-primary" to="nuevo">Nuevo empleado</Link>
    </div>
    <div className="card card-body mb-3"><div className="row g-2">
      <div className="col-lg-6"><input className="form-control" placeholder="Buscar por nombre, documento, correo o cargo" value={search} onChange={e => { setSearch(e.target.value); setPage(1) }} /></div>
      <div className="col-md-3"><select className="form-select" value={estado} onChange={e => { setEstado(e.target.value as EstadoFiltro); setPage(1) }}><option>Todos</option><option>Activos</option><option>Inactivos</option></select></div>
      <div className="col-md-3"><select className="form-select" value={profesional} onChange={e => { setProfesional(e.target.value); setPage(1) }}><option value="">Todo el personal</option><option value="true">Solo profesionales</option><option value="false">No profesionales</option></select></div>
    </div></div>
    {error ? <ErrorAlert error={error} /> : !data ? <Loading /> : data.elementos.length === 0 ? <Empty /> : <div className="card"><div className="table-responsive"><table className="table table-hover align-middle mb-0">
      <thead><tr><th>Empleado</th><th>Documento</th><th>Cargo</th><th>Tipo</th><th>Asignaciones</th><th>Estado</th><th /></tr></thead>
      <tbody>{data.elementos.map(item => <tr key={item.id}>
        <td><div className="fw-semibold">{item.nombreCompleto}</div><small className="text-secondary">{item.correo || item.telefono || 'Sin contacto'}</small></td>
        <td>{item.numeroDocumento}</td><td>{item.cargo}</td><td>{item.esProfesional ? <span className="badge text-bg-info">Profesional</span> : 'Empleado'}</td>
        <td><small>{item.cantidadSedes} sede(s){item.esProfesional ? ` · ${item.cantidadServicios} servicio(s)` : ''}</small></td><td><StatusBadge active={item.estaActivo} /></td>
        <td className="text-end text-nowrap"><Link className="btn btn-sm btn-outline-primary me-1" to={item.id}>Ver</Link><Link className="btn btn-sm btn-outline-secondary me-1" to={`${item.id}/editar`}>Editar</Link><button className="btn btn-sm btn-outline-warning me-1" onClick={() => void toggle(item)}>{item.estaActivo ? 'Desactivar' : 'Activar'}</button><button className="btn btn-sm btn-outline-danger" onClick={() => void remove(item)}>Eliminar</button></td>
      </tr>)}</tbody>
    </table></div></div>}
    {data ? <div className="mt-3"><Pagination page={data.paginaActual} total={data.totalPaginas} onChange={setPage} /></div> : null}
    <Link className="btn btn-link px-0 mt-2" to={`/organizaciones/${organizationId}`}>Volver a la organización</Link>
  </>
}

interface FormState {
  tipoDocumento: TipoDocumentoEmpleado
  numeroDocumento: string
  nombres: string
  apellidos: string
  correo: string
  telefono: string
  direccion: string
  fechaNacimiento: string
  cargo: string
  especialidad: string
  esProfesional: boolean
  numeroColegiatura: string
  observaciones: string
  sedeIds: string[]
  servicioIds: string[]
}

const emptyForm: FormState = {
  tipoDocumento: 'DNI', numeroDocumento: '', nombres: '', apellidos: '', correo: '', telefono: '', direccion: '', fechaNacimiento: '', cargo: '', especialidad: '', esProfesional: false, numeroColegiatura: '', observaciones: '', sedeIds: [], servicioIds: [],
}

const clean = (value: string) => value.trim() || null

export function EmpleadoFormPage() {
  const { organizationId = '', id } = useParams()
  const navigate = useNavigate()
  const [form, setForm] = useState<FormState>(emptyForm)
  const [original, setOriginal] = useState<EmpleadoDetalle>()
  const [sedes, setSedes] = useState<Sede[]>([])
  const [servicios, setServicios] = useState<Servicio[]>([])
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<unknown>()

  useEffect(() => {
    const controller = new AbortController()
    Promise.all([
      listSedes(organizationId, { estado: 'Activos' }, controller.signal),
      listServices(organizationId, { estado: 'Activos', pagina: 1, tamanoPagina: 100 }, controller.signal),
      id ? empleadosApi.obtenerEmpleado(id, controller.signal) : Promise.resolve(undefined),
    ]).then(([siteOptions, servicePage, item]) => {
      setSedes(siteOptions)
      setServicios(servicePage.elementos)
      if (item) {
        setOriginal(item)
        setForm({
          tipoDocumento: item.tipoDocumento,
          numeroDocumento: item.numeroDocumento,
          nombres: item.nombres,
          apellidos: item.apellidos,
          correo: item.correo ?? '',
          telefono: item.telefono ?? '',
          direccion: item.direccion ?? '',
          fechaNacimiento: item.fechaNacimiento ?? '',
          cargo: item.cargo,
          especialidad: item.especialidad ?? '',
          esProfesional: item.esProfesional,
          numeroColegiatura: item.numeroColegiatura ?? '',
          observaciones: item.observaciones ?? '',
          sedeIds: item.sedes.filter(s => s.estaActivo).map(s => s.sedeId),
          servicioIds: item.servicios.filter(s => s.estaActivo).map(s => s.servicioId),
        })
      }
    }).catch((caught: unknown) => {
      if (!(caught instanceof DOMException && caught.name === 'AbortError')) setError(caught)
    }).finally(() => setLoading(false))
    return () => controller.abort()
  }, [organizationId, id])

  function field<K extends keyof FormState>(name: K, value: FormState[K]) { setForm(current => ({ ...current, [name]: value })) }
  function toggleSelection(name: 'sedeIds' | 'servicioIds', value: string) {
    setForm(current => ({ ...current, [name]: current[name].includes(value) ? current[name].filter(idValue => idValue !== value) : [...current[name], value] }))
  }

  async function submit(event: FormEvent) {
    event.preventDefault(); setBusy(true); setError(undefined)
    const request: EmpleadoRequest = {
      tipoDocumento: form.tipoDocumento,
      numeroDocumento: form.numeroDocumento.trim(),
      nombres: form.nombres.trim(),
      apellidos: form.apellidos.trim(),
      correo: clean(form.correo),
      telefono: clean(form.telefono),
      direccion: clean(form.direccion),
      fechaNacimiento: form.fechaNacimiento || null,
      cargo: form.cargo.trim(),
      especialidad: form.esProfesional ? clean(form.especialidad) : null,
      esProfesional: form.esProfesional,
      numeroColegiatura: form.esProfesional ? clean(form.numeroColegiatura) : null,
      observaciones: clean(form.observaciones),
    }
    try {
      if (id) {
        if (original?.esProfesional && !form.esProfesional && original.servicios.length > 0) await empleadosApi.reemplazarServiciosProfesional(id, [])
        await empleadosApi.actualizarEmpleado(id, request)
        await empleadosApi.reemplazarSedesEmpleado(id, form.sedeIds)
        if (form.esProfesional) await empleadosApi.reemplazarServiciosProfesional(id, form.servicioIds)
        navigate(`/organizaciones/${organizationId}/empleados/${id}`)
      } else {
        const created = await empleadosApi.crearEmpleado(organizationId, { ...request, sedeIds: form.sedeIds, servicioIds: form.esProfesional ? form.servicioIds : [] })
        navigate(`/organizaciones/${organizationId}/empleados/${created.id}`)
      }
    } catch (caught) { setError(caught) } finally { setBusy(false) }
  }

  if (loading) return <Loading />
  return <>
    <h1>{id ? 'Editar empleado' : 'Nuevo empleado'}</h1><p className="text-secondary">Registra los datos personales, laborales y asignaciones del empleado.</p>
    {error ? <ErrorAlert error={error} /> : null}
    <form className="card card-body" onSubmit={submit}><div className="row g-3">
      <div className="col-md-4"><label className="form-label">Tipo de documento</label><select className="form-select" required value={form.tipoDocumento} onChange={e => field('tipoDocumento', e.target.value as TipoDocumentoEmpleado)}>{tiposDocumento.map(option => <option key={option.value} value={option.value}>{option.label}</option>)}</select></div>
      <div className="col-md-4"><label className="form-label">Número de documento</label><input className="form-control" required maxLength={20} value={form.numeroDocumento} onChange={e => field('numeroDocumento', e.target.value)} /></div>
      <div className="col-md-4"><label className="form-label">Cargo</label><input className="form-control" required maxLength={120} value={form.cargo} onChange={e => field('cargo', e.target.value)} /></div>
      <div className="col-md-6"><label className="form-label">Nombres</label><input className="form-control" required maxLength={120} value={form.nombres} onChange={e => field('nombres', e.target.value)} /></div>
      <div className="col-md-6"><label className="form-label">Apellidos</label><input className="form-control" required maxLength={120} value={form.apellidos} onChange={e => field('apellidos', e.target.value)} /></div>
      <div className="col-md-6"><label className="form-label">Correo</label><input type="email" className="form-control" value={form.correo} onChange={e => field('correo', e.target.value)} /></div>
      <div className="col-md-6"><label className="form-label">Teléfono</label><input className="form-control" value={form.telefono} onChange={e => field('telefono', e.target.value)} /></div>
      <div className="col-md-8"><label className="form-label">Dirección</label><input className="form-control" value={form.direccion} onChange={e => field('direccion', e.target.value)} /></div>
      <div className="col-md-4"><label className="form-label">Fecha de nacimiento</label><input type="date" className="form-control" value={form.fechaNacimiento} onChange={e => field('fechaNacimiento', e.target.value)} /></div>
      <div className="col-12"><div className="form-check"><input id="es-profesional" className="form-check-input" type="checkbox" checked={form.esProfesional} onChange={e => field('esProfesional', e.target.checked)} /><label htmlFor="es-profesional" className="form-check-label fw-semibold">Es profesional que presta servicios</label></div></div>
      {form.esProfesional ? <><div className="col-md-6"><label className="form-label">Especialidad</label><input className="form-control" value={form.especialidad} onChange={e => field('especialidad', e.target.value)} /></div><div className="col-md-6"><label className="form-label">N.º de colegiatura</label><input className="form-control" value={form.numeroColegiatura} onChange={e => field('numeroColegiatura', e.target.value)} /></div></> : null}
      <div className="col-12"><label className="form-label">Observaciones</label><textarea className="form-control" rows={3} value={form.observaciones} onChange={e => field('observaciones', e.target.value)} /></div>
      <div className="col-12"><label className="form-label fw-semibold">Sedes asignadas</label><div className="border rounded p-3 d-flex flex-wrap gap-3">{sedes.length === 0 ? <span className="text-secondary">No hay sedes activas.</span> : sedes.map(sede => <label className="form-check" key={sede.id}><input className="form-check-input" type="checkbox" checked={form.sedeIds.includes(sede.id)} onChange={() => toggleSelection('sedeIds', sede.id)} /><span className="form-check-label">{sede.nombre}</span></label>)}</div></div>
      {form.esProfesional ? <div className="col-12"><label className="form-label fw-semibold">Servicios que puede atender</label><div className="border rounded p-3 d-flex flex-wrap gap-3">{servicios.length === 0 ? <span className="text-secondary">No hay servicios activos.</span> : servicios.map(servicio => <label className="form-check" key={servicio.id}><input className="form-check-input" type="checkbox" checked={form.servicioIds.includes(servicio.id)} onChange={() => toggleSelection('servicioIds', servicio.id)} /><span className="form-check-label">{servicio.nombre}</span></label>)}</div></div> : null}
    </div><div className="d-flex gap-2 mt-4"><button className="btn btn-primary" disabled={busy}>{busy ? 'Guardando…' : 'Guardar'}</button><Link className="btn btn-outline-secondary" to={id ? `/organizaciones/${organizationId}/empleados/${id}` : `/organizaciones/${organizationId}/empleados`}>Cancelar</Link></div></form>
  </>
}

export function EmpleadoDetailPage() {
  const { organizationId = '', id = '' } = useParams()
  const navigate = useNavigate()
  const [item, setItem] = useState<EmpleadoDetalle>()
  const [error, setError] = useState<unknown>()
  const [message, setMessage] = useState('')
  const load = useCallback(() => empleadosApi.obtenerEmpleado(id).then(setItem).catch(setError), [id])
  useEffect(() => { void load() }, [load])

  async function toggle() {
    if (!item) return
    try { await empleadosApi.cambiarEstadoEmpleado(item.id, !item.estaActivo); setMessage('Estado actualizado.'); await load() } catch (caught) { setError(caught) }
  }
  async function remove() {
    if (!item || !confirm(`¿Eliminar lógicamente a ${item.nombreCompleto}?`)) return
    try { await empleadosApi.eliminarEmpleado(item.id); navigate(`/organizaciones/${organizationId}/empleados`) } catch (caught) { setError(caught) }
  }

  if (error) return <ErrorAlert error={error} />
  if (!item) return <Loading />
  return <>
    <SuccessAlert message={message} />
    <div className="d-flex justify-content-between align-items-start mb-3"><div><h1>{item.nombreCompleto}</h1><div className="d-flex gap-2"><StatusBadge active={item.estaActivo} />{item.esProfesional ? <span className="badge text-bg-info">Profesional</span> : null}</div></div><Link className="btn btn-primary" to="editar">Editar</Link></div>
    <div className="row g-3"><div className="col-lg-7"><div className="card card-body h-100"><h2 className="h5">Datos del empleado</h2><dl className="row mb-0"><dt className="col-sm-4">Documento</dt><dd className="col-sm-8">{item.tipoDocumento} · {item.numeroDocumento}</dd><dt className="col-sm-4">Cargo</dt><dd className="col-sm-8">{item.cargo}</dd><dt className="col-sm-4">Contacto</dt><dd className="col-sm-8">{item.correo || '—'} {item.telefono ? ` · ${item.telefono}` : ''}</dd><dt className="col-sm-4">Dirección</dt><dd className="col-sm-8">{item.direccion || '—'}</dd><dt className="col-sm-4">Nacimiento</dt><dd className="col-sm-8">{item.fechaNacimiento || '—'}</dd>{item.esProfesional ? <><dt className="col-sm-4">Especialidad</dt><dd className="col-sm-8">{item.especialidad || '—'}</dd><dt className="col-sm-4">Colegiatura</dt><dd className="col-sm-8">{item.numeroColegiatura || '—'}</dd></> : null}<dt className="col-sm-4">Observaciones</dt><dd className="col-sm-8">{item.observaciones || '—'}</dd></dl></div></div>
      <div className="col-lg-5"><div className="card card-body mb-3"><h2 className="h5">Sedes</h2>{item.sedes.length === 0 ? <span className="text-secondary">Sin sedes asignadas.</span> : <div className="d-flex flex-wrap gap-2">{item.sedes.map(s => <span key={s.id} className={`badge ${s.estaActivo ? 'text-bg-light border' : 'text-bg-secondary'}`}>{s.nombre}</span>)}</div>}</div>{item.esProfesional ? <div className="card card-body"><h2 className="h5">Servicios</h2>{item.servicios.length === 0 ? <span className="text-secondary">Sin servicios asignados.</span> : <div className="d-flex flex-wrap gap-2">{item.servicios.map(s => <span key={s.id} className={`badge ${s.estaActivo ? 'text-bg-light border' : 'text-bg-secondary'}`}>{s.nombre}</span>)}</div>}</div> : null}</div>
    </div>
    <div className="d-flex gap-2 flex-wrap mt-3"><button className="btn btn-outline-warning" onClick={() => void toggle()}>{item.estaActivo ? 'Desactivar' : 'Activar'}</button><button className="btn btn-outline-danger" onClick={() => void remove()}>Eliminar</button><Link className="btn btn-link" to={`/organizaciones/${organizationId}/empleados`}>Volver</Link></div>
  </>
}
