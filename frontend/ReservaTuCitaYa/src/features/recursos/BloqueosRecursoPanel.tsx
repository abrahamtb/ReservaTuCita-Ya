import { useCallback, useEffect, useState, type FormEvent } from 'react'
import {
  actualizarBloqueoRecurso,
  crearBloqueoRecurso,
  eliminarBloqueoRecurso,
  listarBloqueosRecurso,
  type BloqueoRecursoDto,
  type BloqueoRecursoRequest,
  type TipoBloqueo,
} from '../../api/bloqueosApi'

const initial = (): BloqueoRecursoRequest => ({
  fechaHoraInicio: '',
  fechaHoraFin: '',
  tipoBloqueo: 'Mantenimiento',
  motivo: '',
  observaciones: '',
})

const tipoLabel: Record<TipoBloqueo, string> = {
  Feriado: 'Feriado',
  Mantenimiento: 'Mantenimiento',
  Vacaciones: 'Vacaciones',
  Personal: 'Uso personal / interno',
}

const toInputDateTime = (value: string) => value ? value.slice(0, 16) : ''
const displayDateTime = (value: string) => new Intl.DateTimeFormat('es-PE', {
  day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit',
}).format(new Date(value))

export function BloqueosRecursoPanel({ recursoId }: { recursoId: string }) {
  const [items, setItems] = useState<BloqueoRecursoDto[]>([])
  const [form, setForm] = useState<BloqueoRecursoRequest>(initial)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [showForm, setShowForm] = useState(false)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  const load = useCallback(async () => {
    const result = await listarBloqueosRecurso(recursoId)
    setItems(result)
  }, [recursoId])

  useEffect(() => { void load().catch(caught => setError(caught instanceof Error ? caught.message : 'No se pudieron cargar los bloqueos.')) }, [load])

  function openNew() {
    setEditingId(null)
    setForm(initial())
    setError('')
    setShowForm(true)
  }

  function openEdit(item: BloqueoRecursoDto) {
    setEditingId(item.id)
    setForm({
      fechaHoraInicio: toInputDateTime(item.fechaHoraInicio),
      fechaHoraFin: toInputDateTime(item.fechaHoraFin),
      tipoBloqueo: item.tipoBloqueo,
      motivo: item.motivo,
      observaciones: item.observaciones ?? '',
    })
    setError('')
    setShowForm(true)
  }

  async function submit(event: FormEvent) {
    event.preventDefault()
    if (form.fechaHoraFin <= form.fechaHoraInicio) {
      setError('La fecha y hora de fin debe ser posterior al inicio.')
      return
    }
    setBusy(true); setError('')
    try {
      if (editingId) await actualizarBloqueoRecurso(editingId, form)
      else await crearBloqueoRecurso(recursoId, form)
      setShowForm(false); setEditingId(null); setForm(initial())
      await load()
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : 'No se pudo guardar el bloqueo.')
    } finally { setBusy(false) }
  }

  async function remove(id: string) {
    if (!confirm('¿Eliminar este bloqueo?')) return
    setError('')
    try { await eliminarBloqueoRecurso(id); await load() }
    catch (caught) { setError(caught instanceof Error ? caught.message : 'No se pudo eliminar el bloqueo.') }
  }

  return <div className="card mt-4">
    <div className="card-header d-flex justify-content-between align-items-center">
      <div><strong>Bloqueos</strong><div className="small text-secondary">Los bloqueos afectan la disponibilidad calculada. Los horarios adyacentes sí pueden reservarse.</div></div>
      <button type="button" className="btn btn-sm btn-primary" onClick={openNew}>+ Nuevo bloqueo</button>
    </div>
    {error && <div className="alert alert-danger m-3 mb-0">{error}</div>}
    {showForm && <form className="card-body border-bottom" onSubmit={submit}>
      <h3 className="h6">{editingId ? 'Editar bloqueo' : 'Nuevo bloqueo'}</h3>
      <div className="row g-3">
        <div className="col-md-4"><label className="form-label">Inicio *</label><input required type="datetime-local" className="form-control" value={form.fechaHoraInicio} onChange={e => setForm({ ...form, fechaHoraInicio: e.target.value })} /></div>
        <div className="col-md-4"><label className="form-label">Fin *</label><input required type="datetime-local" className="form-control" value={form.fechaHoraFin} onChange={e => setForm({ ...form, fechaHoraFin: e.target.value })} /></div>
        <div className="col-md-4"><label className="form-label">Tipo *</label><select className="form-select" value={form.tipoBloqueo} onChange={e => setForm({ ...form, tipoBloqueo: e.target.value as TipoBloqueo })}>{Object.entries(tipoLabel).map(([value, label]) => <option key={value} value={value}>{label}</option>)}</select></div>
        <div className="col-md-6"><label className="form-label">Motivo *</label><input required className="form-control" value={form.motivo} onChange={e => setForm({ ...form, motivo: e.target.value })} /></div>
        <div className="col-md-6"><label className="form-label">Observaciones</label><input className="form-control" value={form.observaciones ?? ''} onChange={e => setForm({ ...form, observaciones: e.target.value })} /></div>
      </div>
      <div className="d-flex gap-2 mt-3"><button className="btn btn-primary" disabled={busy}>{busy ? 'Guardando…' : 'Guardar'}</button><button type="button" className="btn btn-outline-secondary" onClick={() => setShowForm(false)}>Cancelar</button></div>
    </form>}
    <div className="table-responsive"><table className="table align-middle mb-0"><thead><tr><th>Inicio</th><th>Fin</th><th>Tipo</th><th>Motivo</th><th>Acciones</th></tr></thead><tbody>
      {items.map(item => <tr key={item.id}><td>{displayDateTime(item.fechaHoraInicio)}</td><td>{displayDateTime(item.fechaHoraFin)}</td><td>{tipoLabel[item.tipoBloqueo] ?? item.tipoBloqueo}</td><td>{item.motivo}</td><td><div className="d-flex gap-1"><button type="button" className="btn btn-sm btn-outline-primary" onClick={() => openEdit(item)}>Editar</button><button type="button" className="btn btn-sm btn-outline-danger" onClick={() => void remove(item.id)}>Eliminar</button></div></td></tr>)}
      {items.length === 0 && <tr><td colSpan={5} className="text-center text-secondary py-4">No hay bloqueos registrados.</td></tr>}
    </tbody></table></div>
  </div>
}
