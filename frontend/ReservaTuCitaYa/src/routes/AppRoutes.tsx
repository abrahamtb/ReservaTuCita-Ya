import { Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from '../auth/ProtectedRoute'
import { AppLayout } from '../components/layout/AppLayout'
import { CategoriesPage, CategoryDetailPage, CategoryFormPage } from '../features/categorias/CategoryPages'
import { OrganizationDetailPage, OrganizationFormPage, OrganizationsPage } from '../features/organizaciones/OrganizationPages'
import { SedeDetailPage, SedeFormPage, SedesPage } from '../features/sedes/SedePages'
import { ServiceDetailPage, ServiceFormPage, ServicesPage } from '../features/servicios/ServicePages'
import { AccessDeniedPage } from '../pages/AccessDeniedPage'
import { DashboardPage } from '../pages/DashboardPage'
import { LoginPage } from '../pages/LoginPage'
import { NotFoundPage } from '../pages/NotFoundPage'
import ClientesPage from '../features/clientes/pages/ClientesPage'
import ClienteFormPage from '../features/clientes/pages/ClientesFormPage'
import EmpleadosPage from '../features/empleados/pages/EmpleadosPage'
import EmpleadoFormPage from '../features/empleados/pages/EmpleadoFormPage'
import { PagosReservaPage } from '../features/pagos/pages/PagosReservaPage'
import { PagosGlobalPage } from '../features/pagos/pages/PagosGlobalPage'

export function AppRoutes() {
  return <Routes>
    <Route path="/login" element={<LoginPage />} />
    <Route path="/acceso-denegado" element={<AccessDeniedPage />} />
    <Route element={<ProtectedRoute />}><Route element={<AppLayout />}>
      <Route index element={<DashboardPage />} />
      <Route path="organizaciones" element={<OrganizationsPage />} />
      <Route path="organizaciones/nueva" element={<OrganizationFormPage />} />
      <Route path="organizaciones/:id" element={<OrganizationDetailPage />} />
      <Route path="organizaciones/:id/editar" element={<OrganizationFormPage />} />
      <Route path="organizaciones/:organizationId/sedes" element={<SedesPage />} />
      <Route path="organizaciones/:organizationId/sedes/nueva" element={<SedeFormPage />} />
      <Route path="sedes/:id" element={<SedeDetailPage />} />
      <Route path="sedes/:id/editar" element={<SedeFormPage />} />
      <Route path="organizaciones/:organizationId/categorias" element={<CategoriesPage />} />
      <Route path="organizaciones/:organizationId/categorias/nueva" element={<CategoryFormPage />} />
      <Route path="categorias/:id" element={<CategoryDetailPage />} />
      <Route path="categorias/:id/editar" element={<CategoryFormPage />} />
      <Route path="organizaciones/:organizationId/servicios" element={<ServicesPage />} />
      <Route path="organizaciones/:organizationId/servicios/nuevo" element={<ServiceFormPage />} />
      <Route path="servicios/:id" element={<ServiceDetailPage />} />
      <Route path="servicios/:id/editar" element={<ServiceFormPage />} />
      <Route path="clientes" element={<ClientesPage />} />
      <Route path="clientes/nuevo" element={<ClienteFormPage />} />
      <Route path="empleados" element={<EmpleadosPage />} />
      <Route path="empleados/nuevo" element={<EmpleadoFormPage />} />
      <Route path="pagos" element={<PagosGlobalPage />} />
      <Route path="pagos/:reservaId" element={<PagosReservaPage />} />
    </Route></Route>
    <Route path="*" element={<NotFoundPage />} />
  </Routes>
}
