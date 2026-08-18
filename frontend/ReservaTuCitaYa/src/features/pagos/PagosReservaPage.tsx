import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { Link, useParams } from 'react-router-dom'
import * as api from '../../api/pagosApi'
import { ErrorAlert, Loading } from '../../components/common/Feedback'

const hoy = new Date().toISOString().slice(0, 10)
const money = new Intl.NumberFormat('es-PE', { style: 'currency', currency: 'PEN' })

export function PagosReservaPage() {
  const { reservaId = '' } = useParams()
  const [resumen, setResumen] = useState<api.ResumenPago>()
  const [metodos, setMetodos] = useState<api.MetodoPago[]>([])
  const [error, setError] = useState<unknown>()
  const [busy, setBusy] = useState(false)
  const [pago, setPago] = useState<api.PagoRequest>({ metodoPagoId: '', monto: 0, fechaPago: hoy })
  const [reembolsoPago, setReembolsoPago] = useState<api.Pago | null>(null)
  const [reembolso, setReembolso] = useState<api.ReembolsoRequest>({ metodoPagoId: '', monto: 0, fechaReembolso: hoy, motivo: '' })

  const load = useCallback(async () => {
    setError(undefined)
    try {
      const [summary, methods] = await Promise.all([api.obtenerResumenPago(reservaId), api.listarMetodosPago()])
      setResumen(summary); setMetodos(methods)
      setPago(current => ({ ...current, metodoPagoId: current.metodoPagoId || methods[0]?.id || '' }))
      setReembolso(current => ({ ...current, metodoPagoId: current.metodoPagoId || methods[0]?.id || '' }))
    } catch (caught) { setError(caught) }
  }, [reservaId])
  useEffect(() => { void load() }, [load])

  async function submitPago(event: FormEvent) {
    event.preventDefault(); if (!reservaId || pago.monto <= 0) return
    setBusy(true); setError(undefined)
    try { await api.registrarPago(reservaId, pago); setPago(current => ({ ...current, monto: 0, numeroOperacion: '', observacion: '' })); await load() }
    catch (caught) { setError(caught) } finally { setBusy(false) }
  }
  async function annul(item: api.Pago) {
    const motivo = prompt('Motivo de anulación:')?.trim(); if (!motivo) return
    setBusy(true); try { await api.anularPago(item.id, motivo); await load() } catch (caught) { setError(caught) } finally { setBusy(false) }
  }
  async function submitRefund(event: FormEvent) {
    event.preventDefault(); if (!reembolsoPago || reembolso.monto <= 0 || !reembolso.motivo.trim()) return
    setBusy(true); setError(undefined)
    try { await api.registrarReembolso(reservaId, reembolso); setReembolsoPago(null); setReembolso(current => ({ ...current, monto: 0, numeroOperacion: '', motivo: '', observacion: '' })); await load() }
    catch (caught) { setError(caught) } finally { setBusy(false) }
  }

  if (error && !resumen) return <ErrorAlert error={error} />
  if (!resumen) return <Loading />
  return <section>
    <div className="d-flex justify-content-between align-items-start mb-3"><div><h1>Pagos de reserva</h1><p className="text-secondary mb-0">Reserva {resumen.codigoReserva}</p></div><Link className="btn btn-outline-secondary" to={`/reservas/${reservaId}`}>Volver</Link></div>
    {error ? <ErrorAlert error={error} /> : null}
    <div className="row g-3 mb-4">
      {[['Precio total', resumen.precioTotal], ['Pagado neto', resumen.totalPagadoNeto], ['Reembolsado', resumen.totalReembolsado], ['Saldo pendiente', resumen.saldoPendiente]].map(([label, value]) => <div className="col-md-3" key={String(label)}><div className="card card-body h-100"><small className="text-secondary">{label}</small><strong className="fs-4">{money.format(Number(value))}</strong></div></div>)}
    </div>
    <div className="row g-4">
      <div className="col-lg-8"><div className="card"><div className="card-header fw-semibold">Historial</div><div className="table-responsive"><table className="table mb-0 align-middle"><thead><tr><th>Código</th><th>Fecha</th><th>Método</th><th>Monto</th><th>Estado</th><th /></tr></thead><tbody>{resumen.pagos.map(item => <tr key={item.id}><td>{item.codigo}</td><td>{item.fechaPago}</td><td>{item.metodoPago}</td><td>{money.format(item.monto)}</td><td>{item.estaAnulado ? <span className="badge text-bg-secondary">Anulado</span> : <span className="badge text-bg-success">Vigente</span>}</td><td className="text-end">{!item.estaAnulado && <><button disabled={busy} className="btn btn-sm btn-outline-warning me-1" onClick={() => void annul(item)}>Anular</button><button className="btn btn-sm btn-outline-danger" onClick={() => { setReembolsoPago(item); setReembolso(current => ({ ...current, monto: Math.min(item.monto, resumen.totalPagadoNeto) })) }}>Reembolsar</button></>}</td></tr>)}</tbody></table></div></div>
        {resumen.reembolsos.length > 0 && <div className="card mt-3"><div className="card-header fw-semibold">Reembolsos</div><div className="table-responsive"><table className="table mb-0"><thead><tr><th>Código</th><th>Fecha</th><th>Método</th><th>Monto</th><th>Motivo</th></tr></thead><tbody>{resumen.reembolsos.map(item => <tr key={item.id}><td>{item.codigo}</td><td>{item.fechaReembolso}</td><td>{item.metodoPago ?? '—'}</td><td>{money.format(item.monto)}</td><td>{item.motivo}</td></tr>)}</tbody></table></div></div>}
      </div>
      <div className="col-lg-4"><form className="card card-body" onSubmit={submitPago}><h2 className="h5">Registrar pago</h2><label className="form-label">Método<select required className="form-select" value={pago.metodoPagoId} onChange={e => setPago({ ...pago, metodoPagoId: e.target.value })}>{metodos.map(m => <option key={m.id} value={m.id}>{m.nombre}</option>)}</select></label><label className="form-label">Monto<input required min="0.01" step="0.01" type="number" className="form-control" value={pago.monto || ''} onChange={e => setPago({ ...pago, monto: Number(e.target.value) })} /></label><label className="form-label">Fecha<input required type="date" className="form-control" value={pago.fechaPago} onChange={e => setPago({ ...pago, fechaPago: e.target.value })} /></label><label className="form-label">N.º operación<input className="form-control" value={pago.numeroOperacion ?? ''} onChange={e => setPago({ ...pago, numeroOperacion: e.target.value })} /></label><label className="form-label">Observación<textarea className="form-control" value={pago.observacion ?? ''} onChange={e => setPago({ ...pago, observacion: e.target.value })} /></label><button disabled={busy || !pago.metodoPagoId} className="btn btn-primary">{busy ? 'Guardando…' : 'Registrar pago'}</button></form></div>
    </div>
    {reembolsoPago && <div className="modal d-block" tabIndex={-1} style={{ background: '#0008' }}><div className="modal-dialog"><form className="modal-content" onSubmit={submitRefund}><div className="modal-header"><h2 className="modal-title fs-5">Registrar reembolso</h2><button type="button" className="btn-close" onClick={() => setReembolsoPago(null)} /></div><div className="modal-body"><p>Pago: <strong>{reembolsoPago.codigo}</strong></p><label className="form-label w-100">Método<select required className="form-select" value={reembolso.metodoPagoId} onChange={e => setReembolso({ ...reembolso, metodoPagoId: e.target.value })}>{metodos.map(m => <option key={m.id} value={m.id}>{m.nombre}</option>)}</select></label><label className="form-label w-100">Monto<input required min="0.01" step="0.01" type="number" className="form-control" value={reembolso.monto || ''} onChange={e => setReembolso({ ...reembolso, monto: Number(e.target.value) })} /></label><label className="form-label w-100">Fecha<input required type="date" className="form-control" value={reembolso.fechaReembolso} onChange={e => setReembolso({ ...reembolso, fechaReembolso: e.target.value })} /></label><label className="form-label w-100">Motivo<input required className="form-control" value={reembolso.motivo} onChange={e => setReembolso({ ...reembolso, motivo: e.target.value })} /></label></div><div className="modal-footer"><button type="button" className="btn btn-outline-secondary" onClick={() => setReembolsoPago(null)}>Cancelar</button><button disabled={busy} className="btn btn-danger">Registrar reembolso</button></div></form></div></div>}
  </section>
}
