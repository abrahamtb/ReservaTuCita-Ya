import { useEffect, useState } from 'react'
import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../../auth/useAuth'

export function AppLayout() {
  const { user, logout } = useAuth()
  const [selectedOrganizationId, setSelectedOrganizationId] = useState(() => window.localStorage.getItem('reserva-tu-cita:selected-organization') ?? '')
  useEffect(() => {
    const updateSelection = () => setSelectedOrganizationId(window.localStorage.getItem('reserva-tu-cita:selected-organization') ?? '')
    window.addEventListener('reserva-tu-cita:organization-selected', updateSelection)
    return () => window.removeEventListener('reserva-tu-cita:organization-selected', updateSelection)
  }, [])
  const orgId = user?.organizacion?.id ?? selectedOrganizationId
  const permisos = new Set(user?.permisos ?? [])
  const roles = user?.roles ?? []
  const has = (permission: string) => permisos.has(permission)
  const isClient = roles.includes('Cliente')
  const isProfessional = roles.includes('Profesional')
  const isReception = roles.includes('Recepcionista')
  const isAdmin = roles.includes('Administrador') || roles.includes('Superadministrador')
  const organizationPath = (suffix: string) => orgId ? `/organizaciones/${orgId}/${suffix}` : '/organizaciones'
  const primaryRole = roles[0] ?? 'Usuario'

  return <div className="app-shell">
    <header className="navbar navbar-dark bg-primary px-3 shadow-sm app-topbar">
      <NavLink className="navbar-brand fw-semibold brand-lockup" to="/"><span className="brand-lockup__mark">R</span><span>Reserva tu<br />Cita Ya</span></NavLink>
      <div className="d-flex align-items-center gap-3 text-white">
        <div className="text-end d-none d-sm-block"><div className="small fw-semibold">{user?.email}</div><div className="small opacity-75">{primaryRole}</div></div>
        <span className="user-initial" aria-hidden="true">{(user?.email?.[0] ?? 'U').toUpperCase()}</span>
        <button className="btn btn-sm btn-outline-light" onClick={() => void logout()}>Cerrar sesión</button>
      </div>
    </header>
    <div className="app-body">
      <aside className="app-sidebar">
        <div className="small fw-bold text-secondary text-uppercase px-3 mb-2">Gestión</div>
        <nav className="nav flex-column gap-1">
          {isAdmin && has('dashboard.ver') && <NavLink className="nav-link" to="/">Dashboard</NavLink>}
          {!isClient && !isProfessional && has('clientes.ver') && (orgId ? <NavLink className="nav-link" to={organizationPath('clientes')}>Clientes</NavLink> : <NavLink className="nav-link" end to="/organizaciones">Seleccionar organización</NavLink>)}
          {isAdmin && has('empleados.ver') && <NavLink className="nav-link" to={organizationPath('empleados')}>Empleados y profesionales</NavLink>}
          {(isAdmin || isReception) && has('servicios.ver') && <NavLink className="nav-link" to={organizationPath('servicios')}>Servicios</NavLink>}
          {isAdmin && has('recursos.ver') && <NavLink className="nav-link" to={orgId ? `/organizaciones/${orgId}/sedes` : '/organizaciones'}>Recursos</NavLink>}
          {isAdmin && has('horarios.ver') && <NavLink className="nav-link" to="/horarios">Horarios</NavLink>}
          {(isAdmin || isReception) && has('reservas.ver') && <NavLink className="nav-link" to="/reservas">Reservas</NavLink>}
          {isClient && has('reservas.crear') && orgId && <NavLink className="nav-link" to={`/organizaciones/${orgId}/reservas/nueva`}>Nueva reserva</NavLink>}
          {isClient && has('reservas.ver') && orgId && <NavLink className="nav-link" to="/reservas">Mis reservas</NavLink>}
          {isReception && has('reservas.ver') && <NavLink className="nav-link" to="/disponibilidad">Disponibilidad</NavLink>}
          {isProfessional && has('atenciones.ver') && <NavLink className="nav-link" to="/atenciones/agenda">Mi agenda</NavLink>}
          {(isAdmin || isReception) && has('atenciones.ver') && <NavLink className="nav-link" to="/atenciones/agenda">Atenciones</NavLink>}
          {!isProfessional && has('pagos.ver') && <NavLink className="nav-link" to="/pagos">{isClient ? 'Mis pagos' : 'Pagos'}</NavLink>}
          {isClient && has('calificaciones.crear') && orgId && <NavLink className="nav-link" to="/calificaciones">Calificaciones</NavLink>}
          {isAdmin && has('reportes.ver') && <NavLink className="nav-link" to="/reportes">Reportes</NavLink>}
        </nav>

        {isAdmin && (has('organizaciones.ver') || has('sedes.ver') || has('roles.ver') || has('usuarios.ver')) && <>
          <div className="small fw-bold text-secondary text-uppercase px-3 mt-4 mb-2">Configuración</div>
          <nav className="nav flex-column gap-1">
            {has('organizaciones.ver') && <NavLink className="nav-link" to="/organizaciones">Organización</NavLink>}
            {has('sedes.ver') && <NavLink className="nav-link" to={organizationPath('sedes')}>Sedes</NavLink>}
            {has('servicios.ver') && <NavLink className="nav-link" to={organizationPath('categorias')}>Categorías</NavLink>}
            {(has('usuarios.ver') || has('roles.ver')) && <NavLink className="nav-link" to="/usuarios-roles">Usuarios y roles</NavLink>}
          </nav>
        </>}

        {!orgId && isAdmin && <p className="small text-secondary mt-4 px-3">Selecciona una organización para administrar sus clientes, sedes, servicios y personal.</p>}
        {!orgId && isClient && <p className="small text-secondary mt-4 px-3">Tu cuenta no tiene una organización asignada.</p>}
      </aside>
      <main className="app-content"><Outlet /></main>
    </div>
    <MobileBottomNav roles={roles} permissions={permisos} orgId={orgId} />
  </div>
}

function MobileBottomNav({ roles, permissions, orgId }: { roles: string[]; permissions: Set<string>; orgId?: string }) {
  const client = roles.includes('Cliente')
  const professional = roles.includes('Profesional')
  const items = client
    ? [{ to: '/', label: 'Inicio' }, ...(permissions.has('reservas.ver') ? [{ to: '/reservas', label: 'Reservas' }] : []), ...(orgId && permissions.has('reservas.crear') ? [{ to: `/organizaciones/${orgId}/reservas/nueva`, label: 'Nueva' }] : []), ...(permissions.has('pagos.ver') ? [{ to: '/pagos', label: 'Pagos' }] : [])]
    : professional
      ? [{ to: '/', label: 'Inicio' }, { to: '/atenciones/agenda', label: 'Agenda' }]
      : [{ to: '/', label: 'Inicio' }, ...(permissions.has('reservas.ver') ? [{ to: '/reservas', label: 'Reservas' }] : []), ...(permissions.has('pagos.ver') ? [{ to: '/pagos', label: 'Pagos' }] : [])]
  return <nav className="mobile-bottom-nav" aria-label="Navegación móvil">{items.map(item => <NavLink key={item.to} to={item.to}>{item.label}</NavLink>)}</nav>
}
