import { Navigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import { DashboardPage } from './DashboardPage'

export function HomePage() {
  const { user } = useAuth()
  const permisos = new Set(user?.permisos ?? [])
  const roles = user?.roles ?? []

  if (permisos.has('dashboard.ver')) return <DashboardPage />
  if (roles.includes('Profesional') && permisos.has('atenciones.ver')) return <Navigate to="/atenciones/agenda" replace />
  if (roles.includes('Cliente')) {
    if (permisos.has('reservas.ver')) return <Navigate to="/reservas" replace />
    if (permisos.has('calificaciones.crear')) return <Navigate to="/calificaciones" replace />
  }
  if (permisos.has('reservas.ver')) return <Navigate to="/reservas" replace />
  if (permisos.has('clientes.ver') && user?.organizacion?.id) return <Navigate to={`/organizaciones/${user.organizacion.id}/clientes`} replace />
  if (permisos.has('organizaciones.ver')) return <Navigate to="/organizaciones" replace />

  return <div className="alert alert-info">Tu usuario no tiene módulos habilitados. Solicita permisos a un administrador.</div>
}
