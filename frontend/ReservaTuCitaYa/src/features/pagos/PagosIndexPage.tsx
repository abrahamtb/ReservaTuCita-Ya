import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { listOrganizations } from '../../api/organizacionesApi'
import { listarReservas, type ReservaLista } from '../../api/reservasApi'
import { useAuth } from '../../auth/useAuth'
import type { Organization } from '../../types'

export function PagosIndexPage() {
  const { user } = useAuth()
  const superAdmin = user?.roles.includes('Superadministrador') ?? false
  const [organizaciones, setOrganizaciones] = useState<Organization[]>([])
  const [organizacionId, setOrganizacionId] = useState(user?.organizacion?.id ?? '')
  const [reservas, setReservas] = useState<ReservaLista[]>([])
  const [busqueda, setBusqueda] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    if (user?.organizacion) {
      setOrganizaciones([{ id: user.organizacion.id, nombreComercial: user.organizacion.nombre, tipoOrganizacion: '', numeroDocumento: '', estaActivo: true }])
      setOrganizacionId(user.organizacion.id)
      return
    }
    if (!superAdmin) return
    listOrganizations({ estado: 'Activos', pagina: 1, tamanoPagina: 100 }).then(result => {
      setOrganizaciones(result.elementos)
      setOrganizacionId(current => current || result.elementos[0]?.id || '')
    }).catch(caught => setError(caught instanceof Error ? caught.message : 'No se pudieron cargar las organizaciones.'))
  }, [superAdmin, user?.organizacion])

  useEffect(() => {
    if (!organizacionId) { setReservas([]); setLoading(false); return }
    const controller = new AbortController()
    setLoading(true); setError('')
    listarReservas(organizacionId, { pagina: 1, tamanoPagina: 100 }, controller.signal)
      .then(result => setReservas(result.elementos))
      .catch(caught => { if (!controller.signal.aborted) setError(caught instanceof Error ? caught.message : 'No se pudieron cargar las reservas.') })
      .finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [organizacionId])

  const items = useMemo(() => reservas.filter(item => {
    const q = busqueda.trim().toLowerCase()
    return !q || `${item.codigo} ${item.clienteNombre} ${item.servicioNombre}`.toLowerCase().includes(q)
  }), [busqueda, reservas])

  return <section>
    <div className="mb-4"><h1>Pagos</h1><p className="text-secondary">Selecciona una reserva para consultar su resumen económico, registrar pagos o reembolsos.</p></div>
    {error && <div className="alert alert-danger">{error}</div>}
    <div className="card card-body mb-3"><div className="row g-2">
      {superAdmin && <div className="col-md-4"><label className="form-label">Organización</label><select className="form-select" value={organizacionId} onChange={e => setOrganizacionId(e.target.value)}>{organizaciones.map(org => <option value={org.id} key={org.id}>{org.nombreComercial}</option>)}</select></div>}
      <div className={superAdmin ? 'col-md-8' : 'col-12'}><label className="form-label">Buscar reserva</label><input className="form-control" placeholder="Código, cliente o servicio" value={busqueda} onChange={e => setBusqueda(e.target.value)} /></div>
    </div></div>
    {loading ? <div className="py-5 text-center">Cargando reservas…</div> : <div className="card table-responsive"><table className="table table-hover align-middle mb-0"><thead><tr><th>Reserva</th><th>Cliente</th><th>Servicio</th><th>Fecha</th><th>Estado</th><th /></tr></thead><tbody>{items.map(item => <tr key={item.id}><td>{item.codigo}</td><td>{item.clienteNombre}</td><td>{item.servicioNombre}</td><td>{item.fecha} · {item.horaInicio.slice(0, 5)}</td><td>{item.estado}</td><td className="text-end"><Link className="btn btn-sm btn-outline-primary" to={`/pagos/${item.id}`}>Ver pagos</Link></td></tr>)}{items.length === 0 && <tr><td colSpan={6} className="text-center text-secondary py-4">No hay reservas para mostrar.</td></tr>}</tbody></table></div>}
  </section>
}
