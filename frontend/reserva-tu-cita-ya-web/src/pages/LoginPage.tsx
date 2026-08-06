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
  return <main className="login-page"><form className="card shadow-sm login-card" onSubmit={submit}>
    <div className="card-body p-4"><h1 className="h3 mb-1">Reserva tu Cita Ya</h1><p className="text-secondary mb-4">Acceso administrativo</p>
      {error ? <ErrorAlert error={error} /> : null}
      <label className="form-label">Correo</label><input className="form-control mb-3" type="email" required value={email} onChange={e => setEmail(e.target.value)} autoComplete="username" />
      <label className="form-label">Contraseña</label><input className="form-control mb-3" type="password" required value={password} onChange={e => setPassword(e.target.value)} autoComplete="current-password" />
      <div className="form-check mb-3"><input className="form-check-input" id="remember" type="checkbox" checked={remember} onChange={e => setRemember(e.target.checked)} /><label className="form-check-label" htmlFor="remember">Recordarme</label></div>
      <button className="btn btn-primary w-100" disabled={busy}>{busy ? 'Ingresando…' : 'Ingresar'}</button>
    </div></form></main>
}
