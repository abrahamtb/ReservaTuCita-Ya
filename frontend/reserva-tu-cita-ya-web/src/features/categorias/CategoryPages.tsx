import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import * as api from '../../api/categoriasApi'
import { Empty, ErrorAlert, Loading, StatusBadge, SuccessAlert } from '../../components/common/Feedback'
import { Pagination } from '../../components/tables/Pagination'
import type { Categoria, CategoriaRequest, EstadoFiltro, PageResult } from '../../types'

export function CategoriesPage() {
  const { organizationId = '' } = useParams(); const [data, setData] = useState<PageResult<Categoria>>(); const [error, setError] = useState<unknown>()
  const [search, setSearch] = useState(''); const [state, setState] = useState<EstadoFiltro>('Todos'); const [page, setPage] = useState(1)
  useEffect(() => { const controller = new AbortController(); api.listCategories(organizationId, { busqueda: search, estado: state, pagina: page }, controller.signal).then(setData).catch(e => { if (e.name !== 'AbortError') setError(e) }); return () => controller.abort() }, [organizationId, search, state, page])
  return <><div className="d-flex justify-content-between"><div><h1>Categorías</h1><p className="text-secondary">Categorías de servicios de la organización.</p></div><Link className="btn btn-primary align-self-start" to="nueva">Nueva categoría</Link></div><div className="card card-body mb-3"><div className="row g-2"><div className="col-md-8"><input className="form-control" placeholder="Buscar categoría" value={search} onChange={e => { setSearch(e.target.value); setPage(1) }} /></div><div className="col-md-4"><select className="form-select" value={state} onChange={e => { setState(e.target.value as EstadoFiltro); setPage(1) }}><option>Todos</option><option>Activos</option><option>Inactivos</option></select></div></div></div>
    {error ? <ErrorAlert error={error} /> : !data ? <Loading /> : data.elementos.length === 0 ? <Empty /> : <div className="card table-responsive"><table className="table table-hover mb-0"><thead><tr><th>Nombre</th><th>Descripción</th><th>Servicios</th><th>Estado</th><th /></tr></thead><tbody>{data.elementos.map(item => <tr key={item.id}><td>{item.nombre}</td><td>{item.descripcion || '—'}</td><td>{item.cantidadServicios}</td><td><StatusBadge active={item.estaActivo} /></td><td className="text-end"><Link className="btn btn-sm btn-outline-primary" to={`/categorias/${item.id}`}>Ver</Link></td></tr>)}</tbody></table></div>}
    {data ? <div className="mt-3"><Pagination page={data.paginaActual} total={data.totalPaginas} onChange={setPage} /></div> : null}<Link className="btn btn-link" to={`/organizaciones/${organizationId}`}>Volver</Link></>
}

export function CategoryFormPage() {
  const { organizationId, id } = useParams(); const navigate = useNavigate(); const [form, setForm] = useState<CategoriaRequest>({ nombre: '', descripcion: '' }); const [loading, setLoading] = useState(Boolean(id)); const [error, setError] = useState<unknown>(); const [busy, setBusy] = useState(false)
  useEffect(() => { if (!id) return; api.getCategory(id).then(item => setForm({ nombre: item.nombre, descripcion: item.descripcion })).catch(setError).finally(() => setLoading(false)) }, [id])
  async function submit(event: FormEvent) { event.preventDefault(); setBusy(true); setError(undefined); try { if (id) { await api.updateCategory(id, form); navigate(`/categorias/${id}`) } else if (organizationId) { const created = await api.createCategory(organizationId, form); navigate(`/categorias/${created.id}`) } } catch (caught) { setError(caught) } finally { setBusy(false) } }
  if (loading) return <Loading />
  return <><h1>{id ? 'Editar categoría' : 'Nueva categoría'}</h1>{error ? <ErrorAlert error={error} /> : null}<form className="card card-body form-card" onSubmit={submit}><label className="form-label">Nombre</label><input required maxLength={150} className="form-control mb-3" value={form.nombre} onChange={e => setForm({ ...form, nombre: e.target.value })} /><label className="form-label">Descripción</label><textarea maxLength={500} className="form-control" value={form.descripcion ?? ''} onChange={e => setForm({ ...form, descripcion: e.target.value })} /><div className="d-flex gap-2 mt-4"><button disabled={busy} className="btn btn-primary">Guardar</button><button type="button" className="btn btn-outline-secondary" onClick={() => navigate(-1)}>Cancelar</button></div></form></>
}

export function CategoryDetailPage() {
  const { id = '' } = useParams(); const navigate = useNavigate(); const [item, setItem] = useState<Categoria>(); const [error, setError] = useState<unknown>(); const [message, setMessage] = useState('')
  const load = useCallback(async () => { setItem(await api.getCategory(id)) }, [id])
  useEffect(() => { void load().catch(setError) }, [load])
  async function toggle() { const hasActive = (item?.cantidadServiciosActivos ?? 0) > 0; if (!confirm(hasActive ? 'La categoría tiene servicios activos. ¿Desactivarla sin modificar esos servicios?' : '¿Cambiar el estado?')) return; try { await api.toggleCategory(id, hasActive); setMessage('Estado actualizado.'); await load() } catch (caught) { setError(caught) } }
  async function remove() { if (!confirm('¿Eliminar lógicamente esta categoría?')) return; try { await api.deleteCategory(id); navigate(`/organizaciones/${item?.organizacionId}/categorias`) } catch (caught) { setError(caught) } }
  if (error) return <ErrorAlert error={error} />; if (!item) return <Loading />
  return <><SuccessAlert message={message} /><div className="d-flex justify-content-between"><div><h1>{item.nombre}</h1><StatusBadge active={item.estaActivo} /></div><Link className="btn btn-primary align-self-start" to="editar">Editar</Link></div>{item.estaActivo && (item.cantidadServiciosActivos ?? 0) > 0 ? <div className="alert alert-warning mt-3">Tiene {item.cantidadServiciosActivos} servicio(s) activo(s).</div> : null}<div className="card card-body my-3"><p><strong>Organización:</strong> {item.organizacion}</p><p><strong>Descripción:</strong> {item.descripcion || '—'}</p><p className="mb-0"><strong>Servicios:</strong> {item.cantidadServicios}</p></div><div className="d-flex gap-2"><button className="btn btn-outline-warning" onClick={() => void toggle()}>{item.estaActivo ? 'Desactivar' : 'Activar'}</button><button className="btn btn-outline-danger" onClick={() => void remove()}>Eliminar</button><Link className="btn btn-link" to={`/organizaciones/${item.organizacionId}/categorias`}>Volver</Link></div></>
}
