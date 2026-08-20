import { Navigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import { DashboardPage } from './DashboardPage'

export function HomePage() {
  const { user } = useAuth()
  const permisos = new Set(user?.permisos ?? [])
  const roles = user?.roles ?? []

  if (permisos.has('dashboard.ver')) return <DashboardPage />
  if (roles.includes('Profesional') && !user?.empleadoId) return <RoleContextState title="Profesional sin vínculo" detail="Tu usuario no está vinculado a un perfil profesional. Contacta con un administrador." />
  if (roles.includes('Profesional') && permisos.has('atenciones.ver')) return <Navigate to="/atenciones/agenda" replace />
  if (roles.includes('Cliente')) {
    if (!user?.clienteId) return <RoleContextState title="Cliente sin vínculo" detail="Tu cuenta todavía no está vinculada a un cliente. Contacta con un administrador." />
    if (!user?.organizacion?.id) return <RoleContextState title="Sin organización asignada" detail="Tu cuenta de cliente no tiene una organización seleccionada. La API actual requiere ese contexto para consultar tus reservas." />
    if (permisos.has('reservas.ver')) return <Navigate to="/reservas" replace />
    if (permisos.has('calificaciones.crear')) return <Navigate to="/calificaciones" replace />
  }
  if (permisos.has('reservas.ver')) return <Navigate to="/reservas" replace />
  if (permisos.has('clientes.ver') && user?.organizacion?.id) return <Navigate to={`/organizaciones/${user.organizacion.id}/clientes`} replace />
  if (permisos.has('organizaciones.ver')) return <Navigate to="/organizaciones" replace />

  return <RoleContextState title="Sin módulos habilitados" detail="Tu usuario no tiene módulos habilitados. Solicita permisos a un administrador." />
}

function RoleContextState({ title, detail }: { title: string; detail: string }) {
  return <section className="role-context-state"><p className="security-code">!</p><h1>{title}</h1><p>{detail}</p></section>
}
