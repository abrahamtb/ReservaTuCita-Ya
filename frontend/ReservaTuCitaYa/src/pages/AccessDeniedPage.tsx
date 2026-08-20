import { Link } from 'react-router-dom'

export function AccessDeniedPage() {
  return <main className="security-state">
    <p className="security-code">403</p>
    <h1>Acceso denegado</h1>
    <p>No tienes permisos para acceder a esta sección.</p>
    <div className="d-flex gap-2 justify-content-center">
      <Link className="btn btn-outline-primary" to="/">Volver</Link>
      <Link className="btn btn-primary" to="/">Ir al inicio</Link>
    </div>
  </main>
}
