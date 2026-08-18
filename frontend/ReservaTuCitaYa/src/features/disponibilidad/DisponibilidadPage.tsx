import { useState, type FormEvent } from 'react'
import { consultarDisponibilidad, type DisponibilidadRespuesta } from '../../api/disponibilidadApi'
import { ErrorAlert, Loading } from '../../components/common/Feedback'

const today = new Date().toISOString().slice(0, 10)

export function DisponibilidadPage() {
  const [sedeId, setSedeId] = useState('')
  const [servicioId, setServicioId] = useState('')
  const [fechaDesde, setFechaDesde] = useState(today)
  const [fechaHasta, setFechaHasta] = useState(today)
  const [resultado, setResultado] = useState<DisponibilidadRespuesta>()
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<unknown>()

  async function submit(event: FormEvent) {
    event.preventDefault()
    setLoading(true)
    setError(undefined)
    try {
      setResultado(await consultarDisponibilidad({ sedeId, servicioId, fechaDesde, fechaHasta }))
    } catch (caught) {
      setError(caught)
    } finally {
      setLoading(false)
    }
  }

  return <section>
    <h1>Disponibilidad</h1>
    <p className="text-secondary">Consulta horarios libres por sede, servicio y rango de fechas.</p>
    {error ? <ErrorAlert error={error} /> : null}
    <form className="card card-body mb-4" onSubmit={submit}>
      <div className="row g-3">
        <div className="col-md-3"><label className="form-label">ID de sede</label><input required className="form-control" value={sedeId} onChange={e => setSedeId(e.target.value)} /></div>
        <div className="col-md-3"><label className="form-label">ID de servicio</label><input required className="form-control" value={servicioId} onChange={e => setServicioId(e.target.value)} /></div>
        <div className="col-md-3"><label className="form-label">Desde</label><input required type="date" className="form-control" value={fechaDesde} onChange={e => setFechaDesde(e.target.value)} /></div>
        <div className="col-md-3"><label className="form-label">Hasta</label><input required type="date" className="form-control" min={fechaDesde} value={fechaHasta} onChange={e => setFechaHasta(e.target.value)} /></div>
      </div>
      <button disabled={loading} className="btn btn-primary mt-3 align-self-start">Consultar</button>
    </form>
    {loading ? <Loading /> : null}
    {resultado ? <div className="row g-3">{resultado.dias.map(dia => <div className="col-lg-6" key={dia.fecha}><div className="card h-100"><div className="card-header"><strong>{dia.fecha}</strong></div><div className="card-body">{dia.horarios.length === 0 ? <span className="text-secondary">Sin horarios disponibles.</span> : <div className="d-flex flex-wrap gap-2">{dia.horarios.map((slot, index) => <span className="badge text-bg-primary p-2" key={`${slot.horaInicio}-${index}`}>{slot.horaInicio.slice(0, 5)} - {slot.horaFinServicio.slice(0, 5)}{slot.profesionalNombre ? ` · ${slot.profesionalNombre}` : ''}</span>)}</div>}</div></div></div>)}</div> : null}
  </section>
}
