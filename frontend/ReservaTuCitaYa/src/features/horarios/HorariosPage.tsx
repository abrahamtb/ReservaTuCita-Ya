import { useCallback, useEffect, useMemo, useState, type FormEvent } from 'react'
import { listarProfesionales, type EmpleadoLista } from '../../api/empleadosApi'
import * as horarios from '../../api/horariosApi'
import { listOrganizations } from '../../api/organizacionesApi'
import { listarRecursos, type RecursoLista } from '../../api/recursosApi'
import { listSedes } from '../../api/sedesApi'
import { useAuth } from '../../auth/useAuth'
import type { Organization, Sede } from '../../types'

type TipoEntidad = 'Sede' | 'Profesional' | 'Recurso'
const dias: horarios.DiaSemana[] = ['Lunes', 'Martes', 'Miercoles', 'Jueves', 'Viernes', 'Sabado', 'Domingo']
const diaLabel: Record<horarios.DiaSemana, string> = { Lunes: 'Lun', Martes: 'Mar', Miercoles: 'Mié', Jueves: 'Jue', Viernes: 'Vie', Sabado: 'Sáb', Domingo: 'Dom' }

function exceptionInitial(): horarios.ExcepcionRequest {
  return { fecha: new Date().toISOString().slice(0, 10), tipoExcepcion: 'CerradoTodoElDia', motivo: '', observaciones: '' }
}

export function HorariosPage() {
  const { user } = useAuth()
  const superAdmin = user?.roles.includes('Superadministrador') ?? false
  const [organizaciones, setOrganizaciones] = useState<Organization[]>([])
  const [organizacionId, setOrganizacionId] = useState(user?.organizacion?.id ?? '')
  const [sedes, setSedes] = useState<Sede[]>([])
  const [sedeId, setSedeId] = useState('')
  const [tipo, setTipo] = useState<TipoEntidad>('Profesional')
  const [profesionales, setProfesionales] = useState<EmpleadoLista[]>([])
  const [profesionalId, setProfesionalId] = useState('')
  const [recursos, setRecursos] = useState<RecursoLista[]>([])
  const [recursoId, setRecursoId] = useState('')
  const [intervalos, setIntervalos] = useState<horarios.IntervaloHorario[]>([])
  const [excepciones, setExcepciones] = useState<horarios.ExcepcionHorario[]>([])
  const [excepcion, setExcepcion] = useState<horarios.ExcepcionRequest>(exceptionInitial)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  useEffect(() => {
    if (user?.organizacion) {
      setOrganizaciones([{ id: user.organizacion.id, nombreComercial: user.organizacion.nombre, tipoOrganizacion: '', numeroDocumento: '', estaActivo: true }])
      setOrganizacionId(user.organizacion.id)
      return
    }
    if (!superAdmin) return
    const controller = new AbortController()
    listOrganizations({ estado: 'Activos', pagina: 1, tamanoPagina: 100 }, controller.signal).then(result => {
      setOrganizaciones(result.elementos)
      setOrganizacionId(current => current || result.elementos[0]?.id || '')
    }).catch(caught => { if (!controller.signal.aborted) setError(caught instanceof Error ? caught.message : 'No se pudieron cargar organizaciones.') })
    return () => controller.abort()
  }, [superAdmin, user?.organizacion])

  useEffect(() => {
    if (!organizacionId) { setSedes([]); setProfesionales([]); setSedeId(''); return }
    const controller = new AbortController()
    Promise.all([
      listSedes(organizacionId, { estado: 'Activos' }, controller.signal),
      listarProfesionales(organizacionId, controller.signal),
    ]).then(([siteItems, professionalPage]) => {
      setSedes(siteItems)
      setSedeId(current => siteItems.some(item => item.id === current) ? current : siteItems[0]?.id ?? '')
      setProfesionales(professionalPage.elementos)
      setProfesionalId(current => professionalPage.elementos.some(item => item.id === current) ? current : professionalPage.elementos[0]?.id ?? '')
    }).catch(caught => { if (!controller.signal.aborted) setError(caught instanceof Error ? caught.message : 'No se pudieron cargar las opciones de horarios.') })
    return () => controller.abort()
  }, [organizacionId])

  useEffect(() => {
    if (!sedeId) { setRecursos([]); setRecursoId(''); return }
    const controller = new AbortController()
    listarRecursos(sedeId, { estado: 'Activos', pagina: 1, tamanoPagina: 100 }, controller.signal).then(result => {
      setRecursos(result.elementos)
      setRecursoId(current => result.elementos.some(item => item.id === current) ? current : result.elementos[0]?.id ?? '')
    }).catch(caught => { if (!controller.signal.aborted) setError(caught instanceof Error ? caught.message : 'No se pudieron cargar los recursos.') })
    return () => controller.abort()
  }, [sedeId])

  const entidadId = tipo === 'Sede' ? sedeId : tipo === 'Profesional' ? profesionalId : recursoId
  const canLoad = Boolean(entidadId && (tipo !== 'Profesional' || sedeId))

  const loadSchedule = useCallback(async () => {
    if (!canLoad) { setIntervalos([]); setExcepciones([]); setLoading(false); return }
    setLoading(true); setError(''); setSuccess('')
    try {
      if (tipo === 'Sede') {
        const [schedule, exceptionPage] = await Promise.all([horarios.obtenerHorarioSede(sedeId), horarios.listarExcepcionesSede(sedeId, 1)])
        setIntervalos(schedule.intervalos); setExcepciones(exceptionPage.elementos)
      } else if (tipo === 'Profesional') {
        const [schedule, exceptionPage] = await Promise.all([horarios.obtenerHorarioProfesional(profesionalId, sedeId), horarios.listarExcepcionesProfesional(profesionalId)])
        setIntervalos(schedule.intervalos); setExcepciones(exceptionPage.elementos)
      } else {
        const [schedule, exceptionPage] = await Promise.all([horarios.obtenerHorarioRecurso(recursoId), horarios.listarExcepcionesRecurso(recursoId)])
        setIntervalos(schedule.intervalos); setExcepciones(exceptionPage.elementos)
      }
    } catch (caught) { setError(caught instanceof Error ? caught.message : 'No se pudo cargar la configuración de horarios.') }
    finally { setLoading(false) }
  }, [canLoad, profesionalId, recursoId, sedeId, tipo])

  useEffect(() => { void loadSchedule() }, [loadSchedule])

  const entityName = useMemo(() => {
    if (tipo === 'Sede') return sedes.find(item => item.id === sedeId)?.nombre ?? ''
    if (tipo === 'Profesional') return profesionales.find(item => item.id === profesionalId)?.nombreCompleto ?? ''
    return recursos.find(item => item.id === recursoId)?.nombre ?? ''
  }, [profesionalId, profesionales, recursoId, recursos, sedeId, sedes, tipo])

  function addInterval(diaSemana: horarios.DiaSemana) {
    setIntervalos(current => [...current, { diaSemana, horaInicio: '09:00', horaFin: '18:00' }])
  }
  function updateInterval(index: number, field: 'horaInicio' | 'horaFin', value: string) {
    setIntervalos(current => current.map((item, i) => i === index ? { ...item, [field]: value } : item))
  }

  async function saveSchedule() {
    if (!canLoad) return
    setSaving(true); setError(''); setSuccess('')
    try {
      if (tipo === 'Sede') await horarios.actualizarHorarioSede(sedeId, intervalos)
      else if (tipo === 'Profesional') await horarios.actualizarHorarioProfesional(profesionalId, sedeId, intervalos)
      else await horarios.actualizarHorarioRecurso(recursoId, intervalos)
      setSuccess('Horario semanal guardado correctamente.')
      await loadSchedule()
    } catch (caught) { setError(caught instanceof Error ? caught.message : 'No se pudo guardar el horario.') }
    finally { setSaving(false) }
  }

  async function createException(event: FormEvent) {
    event.preventDefault(); if (!canLoad) return
    setSaving(true); setError(''); setSuccess('')
    const request = {
      ...excepcion,
      horaInicio: excepcion.tipoExcepcion === 'CerradoTodoElDia' ? null : excepcion.horaInicio,
      horaFin: excepcion.tipoExcepcion === 'CerradoTodoElDia' ? null : excepcion.horaFin,
    }
    try {
      if (tipo === 'Sede') await horarios.crearExcepcionSede(sedeId, request)
      else if (tipo === 'Profesional') await horarios.crearExcepcionProfesional(profesionalId, sedeId, request)
      else await horarios.crearExcepcionRecurso(recursoId, request)
      setExcepcion(exceptionInitial()); setSuccess('Excepción agregada correctamente.'); await loadSchedule()
    } catch (caught) { setError(caught instanceof Error ? caught.message : 'No se pudo crear la excepción.') }
    finally { setSaving(false) }
  }

  async function deleteException(id: string) {
    if (!confirm('¿Eliminar esta excepción?')) return
    setError(''); setSuccess('')
    try {
      if (tipo === 'Sede') await horarios.eliminarExcepcionSede(id)
      else if (tipo === 'Profesional') await horarios.eliminarExcepcionProfesional(id)
      else await horarios.eliminarExcepcionRecurso(id)
      setSuccess('Excepción eliminada.'); await loadSchedule()
    } catch (caught) { setError(caught instanceof Error ? caught.message : 'No se pudo eliminar la excepción.') }
  }

  return <section>
    <div className="mb-4"><h1>Configuración de horarios</h1><p className="text-secondary">Configura horarios recurrentes para sede, profesional o recurso.</p></div>
    {error && <div className="alert alert-danger">{error}</div>}{success && <div className="alert alert-success">{success}</div>}
    <div className="card card-body mb-3"><div className="row g-3 align-items-end">
      {superAdmin && <div className="col-lg-3"><label className="form-label">Organización</label><select className="form-select" value={organizacionId} onChange={e => setOrganizacionId(e.target.value)}>{organizaciones.map(item => <option key={item.id} value={item.id}>{item.nombreComercial}</option>)}</select></div>}
      <div className="col-lg-2"><label className="form-label">Tipo</label><select className="form-select" value={tipo} onChange={e => setTipo(e.target.value as TipoEntidad)}><option>Sede</option><option>Profesional</option><option>Recurso</option></select></div>
      <div className="col-lg-3"><label className="form-label">Sede</label><select className="form-select" value={sedeId} onChange={e => setSedeId(e.target.value)}><option value="">Selecciona una sede</option>{sedes.map(item => <option key={item.id} value={item.id}>{item.nombre}</option>)}</select></div>
      {tipo === 'Profesional' && <div className="col-lg-4"><label className="form-label">Profesional</label><select className="form-select" value={profesionalId} onChange={e => setProfesionalId(e.target.value)}><option value="">Selecciona un profesional</option>{profesionales.map(item => <option key={item.id} value={item.id}>{item.nombreCompleto}</option>)}</select></div>}
      {tipo === 'Recurso' && <div className="col-lg-4"><label className="form-label">Recurso</label><select className="form-select" value={recursoId} onChange={e => setRecursoId(e.target.value)}><option value="">Selecciona un recurso</option>{recursos.map(item => <option key={item.id} value={item.id}>{item.nombre}</option>)}</select></div>}
    </div></div>

    {!canLoad ? <div className="alert alert-info">Selecciona la entidad que deseas configurar.</div> : loading ? <div className="py-5 text-center">Cargando horario…</div> : <>
      <div className="card mb-4"><div className="card-header d-flex justify-content-between align-items-center"><div><strong>Horario semanal recurrente</strong><div className="small text-secondary">{entityName} · Se repite cada semana hasta que sea modificado.</div></div><button className="btn btn-primary" disabled={saving} onClick={() => void saveSchedule()}>{saving ? 'Guardando…' : 'Guardar cambios'}</button></div>
        <div className="card-body">{dias.map(dia => { const row = intervalos.map((item, index) => ({ item, index })).filter(value => value.item.diaSemana === dia); return <div className="row g-2 align-items-center border-bottom py-2" key={dia}>
          <div className="col-md-2 fw-semibold">{diaLabel[dia]}</div><div className="col-md-8">{row.length === 0 ? <span className="text-secondary">Cerrado</span> : row.map(({ item, index }) => <div className="d-flex gap-2 mb-1" key={`${dia}-${index}`}><input type="time" className="form-control" value={item.horaInicio.slice(0, 5)} onChange={e => updateInterval(index, 'horaInicio', e.target.value)} /><span className="align-self-center">–</span><input type="time" className="form-control" value={item.horaFin.slice(0, 5)} onChange={e => updateInterval(index, 'horaFin', e.target.value)} /><button type="button" className="btn btn-outline-danger" aria-label="Eliminar intervalo" onClick={() => setIntervalos(current => current.filter((_, i) => i !== index))}>×</button></div>)}</div><div className="col-md-2 text-end"><button type="button" className="btn btn-sm btn-outline-primary" onClick={() => addInterval(dia)}>+ Agregar intervalo</button></div>
        </div>})}</div>
      </div>

      <div className="row g-4"><div className="col-lg-5"><form className="card card-body" onSubmit={createException}><h2 className="h5">Nueva excepción</h2>
        <label className="form-label">Fecha<input required type="date" className="form-control" value={excepcion.fecha} onChange={e => setExcepcion({ ...excepcion, fecha: e.target.value })} /></label>
        <label className="form-label">Tipo<select className="form-select" value={excepcion.tipoExcepcion} onChange={e => setExcepcion({ ...excepcion, tipoExcepcion: e.target.value as horarios.TipoExcepcionHorario })}><option value="CerradoTodoElDia">Cerrado todo el día</option><option value="HorarioEspecial">Horario especial</option><option value="NoDisponibleParcial">No disponible parcial</option></select></label>
        {excepcion.tipoExcepcion !== 'CerradoTodoElDia' && <div className="row"><div className="col"><label className="form-label">Inicio<input required type="time" className="form-control" value={excepcion.horaInicio ?? ''} onChange={e => setExcepcion({ ...excepcion, horaInicio: e.target.value })} /></label></div><div className="col"><label className="form-label">Fin<input required type="time" className="form-control" value={excepcion.horaFin ?? ''} onChange={e => setExcepcion({ ...excepcion, horaFin: e.target.value })} /></label></div></div>}
        <label className="form-label">Motivo<input required className="form-control" value={excepcion.motivo} onChange={e => setExcepcion({ ...excepcion, motivo: e.target.value })} /></label><label className="form-label">Observaciones<textarea className="form-control" value={excepcion.observaciones ?? ''} onChange={e => setExcepcion({ ...excepcion, observaciones: e.target.value })} /></label><button className="btn btn-primary" disabled={saving}>Agregar excepción</button>
      </form></div><div className="col-lg-7"><div className="card"><div className="card-header fw-semibold">Excepciones de horario</div><div className="table-responsive"><table className="table align-middle mb-0"><thead><tr><th>Fecha</th><th>Tipo</th><th>Horario</th><th>Motivo</th><th /></tr></thead><tbody>{excepciones.map(item => <tr key={item.id}><td>{item.fecha}</td><td>{item.tipoExcepcion}</td><td>{item.horaInicio ? `${item.horaInicio.slice(0, 5)}–${item.horaFin?.slice(0, 5)}` : 'Todo el día'}</td><td>{item.motivo}</td><td><button className="btn btn-sm btn-outline-danger" onClick={() => void deleteException(item.id)}>Eliminar</button></td></tr>)}{excepciones.length === 0 && <tr><td colSpan={5} className="text-center text-secondary py-4">No hay excepciones registradas.</td></tr>}</tbody></table></div></div></div></div>
    </>}
  </section>
}
