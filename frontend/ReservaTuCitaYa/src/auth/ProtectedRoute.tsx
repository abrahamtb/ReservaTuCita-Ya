import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from './useAuth'

export function ProtectedRoute() {
  const { user, loading } = useAuth()
  if (loading) return <div className="app-loading">Comprobando sesión…</div>
  if (!user) return <Navigate to="/login" replace />
  const authorized = user.roles.some(role => role === 'Administrador' || role === 'Superadministrador')
  return authorized ? <Outlet /> : <Navigate to="/acceso-denegado" replace />
}




