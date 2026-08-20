import { useState, type FormEvent } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { ErrorAlert } from '../components/common/Feedback'
import { useAuth } from '../auth/useAuth'

export function LoginPage() {
  const { user, login } = useAuth()
  const navigate = useNavigate(); const location = useLocation()
  const [email, setEmail] = useState(''); const [password, setPassword] = useState('')
  const [remember, setRemember] = useState(false); const [error, setError] = useState<unknown>(); const [busy, setBusy] = useState(false)
  if (user) return <Navigate to="/" replace />
  async function submit(event: FormEvent) {
    event.preventDefault(); setBusy(true); setError(undefined)
    try { await login(email, password, remember); navigate((location.state as { from?: string } | null)?.from ?? '/', { replace: true }) }
    catch (caught) { setError(caught) } finally { setBusy(false) }
  }
  return <main className="login-hifi">
    <section className="login-showcase" aria-hidden="true"><div className="brand-mark"><span>R</span><strong>Reserva tu<br />Cita Ya</strong></div><div className="login-message"><p className="eyebrow">RESERVA TU CITA YA</p><h1>Tu agenda organizada.<br />Tus clientes más cerca.</h1><article className="next-appointment"><small>PRÓXIMA CITA</small><strong>María González</strong><span>Hoy · 10:30 · Tratamiento facial</span></article></div></section>
    <section className="login-form-wrap"><form className="login-card" onSubmit={submit}><div className="brand-mobile"><span>R</span> Reserva tu Cita Ya</div><h1>Bienvenido</h1><p>Ingresa a tu cuenta para continuar</p>{error ? <ErrorAlert error={error} /> : null}
      <label htmlFor="login-email" className="form-label">Correo</label><input id="login-email" className="form-control mb-3" type="email" required value={email} onChange={e => setEmail(e.target.value)} autoComplete="username" />
      <label htmlFor="login-password" className="form-label">Contraseña</label><input id="login-password" className="form-control mb-3" type="password" required value={password} onChange={e => setPassword(e.target.value)} autoComplete="current-password" />
      <div className="d-flex justify-content-between align-items-center mb-4"><div className="form-check"><input className="form-check-input" id="remember" type="checkbox" checked={remember} onChange={e => setRemember(e.target.checked)} /><label className="form-check-label" htmlFor="remember">Recordarme</label></div><span className="text-secondary small">Recuperar contraseña</span></div>
      <button className="btn btn-primary w-100" disabled={busy}>{busy ? 'Ingresando…' : 'Ingresar'}</button>
    </form></section>
  </main>
}
