import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../../auth/useAuth'

export function AppLayout() {
  const { user, logout } = useAuth()
  const orgId = user?.organizacion?.id
  const permisos = new Set(user?.permisos ?? [])
  const roles = user?.roles ?? []
  const has = (permission: string) => permisos.has(permission)
  const isClient = roles.includes('Cliente')
  const isProfessional = roles.includes('Profesional')
  const organizationPath = (suffix: string) => orgId ? `/organizaciones/${orgId}/${suffix}` : '/organizaciones'
  const primaryRole = roles[0] ?? 'Usuario'

  return <div className="app-shell">
    <header className="navbar navbar-dark bg-primary px-3 shadow-sm">
      <NavLink className="navbar-brand fw-semibold" to="/">Reserva tu Cita Ya</NavLink>
      <div className="d-flex align-items-center gap-3 text-white">
        <div className="text-end d-none d-sm-block"><div className="small fw-semibold">{user?.email}</div><div className="small opacity-75">{primaryRole}</div></div>
        <button className="btn btn-sm btn-outline-light" onClick={() => void logout()}>Cerrar sesión</button>
      </div>
    </header>
    <div className="app-body">
      <aside className="app-sidebar">
        <div className="small fw-bold text-secondary text-uppercase px-3 mb-2">Gestión</div>
        <nav className="nav flex-column gap-1">
          {has('dashboard.ver') && <NavLink className="nav-link" to="/">Dashboard</NavLink>}
          {has('clientes.ver') && <NavLink className="nav-link" to={organizationPath('clientes')}>Clientes</NavLink>}
          {has('empleados.ver') && <NavLink className="nav-link" to={organizationPath('empleados')}>Empleados y profesionales</NavLink>}
          {has('servicios.ver') && <NavLink className="nav-link" to={organizationPath('servicios')}>Servicios</NavLink>}
          {has('recursos.ver') && <NavLink className="nav-link" to={orgId ? `/organizaciones/${orgId}/sedes` : '/organizaciones'}>Recursos</NavLink>}
          {has('horarios.ver') && <NavLink className="nav-link" to="/horarios">Horarios</NavLink>}
          {has('reservas.ver') && <NavLink className="nav-link" to="/reservas">{isClient ? 'Mis reservas' : 'Reservas'}</NavLink>}
          {has('reservas.crear') && isClient && orgId && <NavLink className="nav-link" to={`/organizaciones/${orgId}/reservas/nueva`}>Nueva reserva</NavLink>}
          {has('atenciones.ver') && <NavLink className="nav-link" to="/atenciones/agenda">{isProfessional ? 'Mi agenda' : 'Atenciones'}</NavLink>}
          {has('pagos.ver') && <NavLink className="nav-link" to="/pagos">{isClient ? 'Mis pagos' : 'Pagos'}</NavLink>}
          {has('calificaciones.crear') && <NavLink className="nav-link" to="/calificaciones">Calificaciones</NavLink>}
          {has('reportes.ver') && <NavLink className="nav-link" to="/reportes">Reportes</NavLink>}
        </nav>

        {(has('organizaciones.ver') || has('sedes.ver') || has('roles.ver') || has('usuarios.ver')) && <>
          <div className="small fw-bold text-secondary text-uppercase px-3 mt-4 mb-2">Configuración</div>
          <nav className="nav flex-column gap-1">
            {has('organizaciones.ver') && <NavLink className="nav-link" to="/organizaciones">Organización</NavLink>}
            {has('sedes.ver') && <NavLink className="nav-link" to={organizationPath('sedes')}>Sedes</NavLink>}
            {has('servicios.ver') && <NavLink className="nav-link" to={organizationPath('categorias')}>Categorías</NavLink>}
            {(has('usuarios.ver') || has('roles.ver')) && <NavLink className="nav-link" to="/usuarios-roles">Usuarios y roles</NavLink>}
          </nav>
        </>}

        {!orgId && !isClient && <p className="small text-secondary mt-4 px-3">Selecciona una organización para administrar sus clientes, sedes, servicios y personal.</p>}
      </aside>
      <main className="app-content"><Outlet /></main>
    </div>
  </div>
}
