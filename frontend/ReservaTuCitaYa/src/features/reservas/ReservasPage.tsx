import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { listOrganizations } from '../../api/organizacionesApi'
import { listarReservas, type ReservaLista } from '../../api/reservasApi'
import { useAuth } from '../../auth/useAuth'
import { Empty, ErrorAlert, Loading } from '../../components/common/Feedback'
import { Pagination } from '../../components/tables/Pagination'
import type { EstadoReserva, Organization, PageResult } from '../../types'

const estados: (EstadoReserva | '')[] = ['', 'Pendiente', 'Confirmada', 'Presente', 'EnAtencion', 'Atendida', 'Reprogramada', 'Cancelada', 'NoAsistio']

export function ReservasPage() {
  const { user } = useAuth()
  const [organizations, setOrganizations] = useState<Organization[]>([])
  const [organizationId, setOrganizationId] = useState(user?.organizacion?.id ?? '')
  const [state, setState] = useState<EstadoReserva | ''>('')
  const [page, setPage] = useState(1)
  const [data, setData] = useState<PageResult<ReservaLista>>()
  const [error, setError] = useState<unknown>()

  useEffect(() => {
    if (user?.organizacion) {
      setOrganizations([{ id: user.organizacion.id, nombreComercial: user.organizacion.nombre, tipoOrganizacion: '', numeroDocumento: '', estaActivo: true }])
      return
    }
    const controller = new AbortController()
    listOrganizations({ estado: 'Activos', pagina: 1, tamanoPagina: 100 }, controller.signal)
      .then(result => { setOrganizations(result.elementos); setOrganizationId(current => current || result.elementos[0]?.id || '') })
      .catch(caught => { if (!controller.signal.aborted && (caught as Error).name !== 'AbortError') setError(caught) })
    return () => controller.abort()
  }, [user?.organizacion])

  useEffect(() => {
    if (!organizationId) { setData(undefined); return }
    const controller = new AbortController()
    listarReservas(organizationId, { estado: state, pagina: page, tamanoPagina: 10 }, controller.signal)
      .then(setData).catch(caught => { if (!controller.signal.aborted && (caught as Error).name !== 'AbortError') setError(caught) })
    return () => controller.abort()
  }, [organizationId, state, page])

  return <section>
    <div className="d-flex justify-content-between align-items-start mb-3">
      <div><h1>Reservas</h1><p className="text-secondary">Consulta y administra las reservas.</p></div>
      {organizationId ? <Link className="btn btn-primary" to={`/organizaciones/${organizationId}/reservas/nueva`}>Nueva reserva</Link> : null}
    </div>
    {error ? <ErrorAlert error={error} /> : null}
    <div className="card card-body mb-3"><div className="row g-2">
      <div className="col-md-6"><label className="form-label">Organización</label><select className="form-select" disabled={Boolean(user?.organizacion)} value={organizationId} onChange={e => { setOrganizationId(e.target.value); setPage(1) }}><option value="">Selecciona</option>{organizations.map(item => <option key={item.id} value={item.id}>{item.nombreComercial}</option>)}</select></div>
      <div className="col-md-6"><label className="form-label">Estado</label><select className="form-select" value={state} onChange={e => { setState(e.target.value as EstadoReserva | ''); setPage(1) }}>{estados.map(item => <option key={item || 'todos'} value={item}>{item || 'Todos'}</option>)}</select></div>
    </div></div>
    {!organizationId ? <div className="alert alert-info">Selecciona una organización.</div> : !data ? <Loading /> : data.elementos.length === 0 ? <Empty /> : <div className="card"><div className="table-responsive"><table className="table table-hover mb-0"><thead><tr><th>Código</th><th>Fecha</th><th>Cliente</th><th>Servicio</th><th>Profesional</th><th>Estado</th><th /></tr></thead><tbody>{data.elementos.map(item => <tr key={item.id}><td>{item.codigo}</td><td>{item.fecha} {item.horaInicio.slice(0, 5)}</td><td>{item.clienteNombre}</td><td>{item.servicioNombre}</td><td>{item.profesionalNombre ?? '—'}</td><td><span className="badge text-bg-secondary">{item.estado}</span></td><td className="text-end"><Link className="btn btn-sm btn-outline-primary" to={`/reservas/${item.id}`}>Ver</Link></td></tr>)}</tbody></table></div></div>}
    {data ? <div className="mt-3"><Pagination page={data.paginaActual} total={data.totalPaginas} onChange={setPage} /></div> : null}
  </section>
}
