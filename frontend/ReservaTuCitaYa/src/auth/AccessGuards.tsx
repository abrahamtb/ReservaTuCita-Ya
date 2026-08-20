import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from './useAuth'

export function PermissionGuard({ anyOf }: { anyOf: string[] }) {
  const { user, loading } = useAuth()
  if (loading) return <div className="app-loading">Comprobando permisos…</div>
  return anyOf.some(permission => user?.permisos.includes(permission))
    ? <Outlet />
    : <Navigate to="/acceso-denegado" replace />
}

export function RoleGuard({ roles }: { roles: string[] }) {
  const { user, loading } = useAuth()
  if (loading) return <div className="app-loading">Comprobando permisos…</div>
  return roles.some(role => user?.roles.includes(role))
    ? <Outlet />
    : <Navigate to="/acceso-denegado" replace />
}
