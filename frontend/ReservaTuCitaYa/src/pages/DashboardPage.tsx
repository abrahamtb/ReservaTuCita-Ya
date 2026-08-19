import { useEffect, useMemo, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { ApiError } from '../api/apiClient'
import { obtenerDashboardResumen } from '../api/dashboardApi'
import { listOrganizations } from '../api/organizacionesApi'
import { listSedes } from '../api/sedesApi'
import { useAuth } from '../auth/useAuth'
import { ReservationStatus } from '../features/atenciones/AttentionShared'
import type { DashboardFiltros, DashboardResumen, IndicadorComparativo, Organization, Sede } from '../types'
import './dashboard.css'

type Periodo = 'hoy' | '7dias' | '30dias' | 'personalizado'

function fechaLocal(date: Date) {
  const offset = date.getTimezoneOffset() * 60_000
  return new Date(date.getTime() - offset).toISOString().slice(0, 10)
}

function rango(periodo: Exclude<Periodo, 'personalizado'>): DashboardFiltros {
  const hasta = new Date()
  const desde = new Date(hasta)
  if (periodo === '7dias') desde.setDate(desde.getDate() - 6)
  if (periodo === '30dias') desde.setDate(desde.getDate() - 29)
  return { fechaDesde: fechaLocal(desde), fechaHasta: fechaLocal(hasta) }
}

const numero = new Intl.NumberFormat('es-PE', { maximumFractionDigits: 2 })
const moneda = new Intl.NumberFormat('es-PE', { style: 'currency', currency: 'PEN' })
const fechaCorta = new Intl.DateTimeFormat('es-PE', { day: '2-digit', month: 'short' })
const hora = new Intl.DateTimeFormat('es-PE', { hour: '2-digit', minute: '2-digit' })
const parseFecha = (value: string) => new Date(`${value.slice(0, 10)}T12:00:00`)

function Kpi({ titulo, dato, dinero = false, ayuda }: { titulo: string; dato: IndicadorComparativo; dinero?: boolean; ayuda?: string }) {
  const variacion = dato.variacionPorcentaje
  const tendencia = variacion && variacion > 0 ? 'positive' : variacion && variacion < 0 ? 'negative' : 'neutral'
  return <article className="dashboard-card dashboard-kpi">
    <div className="dashboard-kpi__heading"><span>{titulo}</span>{ayuda && <span title={ayuda}>ⓘ</span>}</div>
    <strong>{dinero ? moneda.format(dato.valorActual) : numero.format(dato.valorActual)}</strong>
    {dato.sinBaseComparacion
      ? <small>Sin comparación anterior</small>
      : <small className={`dashboard-variation--${tendencia}`}>{variacion == null ? '—' : `${variacion > 0 ? '+' : ''}${numero.format(variacion)} %`} respecto al periodo anterior</small>}
  </article>
}

function Cargando() {
  return <div aria-label="Cargando dashboard" aria-busy="true">
    <div className="dashboard-kpis">{Array.from({ length: 6 }, (_, i) => <div className="dashboard-skeleton dashboard-skeleton--kpi" key={i} />)}</div>
    <div className="dashboard-grid"><div className="dashboard-skeleton dashboard-skeleton--chart" /><div className="dashboard-skeleton dashboard-skeleton--chart" /></div>
  </div>
}

function Vacio({ children = 'No hay información suficiente para este periodo.' }: { children?: string }) {
  return <div className="dashboard-empty">{children}</div>
}

function ReservasDia({ datos }: { datos: DashboardResumen['reservasPorDia'] }) {
  if (!datos.length) return <Vacio />
  const maximo = Math.max(...datos.map(item => item.cantidad), 1)
  return <div className="dashboard-bars" role="img" aria-label="Reservas por día">
    {datos.map(item => <div className="dashboard-bar-item" key={item.fecha} title={`${item.fecha}: ${item.cantidad} reservas`}>
      <b>{item.cantidad}</b><span style={{ height: `${Math.max(item.cantidad / maximo * 145, item.cantidad ? 5 : 1)}px` }} /><small>{fechaCorta.format(parseFecha(item.fecha))}</small>
    </div>)}
  </div>
}

function IngresosDia({ datos }: { datos: DashboardResumen['ingresosPorDia'] }) {
  if (!datos.length) return <Vacio />
  return <div className="dashboard-income-list">
    {datos.map(item => <div className="dashboard-income-row" key={item.fecha} title={`Bruto: ${moneda.format(item.ingresosBrutos)}; reembolsos: ${moneda.format(item.reembolsos)}; neto: ${moneda.format(item.ingresosNetos)}`}>
      <strong>{fechaCorta.format(parseFecha(item.fecha))}</strong>
      <span><i className="income-dot" />Bruto {moneda.format(item.ingresosBrutos)}</span>
      <span><i className="income-dot income-dot--refund" />Reembolsos {moneda.format(item.reembolsos)}</span>
      <span className={item.ingresosNetos < 0 ? 'text-danger' : ''}><i className="income-dot income-dot--net" />Neto {moneda.format(item.ingresosNetos)}</span>
    </div>)}
  </div>
}

export function DashboardPage() {
  const { user } = useAuth()
  const navigate = useNavigate()
  const inicial = useMemo(() => rango('7dias'), [])
  const [organizaciones, setOrganizaciones] = useState<Organization[]>([])
  const [organizacionId, setOrganizacionId] = useState(user?.organizacion?.id ?? '')
  const [cargandoOrganizaciones, setCargandoOrganizaciones] = useState(!user?.organizacion)
  const [periodo, setPeriodo] = useState<Periodo>('7dias')
  const [desde, setDesde] = useState(inicial.fechaDesde)
  const [hasta, setHasta] = useState(inicial.fechaHasta)
  const [sedeId, setSedeId] = useState('')
  const [sedes, setSedes] = useState<Sede[]>([])
  const [filtros, setFiltros] = useState<DashboardFiltros>({
    ...inicial,
    organizacionId: user?.organizacion?.id,
  })
  const [datos, setDatos] = useState<DashboardResumen | null>(null)
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState(false)
  const [reintento, setReintento] = useState(0)

  useEffect(() => {
    if (user?.organizacion) {
      setOrganizaciones([{
        id: user.organizacion.id,
        nombreComercial: user.organizacion.nombre,
        tipoOrganizacion: '',
        numeroDocumento: '',
        estaActivo: true,
      }])
      setOrganizacionId(user.organizacion.id)
      setCargandoOrganizaciones(false)
      return
    }

    const controller = new AbortController()
    setCargandoOrganizaciones(true)
    listOrganizations(
      { estado: 'Activos', pagina: 1, tamanoPagina: 100 },
      controller.signal,
    ).then(result => {
      const primeraOrganizacion = result.elementos[0]?.id ?? ''
      setOrganizaciones(result.elementos)
      setOrganizacionId(primeraOrganizacion)
      setFiltros(actuales => ({
        ...actuales,
        organizacionId: primeraOrganizacion || undefined,
        sedeId: undefined,
      }))
    }).catch(caught => {
      if (controller.signal.aborted) return
      if (caught instanceof ApiError && caught.status === 403) {
        navigate('/acceso-denegado', { replace: true })
        return
      }
      setOrganizaciones([])
    }).finally(() => {
      if (!controller.signal.aborted) setCargandoOrganizaciones(false)
    })
    return () => controller.abort()
  }, [navigate, user?.organizacion])

  useEffect(() => {
    if (!organizacionId) {
      setSedes([])
      return
    }
    const controller = new AbortController()
    listSedes(organizacionId, { estado: 'Activos' }, controller.signal).then(setSedes).catch(() => setSedes([]))
    return () => controller.abort()
  }, [organizacionId])

  useEffect(() => {
    if (!filtros.organizacionId) {
      setDatos(null)
      setCargando(false)
      setError(false)
      return
    }
    const controller = new AbortController()
    setCargando(true)
    setError(false)
    obtenerDashboardResumen(filtros, controller.signal).then(setDatos).catch(caught => {
      if (controller.signal.aborted) return
      if (caught instanceof ApiError && caught.status === 403) {
        navigate('/acceso-denegado', { replace: true })
        return
      }
      setError(true)
    }).finally(() => {
      if (!controller.signal.aborted) setCargando(false)
    })
    return () => controller.abort()
  }, [filtros, navigate, reintento])

  function cambiarPeriodo(value: Periodo) {
    setPeriodo(value)
    if (value !== 'personalizado') {
      const fechas = rango(value)
      setDesde(fechas.fechaDesde)
      setHasta(fechas.fechaHasta)
    }
  }

  function cambiarOrganizacion(value: string) {
    setOrganizacionId(value)
    setSedeId('')
    setDatos(null)
    setFiltros(actuales => ({
      ...actuales,
      organizacionId: value || undefined,
      sedeId: undefined,
    }))
  }

  const sinDatos = datos
    && datos.reservasPorDia.every(item => item.cantidad === 0)
    && datos.reservasPorEstado.length === 0
    && datos.ingresosPorDia.every(item => item.ingresosBrutos === 0 && item.reembolsos === 0 && item.ingresosNetos === 0)
    && datos.topServicios.length === 0
    && datos.proximasReservas.length === 0

  return <section className="dashboard-page">
    <header className="dashboard-header"><div><h1>Dashboard</h1><p>Resumen general de la operación.</p></div>{datos && <small>Última actualización: {hora.format(new Date(datos.fechaHoraConsulta))}</small>}</header>
    <form className="dashboard-filters" onSubmit={event => { event.preventDefault(); setFiltros({ fechaDesde: desde, fechaHasta: hasta, sedeId: sedeId || undefined, organizacionId: organizacionId || undefined }) }}>
      <label>Organización<select className="form-select" value={organizacionId} disabled={Boolean(user?.organizacion) || cargandoOrganizaciones} onChange={event => cambiarOrganizacion(event.target.value)} required><option value="">Selecciona una organización</option>{organizaciones.map(organizacion => <option key={organizacion.id} value={organizacion.id}>{organizacion.nombreComercial}</option>)}</select></label>
      <label>Periodo<select className="form-select" value={periodo} onChange={event => cambiarPeriodo(event.target.value as Periodo)}><option value="hoy">Hoy</option><option value="7dias">Últimos 7 días</option><option value="30dias">Últimos 30 días</option><option value="personalizado">Personalizado</option></select></label>
      {periodo === 'personalizado' && <><label>Desde<input className="form-control" type="date" value={desde} max={hasta} onChange={event => setDesde(event.target.value)} required /></label><label>Hasta<input className="form-control" type="date" value={hasta} min={desde} onChange={event => setHasta(event.target.value)} required /></label></>}
      <label>Sede<select className="form-select" value={sedeId} onChange={event => setSedeId(event.target.value)}><option value="">Todas las sedes</option>{sedes.map(sede => <option key={sede.id} value={sede.id}>{sede.nombre}</option>)}</select></label>
      <button className="btn btn-primary" disabled={cargando || !organizacionId}>Actualizar</button>
    </form>

    {!cargandoOrganizaciones && !organizacionId && <div className="alert alert-info dashboard-error">
      <span>No hay organizaciones activas. Crea la primera para comenzar a usar el dashboard.</span>
      <Link className="btn btn-sm btn-primary" to="/organizaciones/nueva">Crear organización</Link>
    </div>}
    {cargando && <Cargando />}
    {!cargando && error && <div className="alert alert-danger dashboard-error"><strong>No se pudo cargar el dashboard.</strong><button className="btn btn-sm btn-outline-danger" onClick={() => setReintento(value => value + 1)}>Reintentar</button></div>}
    {!cargando && !error && datos && <>
      {sinDatos && <div className="alert alert-info">No hay información suficiente para este periodo.</div>}
      <div className="dashboard-kpis"><Kpi titulo="Reservas hoy" dato={datos.reservasHoy} /><Kpi titulo="Por atender hoy" dato={datos.porAtenderHoy} /><Kpi titulo="Atenciones completadas" dato={datos.atencionesCompletadas} /><Kpi titulo="Cancelaciones" dato={datos.cancelaciones} /><Kpi titulo="Clientes nuevos" dato={datos.clientesNuevos} ayuda={sedeId ? 'Clientes nuevos de la organización' : undefined} /><Kpi titulo="Ingresos netos" dato={datos.ingresosNetos} dinero /></div>
      <div className="dashboard-grid">
        <article className="dashboard-card dashboard-panel"><h2>Reservas por día</h2><ReservasDia datos={datos.reservasPorDia} /></article>
        <article className="dashboard-card dashboard-panel"><h2>Reservas por estado</h2>{datos.reservasPorEstado.length ? <div className="dashboard-states">{datos.reservasPorEstado.map(item => <div key={item.estado}><ReservationStatus status={item.estado} /><strong>{item.cantidad}</strong></div>)}</div> : <Vacio />}</article>
      </div>
      <div className="dashboard-grid">
        <article className="dashboard-card dashboard-panel"><h2>Ingresos por día</h2><IngresosDia datos={datos.ingresosPorDia} /></article>
        <article className="dashboard-card dashboard-panel"><h2>Top 5 servicios</h2>{datos.topServicios.length ? <ol className="dashboard-ranking">{datos.topServicios.map(item => <li key={item.servicioId}><span><strong>{item.nombre}</strong><small>{item.cantidadReservas} reservas</small></span><b>{numero.format(item.porcentajeSobreTotal)} %</b></li>)}</ol> : <Vacio>No hay reservas suficientes en este periodo.</Vacio>}</article>
      </div>
      <article className="dashboard-card dashboard-panel"><div className="dashboard-panel__header"><h2>Próximas reservas</h2><Link to="/atenciones/agenda">Ver agenda completa</Link></div>{datos.proximasReservas.length ? <div className="table-responsive"><table className="table align-middle dashboard-table"><thead><tr><th>Hora</th><th>Cliente</th><th>Servicio</th><th>Profesional</th><th>Estado</th><th /></tr></thead><tbody>{datos.proximasReservas.map(reserva => <tr key={reserva.reservaId}><td>{reserva.horaInicio.slice(0, 5)}</td><td>{reserva.cliente}</td><td>{reserva.servicio}</td><td>{reserva.profesional ?? 'Sin asignar'}</td><td><ReservationStatus status={reserva.estado} /></td><td><Link className="btn btn-sm btn-outline-primary" to={organizacionId ? `/organizaciones/${organizacionId}/reservas/${reserva.reservaId}/atencion` : '/atenciones/agenda'}>Ver</Link></td></tr>)}</tbody></table></div> : <Vacio />}</article>
    </>}
  </section>
}
