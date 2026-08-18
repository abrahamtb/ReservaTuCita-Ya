import { useEffect, useState, type FormEvent } from 'react'
import { consultarDisponibilidad, type DisponibilidadRespuesta } from '../../api/disponibilidadApi'
import { listOrganizations } from '../../api/organizacionesApi'
import { listSedes } from '../../api/sedesApi'
import { listServices } from '../../api/serviciosApi'
import { useAuth } from '../../auth/useAuth'
import { ErrorAlert, Loading } from '../../components/common/Feedback'
import type { Organization, Sede, Servicio } from '../../types'

const today = new Date().toISOString().slice(0, 10)

export function DisponibilidadPage() {
  const { user } = useAuth()
  const [organizations, setOrganizations] = useState<Organization[]>([])
  const [organizationId, setOrganizationId] = useState(user?.organizacion?.id ?? '')
  const [sedes, setSedes] = useState<Sede[]>([])
  const [servicios, setServicios] = useState<Servicio[]>([])
  const [sedeId, setSedeId] = useState('')
  const [servicioId, setServicioId] = useState('')
  const [fechaDesde, setFechaDesde] = useState(today)
  const [fechaHasta, setFechaHasta] = useState(today)
  const [resultado, setResultado] = useState<DisponibilidadRespuesta>()
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<unknown>()

  useEffect(() => {
    if (user?.organizacion) {
      setOrganizations([{ id: user.organizacion.id, nombreComercial: user.organizacion.nombre, tipoOrganizacion: '', numeroDocumento: '', estaActivo: true }])
      setOrganizationId(user.organizacion.id)
      return
    }
    const controller = new AbortController()
    listOrganizations({ estado: 'Activos', pagina: 1, tamanoPagina: 100 }, controller.signal)
      .then(result => { setOrganizations(result.elementos); setOrganizationId(current => current || result.elementos[0]?.id || '') })
      .catch(setError)
    return () => controller.abort()
  }, [user?.organizacion])

  useEffect(() => {
    if (!organizationId) { setSedes([]); setServicios([]); return }
    const controller = new AbortController()
    Promise.all([
      listSedes(organizationId, { estado: 'Activos' }, controller.signal),
      listServices(organizationId, { estado: 'Activos', pagina: 1, tamanoPagina: 100 }, controller.signal),
    ]).then(([siteItems, servicePage]) => {
      setSedes(siteItems); setServicios(servicePage.elementos)
      setSedeId(current => siteItems.some(x => x.id === current) ? current : siteItems[0]?.id || '')
      setServicioId(current => servicePage.elementos.some(x => x.id === current) ? current : servicePage.elementos[0]?.id || '')
    }).catch(setError)
    return () => controller.abort()
  }, [organizationId])

  async function submit(event: FormEvent) {
    event.preventDefault(); setLoading(true); setError(undefined)
    try { setResultado(await consultarDisponibilidad({ sedeId, servicioId, fechaDesde, fechaHasta })) }
    catch (caught) { setError(caught) } finally { setLoading(false) }
  }

  return <section><h1>Disponibilidad</h1><p className="text-secondary">Consulta horarios libres por organización, sede y servicio.</p>{error ? <ErrorAlert error={error} /> : null}
    <form className="card card-body mb-4" onSubmit={submit}><div className="row g-3">
      <div className="col-md-4"><label className="form-label">Organización</label><select required className="form-select" disabled={Boolean(user?.organizacion)} value={organizationId} onChange={e => setOrganizationId(e.target.value)}><option value="">Selecciona</option>{organizations.map(x => <option key={x.id} value={x.id}>{x.nombreComercial}</option>)}</select></div>
      <div className="col-md-4"><label className="form-label">Sede</label><select required className="form-select" value={sedeId} onChange={e => setSedeId(e.target.value)}><option value="">Selecciona</option>{sedes.map(x => <option key={x.id} value={x.id}>{x.nombre}</option>)}</select></div>
      <div className="col-md-4"><label className="form-label">Servicio</label><select required className="form-select" value={servicioId} onChange={e => setServicioId(e.target.value)}><option value="">Selecciona</option>{servicios.map(x => <option key={x.id} value={x.id}>{x.nombre}</option>)}</select></div>
      <div className="col-md-3"><label className="form-label">Desde</label><input required type="date" className="form-control" value={fechaDesde} onChange={e => setFechaDesde(e.target.value)} /></div>
      <div className="col-md-3"><label className="form-label">Hasta</label><input required type="date" min={fechaDesde} className="form-control" value={fechaHasta} onChange={e => setFechaHasta(e.target.value)} /></div>
    </div><button disabled={loading || !sedeId || !servicioId} className="btn btn-primary mt-3 align-self-start">Consultar</button></form>
    {loading ? <Loading /> : resultado ? <div className="row g-3">{resultado.dias.map(dia => <div className="col-lg-6" key={dia.fecha}><div className="card h-100"><div className="card-header d-flex justify-content-between"><strong>{dia.fecha}</strong><span className={`badge ${dia.estaDisponible ? 'text-bg-success' : 'text-bg-secondary'}`}>{dia.estaDisponible ? 'Disponible' : 'Sin horarios'}</span></div><div className="card-body">{dia.horarios.length === 0 ? <span className="text-secondary">No hay horarios disponibles.</span> : <div className="d-flex flex-wrap gap-2">{dia.horarios.map((slot, index) => <span className="badge text-bg-primary p-2" key={`${slot.horaInicio}-${index}`}>{slot.horaInicio.slice(0, 5)}–{slot.horaFinServicio.slice(0, 5)}{slot.profesionalNombre ? ` · ${slot.profesionalNombre}` : ''}</span>)}</div>}</div></div></div>)}</div> : null}
  </section>
}
