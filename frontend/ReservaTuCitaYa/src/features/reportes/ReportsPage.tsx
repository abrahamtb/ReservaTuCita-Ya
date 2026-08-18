import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { ApiError } from '../../api/apiClient'
import { listarProfesionales } from '../../api/empleadosApi'
import { listarMetodosPago } from '../../api/metodosPagoApi'
import { listOrganizations } from '../../api/organizacionesApi'
import { listSedes } from '../../api/sedesApi'
import { listServices } from '../../api/serviciosApi'
import { useAuth } from '../../auth/useAuth'
import type { Organization } from '../../types'
import { AttentionsReport } from './AttentionsReport'
import { IncomeReport } from './IncomeReport'
import { ReservationsReport } from './ReservationsReport'
import type { SelectOption } from './ReportShared'
import './reportes.css'

type ReportTab = 'reservas' | 'ingresos' | 'atenciones'

export function ReportsPage() {
  const { user } = useAuth()
  const navigate = useNavigate()
  const [tab, setTab] = useState<ReportTab>('reservas')
  const [organizations, setOrganizations] = useState<Organization[]>([])
  const [organizationId, setOrganizationId] = useState(user?.organizacion?.id ?? '')
  const [sites, setSites] = useState<SelectOption[]>([])
  const [professionals, setProfessionals] = useState<SelectOption[]>([])
  const [services, setServices] = useState<SelectOption[]>([])
  const [paymentMethods, setPaymentMethods] = useState<SelectOption[]>([])
  const [loadingOptions, setLoadingOptions] = useState(true)
  const [optionsError, setOptionsError] = useState('')

  useEffect(() => {
    const controller = new AbortController()
    if (user?.organizacion) {
      setOrganizations([{ id: user.organizacion.id, nombreComercial: user.organizacion.nombre, tipoOrganizacion: '', numeroDocumento: '', estaActivo: true }])
      setOrganizationId(user.organizacion.id)
      return () => controller.abort()
    }
    listOrganizations({ estado: 'Activos', pagina: 1, tamanoPagina: 100 }, controller.signal).then(response => {
      setOrganizations(response.elementos)
      setOrganizationId(current => current || response.elementos[0]?.id || '')
    }).catch(caught => {
      if (controller.signal.aborted) return
      if (caught instanceof ApiError && caught.status === 403) return navigate('/acceso-denegado', { replace: true })
      setOptionsError(caught instanceof Error ? caught.message : 'No se pudieron cargar las organizaciones.')
    })
    return () => controller.abort()
  }, [navigate, user?.organizacion])

  useEffect(() => {
    const controller = new AbortController()
    listarMetodosPago(controller.signal).then(items => setPaymentMethods(items.map(item => ({ id: item.id, nombre: item.nombre })))).catch(() => setPaymentMethods([]))
    return () => controller.abort()
  }, [])

  useEffect(() => {
    if (!organizationId) { setSites([]); setProfessionals([]); setServices([]); setLoadingOptions(false); return }
    const controller = new AbortController()
    setLoadingOptions(true); setOptionsError('')
    Promise.all([
      listSedes(organizationId, { estado: 'Activos' }, controller.signal),
      listarProfesionales(organizationId, controller.signal),
      listServices(organizationId, { estado: 'Activos', pagina: 1, tamanoPagina: 100 }, controller.signal),
    ]).then(([siteItems, professionalPage, servicePage]) => {
      setSites(siteItems.map(item => ({ id: item.id, nombre: item.nombre })))
      setProfessionals(professionalPage.elementos.map(item => ({ id: item.id, nombre: item.nombreCompleto })))
      setServices(servicePage.elementos.map(item => ({ id: item.id, nombre: item.nombre })))
    }).catch(caught => {
      if (controller.signal.aborted) return
      if (caught instanceof ApiError && caught.status === 403) return navigate('/acceso-denegado', { replace: true })
      setOptionsError(caught instanceof Error ? caught.message : 'No se pudieron cargar las opciones de filtros.')
    }).finally(() => { if (!controller.signal.aborted) setLoadingOptions(false) })
    return () => controller.abort()
  }, [navigate, organizationId])

  return <section className="reports-page">
    <header className="reports-header"><h1>Reportes</h1><p>Consulta información histórica de la operación.</p></header>
    <div className="report-tabs" role="tablist" aria-label="Tipos de reporte">
      {([['reservas', 'Reservas'], ['ingresos', 'Ingresos'], ['atenciones', 'Atenciones']] as const).map(([id, label]) => <button type="button" role="tab" aria-selected={tab === id} className={tab === id ? 'active' : ''} key={id} onClick={() => setTab(id)}>{label}</button>)}
    </div>
    <div className="report-filters"><label>Organización<select className="form-select" value={organizationId} disabled={Boolean(user?.organizacion)} onChange={event => setOrganizationId(event.target.value)}><option value="">Selecciona una organización</option>{organizations.map(item => <option key={item.id} value={item.id}>{item.nombreComercial}</option>)}</select></label></div>
    {optionsError ? <div className="alert alert-danger" role="alert">{optionsError}</div> : null}
    {!loadingOptions && !organizationId ? <div className="alert alert-info d-flex justify-content-between align-items-center gap-3"><span>No hay organizaciones activas para consultar.</span><Link className="btn btn-sm btn-primary" to="/organizaciones/nueva">Crear organización</Link></div> : null}
    {loadingOptions ? <div className="alert alert-light border">Cargando filtros…</div> : organizationId ? <>
      {tab === 'reservas' ? <ReservationsReport organizacionId={organizationId} sedes={sites} profesionales={professionals} servicios={services} /> : null}
      {tab === 'ingresos' ? <IncomeReport organizacionId={organizationId} sedes={sites} metodos={paymentMethods} /> : null}
      {tab === 'atenciones' ? <AttentionsReport organizacionId={organizationId} sedes={sites} profesionales={professionals} servicios={services} /> : null}
    </> : null}
  </section>
}
