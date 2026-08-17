import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import * as api from '../../api/sedesApi'
import { Empty, ErrorAlert, Loading, StatusBadge, SuccessAlert } from '../../components/common/Feedback'
import type { EstadoFiltro, Sede, SedeRequest } from '../../types'

const emptyForm: SedeRequest = { nombre: '', direccion: '' }

export function SedesPage() {
  const { organizationId = '' } = useParams(); const [items, setItems] = useState<Sede[]>(); const [error, setError] = useState<unknown>()
  const [search, setSearch] = useState(''); const [state, setState] = useState<EstadoFiltro>('Todos')
  useEffect(() => { const controller = new AbortController(); api.listSedes(organizationId, { busqueda: search, estado: state }, controller.signal).then(res => setItems(res.registros)).catch(e => { if (e.name !== 'AbortError') setError(e) }); return () => controller.abort() }, [organizationId, search, state])
  return <><div className="d-flex justify-content-between"><div><h1>Sedes</h1><p className="text-secondary">Sedes de la organización seleccionada.</p></div><Link className="btn btn-primary align-self-start" to="nueva">Nueva sede</Link></div>
    <div className="card card-body mb-3"><div className="row g-2"><div className="col-md-8"><input className="form-control" placeholder="Buscar sede" value={search} onChange={e => setSearch(e.target.value)} /></div><div className="col-md-4"><select className="form-select" value={state} onChange={e => setState(e.target.value as EstadoFiltro)}><option>Todos</option><option>Activos</option><option>Inactivos</option></select></div></div></div>
    {error ? <ErrorAlert error={error} /> : !items ? <Loading /> : items.length === 0 ? <Empty /> : <div className="card table-responsive"><table className="table table-hover mb-0"><thead><tr><th>Nombre</th><th>Dirección</th><th>Contacto</th><th>Estado</th><th /></tr></thead><tbody>{items.map(item => <tr key={item.id}><td>{item.nombre}</td><td>{item.direccion}</td><td>{item.correo || item.telefono || '—'}</td><td><StatusBadge active={item.estaActivo} /></td><td className="text-end"><Link className="btn btn-sm btn-outline-primary" to={`/sedes/${item.id}`}>Ver</Link></td></tr>)}</tbody></table></div>}
    <Link className="btn btn-link mt-3" to={`/organizaciones/${organizationId}`}>Volver a la organización</Link></>
}

export function SedeFormPage() {
  const { organizationId, id } = useParams(); const navigate = useNavigate(); const [form, setForm] = useState<SedeRequest>(emptyForm)
  const [loading, setLoading] = useState(Boolean(id)); const [busy, setBusy] = useState(false); const [error, setError] = useState<unknown>()
  useEffect(() => { if (!id) return; api.getSede(id).then(item => setForm({ nombre: item.nombre, direccion: item.direccion, telefono: item.telefono, correo: item.correo, referencia: item.referencia })).catch(setError).finally(() => setLoading(false)) }, [id])
  function field(name: keyof SedeRequest, value: string) { setForm(current => ({ ...current, [name]: value })) }
  async function submit(event: FormEvent) { event.preventDefault(); setBusy(true); setError(undefined); try { if (id) { await api.updateSede(id, form); navigate(`/sedes/${id}`) } else if (organizationId) { const created = await api.createSede(organizationId, form); navigate(`/sedes/${created.id}`) } } catch (caught) { setError(caught) } finally { setBusy(false) } }
  if (loading) return <Loading />
  return <><h1>{id ? 'Editar sede' : 'Nueva sede'}</h1>{error ? <ErrorAlert error={error} /> : null}<form className="card card-body form-card" onSubmit={submit}><div className="row g-3"><div className="col-md-6"><label className="form-label">Nombre</label><input required className="form-control" value={form.nombre} onChange={e => field('nombre', e.target.value)} /></div><div className="col-md-6"><label className="form-label">Dirección</label><input required className="form-control" value={form.direccion} onChange={e => field('direccion', e.target.value)} /></div><div className="col-md-6"><label className="form-label">Teléfono</label><input className="form-control" value={form.telefono ?? ''} onChange={e => field('telefono', e.target.value)} /></div><div className="col-md-6"><label className="form-label">Correo</label><input type="email" className="form-control" value={form.correo ?? ''} onChange={e => field('correo', e.target.value)} /></div><div className="col-12"><label className="form-label">Referencia</label><textarea className="form-control" value={form.referencia ?? ''} onChange={e => field('referencia', e.target.value)} /></div></div><div className="mt-4 d-flex gap-2"><button disabled={busy} className="btn btn-primary">Guardar</button><button type="button" className="btn btn-outline-secondary" onClick={() => navigate(-1)}>Cancelar</button></div></form></>
}

export function SedeDetailPage() {
  const { id = '' } = useParams(); const navigate = useNavigate(); const [item, setItem] = useState<Sede>(); const [error, setError] = useState<unknown>(); const [message, setMessage] = useState('')
  const load = useCallback(async () => { setItem(await api.getSede(id)) }, [id])
  useEffect(() => { void load().catch(setError) }, [load])
  async function action(kind: 'toggle' | 'delete') { if (!confirm(kind === 'delete' ? '¿Eliminar lógicamente esta sede?' : '¿Cambiar su estado?')) return; try { if (kind === 'delete') { await api.deleteSede(id); navigate(`/organizaciones/${item?.organizacionId}/sedes`) } else { await api.toggleSede(id); setMessage('Estado actualizado.'); await load() } } catch (caught) { setError(caught) } }
  if (error) return <ErrorAlert error={error} />; if (!item) return <Loading />
  return <><SuccessAlert message={message} /><div className="d-flex justify-content-between"><div><h1>{item.nombre}</h1><StatusBadge active={item.estaActivo} /></div><Link className="btn btn-primary align-self-start" to="editar">Editar</Link></div><div className="card card-body my-3"><p><strong>Organización:</strong> {item.organizacion}</p><p><strong>Dirección:</strong> {item.direccion}</p><p><strong>Contacto:</strong> {item.correo || item.telefono || '—'}</p><p className="mb-0"><strong>Referencia:</strong> {item.referencia || '—'}</p></div><div className="d-flex gap-2"><button className="btn btn-outline-warning" onClick={() => void action('toggle')}>{item.estaActivo ? 'Desactivar' : 'Activar'}</button><button className="btn btn-outline-danger" onClick={() => void action('delete')}>Eliminar</button><Link className="btn btn-link" to={`/organizaciones/${item.organizacionId}/sedes`}>Volver</Link></div></>
}
