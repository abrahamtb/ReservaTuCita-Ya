import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../../auth/useAuth'


export function AppLayout() {
  const { user, logout } = useAuth()
  return <div className="app-shell">
    <header className="navbar navbar-dark bg-primary px-3 shadow-sm">
      <NavLink className="navbar-brand fw-semibold" to="/">Reserva tu Cita Ya</NavLink>
      <div className="d-flex align-items-center gap-3 text-white">
        <span className="small">{user?.email}</span>
        <button className="btn btn-sm btn-outline-light" onClick={() => void logout()}>Cerrar sesión</button>
      </div>
    </header>
    <div className="app-body">
      <aside className="app-sidebar">
        <nav className="nav flex-column gap-1">
          <NavLink className="nav-link" to="/">Inicio</NavLink>
          <NavLink className="nav-link" to="/organizaciones">Organizaciones</NavLink>
          <NavLink className="nav-link" to="/atenciones/agenda">Agenda de atención</NavLink>
        </nav>
        <p className="small text-secondary mt-4">Sedes, categorías y servicios se administran dentro de cada organización.</p>
      </aside>
      <main className="app-content"><Outlet /></main>
    </div>
  </div>
}
