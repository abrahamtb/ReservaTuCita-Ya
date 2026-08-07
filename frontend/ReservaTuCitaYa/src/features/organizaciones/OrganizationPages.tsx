import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import * as api from '../../api/organizacionesApi'
import { Empty, ErrorAlert, Loading, StatusBadge, SuccessAlert } from '../../components/common/Feedback'
import { Pagination } from '../../components/tables/Pagination'
import type { EstadoFiltro, Option, Organization, OrganizationRequest, PageResult } from '../../types'

const emptyForm: OrganizationRequest = { tipoOrganizacionId: '', nombreComercial: '', numeroDocumento: '' }

export function OrganizationsPage() {
  const [data, setData] = useState<PageResult<Organization>>(); const [error, setError] = useState<unknown>()
  const [search, setSearch] = useState(''); const [state, setState] = useState<EstadoFiltro>('Todos'); const [page, setPage] = useState(1)
  useEffect(() => { const controller = new AbortController(); setError(undefined); api.listOrganizations({ busqueda: search, estado: state, pagina: page }, controller.signal).then(setData).catch(e => { if (e.name !== 'AbortError') setError(e) }); return () => controller.abort() }, [search, state, page])
  return <><div className="d-flex justify-content-between align-items-center mb-3"><div><h1>Organizaciones</h1><p className="text-secondary mb-0">Administración de organizaciones registradas.</p></div><Link className="btn btn-primary" to="nueva">Nueva organización</Link></div>
    <div className="card card-body mb-3"><div className="row g-2"><div className="col-md-8"><input className="form-control" placeholder="Buscar por nombre, razón social o documento" value={search} onChange={e => { setSearch(e.target.value); setPage(1) }} /></div><div className="col-md-4"><select className="form-select" value={state} onChange={e => { setState(e.target.value as EstadoFiltro); setPage(1) }}><option>Todos</option><option>Activos</option><option>Inactivos</option></select></div></div></div>
    {error ? <ErrorAlert error={error} /> : !data ? <Loading /> : data.elementos.length === 0 ? <Empty /> : <div className="card"><div className="table-responsive"><table className="table table-hover mb-0"><thead><tr><th>Nombre</th><th>Documento</th><th>Tipo</th><th>Estado</th><th /></tr></thead><tbody>{data.elementos.map(item => <tr key={item.id}><td>{item.nombreComercial}</td><td>{item.numeroDocumento}</td><td>{item.tipoOrganizacion}</td><td><StatusBadge active={item.estaActivo} /></td><td className="text-end"><Link className="btn btn-sm btn-outline-primary" to={item.id}>Ver</Link></td></tr>)}</tbody></table></div></div>}
    {data ? <div className="mt-3"><Pagination page={data.paginaActual} total={data.totalPaginas} onChange={setPage} /></div> : null}</>
}

export function OrganizationFormPage() {
  const { id } = useParams(); const navigate = useNavigate(); const editing = Boolean(id)
  const [form, setForm] = useState<OrganizationRequest>(emptyForm); const [types, setTypes] = useState<Option[]>([])
  const [loading, setLoading] = useState(true); const [busy, setBusy] = useState(false); const [error, setError] = useState<unknown>()
  useEffect(() => { Promise.all([api.listOrganizationTypes(), id ? api.getOrganization(id) : Promise.resolve(undefined)]).then(([options, item]) => { setTypes(options); if (item) setForm({ tipoOrganizacionId: item.tipoOrganizacionId ?? '', nombreComercial: item.nombreComercial, razonSocial: item.razonSocial, numeroDocumento: item.numeroDocumento, telefono: item.telefono, correo: item.correo, direccionPrincipal: item.direccionPrincipal, logoUrl: item.logoUrl }) }).catch(setError).finally(() => setLoading(false)) }, [id])
  function field(name: keyof OrganizationRequest, value: string) { setForm(current => ({ ...current, [name]: value })) }
  async function submit(event: FormEvent) { event.preventDefault(); setBusy(true); setError(undefined); try { if (id) { await api.updateOrganization(id, form); navigate(`/organizaciones/${id}`, { state: { success: 'Organización actualizada.' } }) } else { const created = await api.createOrganization(form); navigate(`/organizaciones/${created.id}`, { state: { success: 'Organización creada.' } }) } } catch (caught) { setError(caught) } finally { setBusy(false) } }
  if (loading) return <Loading />
  return <><h1>{editing ? 'Editar organización' : 'Nueva organización'}</h1>{error ? <ErrorAlert error={error} /> : null}<form className="card card-body form-card" onSubmit={submit}><div className="row g-3">
    <div className="col-md-6"><label className="form-label">Tipo</label><select required className="form-select" value={form.tipoOrganizacionId} onChange={e => field('tipoOrganizacionId', e.target.value)}><option value="">Selecciona…</option>{types.map(type => <option key={type.id} value={type.id}>{type.nombre}</option>)}</select></div>
    <div className="col-md-6"><label className="form-label">Nombre comercial</label><input required maxLength={150} className="form-control" value={form.nombreComercial} onChange={e => field('nombreComercial', e.target.value)} /></div>
    <div className="col-md-6"><label className="form-label">Razón social</label><input maxLength={200} className="form-control" value={form.razonSocial ?? ''} onChange={e => field('razonSocial', e.target.value)} /></div>
    <div className="col-md-6"><label className="form-label">Documento</label><input required maxLength={20} className="form-control" value={form.numeroDocumento} onChange={e => field('numeroDocumento', e.target.value)} /></div>
    <div className="col-md-6"><label className="form-label">Teléfono</label><input className="form-control" value={form.telefono ?? ''} onChange={e => field('telefono', e.target.value)} /></div>
    <div className="col-md-6"><label className="form-label">Correo</label><input type="email" className="form-control" value={form.correo ?? ''} onChange={e => field('correo', e.target.value)} /></div>
    <div className="col-12"><label className="form-label">Dirección principal</label><input className="form-control" value={form.direccionPrincipal ?? ''} onChange={e => field('direccionPrincipal', e.target.value)} /></div>
    <div className="col-12"><label className="form-label">URL de logo</label><input type="url" className="form-control" value={form.logoUrl ?? ''} onChange={e => field('logoUrl', e.target.value)} /></div>
  </div><div className="d-flex gap-2 mt-4"><button disabled={busy} className="btn btn-primary">{busy ? 'Guardando…' : 'Guardar'}</button><Link className="btn btn-outline-secondary" to={id ? `/organizaciones/${id}` : '/organizaciones'}>Cancelar</Link></div></form></>
}

export function OrganizationDetailPage() {
  const { id = '' } = useParams(); const navigate = useNavigate(); const [item, setItem] = useState<Organization>(); const [error, setError] = useState<unknown>(); const [message, setMessage] = useState('')
  const load = useCallback(() => api.getOrganization(id).then(setItem).catch(setError), [id])
  useEffect(() => { void load() }, [load])
  async function action(kind: 'toggle' | 'delete') { if (!confirm(kind === 'delete' ? '¿Eliminar lógicamente esta organización?' : '¿Cambiar el estado de la organización?')) return; try { if (kind === 'delete') { await api.deleteOrganization(id); navigate('/organizaciones') } else { await api.toggleOrganization(id); setMessage('Estado actualizado.'); await load() } } catch (caught) { setError(caught) } }
  if (error) return <ErrorAlert error={error} />; if (!item) return <Loading />
  return <><SuccessAlert message={message} /><div className="d-flex justify-content-between"><div><h1>{item.nombreComercial}</h1><StatusBadge active={item.estaActivo} /></div><Link className="btn btn-primary align-self-start" to="editar">Editar</Link></div>
    <div className="card card-body my-3"><dl className="row mb-0"><dt className="col-sm-3">Tipo</dt><dd className="col-sm-9">{item.tipoOrganizacion}</dd><dt className="col-sm-3">Documento</dt><dd className="col-sm-9">{item.numeroDocumento}</dd><dt className="col-sm-3">Contacto</dt><dd className="col-sm-9">{item.correo || item.telefono || '—'}</dd><dt className="col-sm-3">Dirección</dt><dd className="col-sm-9">{item.direccionPrincipal || '—'}</dd></dl></div>
    <div className="d-flex gap-2 flex-wrap"><Link className="btn btn-outline-primary" to="sedes">Sedes</Link><Link className="btn btn-outline-primary" to="categorias">Categorías</Link><Link className="btn btn-outline-primary" to="servicios">Servicios</Link><button className="btn btn-outline-warning" onClick={() => void action('toggle')}>{item.estaActivo ? 'Desactivar' : 'Activar'}</button><button className="btn btn-outline-danger" onClick={() => void action('delete')}>Eliminar</button><Link className="btn btn-link" to="/organizaciones">Volver</Link></div></>
}
