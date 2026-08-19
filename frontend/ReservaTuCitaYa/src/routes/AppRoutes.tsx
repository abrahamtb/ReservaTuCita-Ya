import { Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from '../auth/ProtectedRoute'
import { AppLayout } from '../components/layout/AppLayout'
import { AttentionDetailPage } from '../features/atenciones/AttentionDetailPage'
import { ProfessionalAgendaPage } from '../features/atenciones/ProfessionalAgendaPage'
import { CalificacionesPage } from '../features/calificaciones/CalificacionesPage'
import { CategoriesPage, CategoryDetailPage, CategoryFormPage } from '../features/categorias/CategoryPages'
import { ClienteActualDetailPage, ClienteActualFormPage, ClientesActualPage } from '../features/clientes/ClientePages'
import { DisponibilidadPage } from '../features/disponibilidad/DisponibilidadPage'
import { EmpleadoDetailPage, EmpleadoFormPage, EmpleadosPage } from '../features/empleados/EmpleadoPages'
import { HorariosPage } from '../features/horarios/HorariosPage'
import { HorariosSedePage } from '../features/horarios/HorariosSedePage'
import { OrganizationDetailPage, OrganizationFormPage, OrganizationsPage } from '../features/organizaciones/OrganizationPages'
import { PagosIndexPage } from '../features/pagos/PagosIndexPage'
import { PagosReservaPage } from '../features/pagos/PagosReservaPage'
import { RecursoDetailWithBloqueosPage } from '../features/recursos/RecursoDetailWithBloqueosPage'
import { RecursoFormPage, RecursosPage } from '../features/recursos/RecursoPages'
import { ReportsPage } from '../features/reportes/ReportsPage'
import { ReservaDetailPage } from '../features/reservas/ReservaDetailPage'
import { ReservaFormPage } from '../features/reservas/ReservaFormPage'
import { ReservasPage } from '../features/reservas/ReservasPage'
import { UsuariosRolesPage } from '../features/seguridad/UsuariosRolesPage'
import { SedeDetailPage, SedeFormPage, SedesPage } from '../features/sedes/SedePages'
import { ServiceDetailPage, ServiceFormPage, ServicesPage } from '../features/servicios/ServicePages'
import { AccessDeniedPage } from '../pages/AccessDeniedPage'
import { HomePage } from '../pages/HomePage'
import { LoginPage } from '../pages/LoginPage'
import { NotFoundPage } from '../pages/NotFoundPage'

export function AppRoutes() {
  return <Routes>
    <Route path="/login" element={<LoginPage />} />
    <Route path="/acceso-denegado" element={<AccessDeniedPage />} />
    <Route element={<ProtectedRoute />}><Route element={<AppLayout />}>
      <Route index element={<HomePage />} />
      <Route path="reservas" element={<ReservasPage />} />
      <Route path="reservas/:id" element={<ReservaDetailPage />} />
      <Route path="organizaciones/:organizationId/reservas/nueva" element={<ReservaFormPage />} />
      <Route path="pagos" element={<PagosIndexPage />} />
      <Route path="pagos/:reservaId" element={<PagosReservaPage />} />
      <Route path="disponibilidad" element={<DisponibilidadPage />} />
      <Route path="horarios" element={<HorariosPage />} />
      <Route path="calificaciones" element={<CalificacionesPage />} />
      <Route path="reportes" element={<ReportsPage />} />
      <Route path="usuarios-roles" element={<UsuariosRolesPage />} />
      <Route path="organizaciones" element={<OrganizationsPage />} />
      <Route path="organizaciones/nueva" element={<OrganizationFormPage />} />
      <Route path="organizaciones/:id" element={<OrganizationDetailPage />} />
      <Route path="organizaciones/:id/editar" element={<OrganizationFormPage />} />
      <Route path="organizaciones/:organizationId/sedes" element={<SedesPage />} />
      <Route path="organizaciones/:organizationId/sedes/nueva" element={<SedeFormPage />} />
      <Route path="sedes/:id" element={<SedeDetailPage />} />
      <Route path="sedes/:id/editar" element={<SedeFormPage />} />
      <Route path="organizaciones/:organizationId/sedes/:sedeId/recursos" element={<RecursosPage />} />
      <Route path="organizaciones/:organizationId/sedes/:sedeId/recursos/nuevo" element={<RecursoFormPage />} />
      <Route path="organizaciones/:organizationId/sedes/:sedeId/recursos/:id" element={<RecursoDetailWithBloqueosPage />} />
      <Route path="organizaciones/:organizationId/sedes/:sedeId/recursos/:id/editar" element={<RecursoFormPage />} />
      <Route path="organizaciones/:organizationId/sedes/:sedeId/horarios" element={<HorariosSedePage />} />
      <Route path="organizaciones/:organizationId/categorias" element={<CategoriesPage />} />
      <Route path="organizaciones/:organizationId/categorias/nueva" element={<CategoryFormPage />} />
      <Route path="categorias/:id" element={<CategoryDetailPage />} />
      <Route path="categorias/:id/editar" element={<CategoryFormPage />} />
      <Route path="organizaciones/:organizationId/servicios" element={<ServicesPage />} />
      <Route path="organizaciones/:organizationId/servicios/nuevo" element={<ServiceFormPage />} />
      <Route path="servicios/:id" element={<ServiceDetailPage />} />
      <Route path="servicios/:id/editar" element={<ServiceFormPage />} />
      <Route path="organizaciones/:organizationId/clientes" element={<ClientesActualPage />} />
      <Route path="organizaciones/:organizationId/clientes/nuevo" element={<ClienteActualFormPage />} />
      <Route path="organizaciones/:organizationId/clientes/:id" element={<ClienteActualDetailPage />} />
      <Route path="organizaciones/:organizationId/clientes/:id/editar" element={<ClienteActualFormPage />} />
      <Route path="organizaciones/:organizationId/empleados" element={<EmpleadosPage />} />
      <Route path="organizaciones/:organizationId/empleados/nuevo" element={<EmpleadoFormPage />} />
      <Route path="organizaciones/:organizationId/empleados/:id" element={<EmpleadoDetailPage />} />
      <Route path="organizaciones/:organizationId/empleados/:id/editar" element={<EmpleadoFormPage />} />
      <Route path="atenciones/agenda" element={<ProfessionalAgendaPage />} />
      <Route path="organizaciones/:organizationId/reservas/:reservationId/atencion" element={<AttentionDetailPage />} />
    </Route></Route>
    <Route path="*" element={<NotFoundPage />} />
  </Routes>
}
