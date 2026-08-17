import { apiRequest, queryString } from "./apiClient";
import {
  EmpleadoListado,
  EmpleadoDetalle,
  CrearEmpleadoRequest,
  ActualizarEmpleadoRequest,
  EmpleadoFiltros,
  PaginaResultado,
  SedeAsignada,
  ServicioAsignado,
  ProfesionalSelector
} from "../features/empleados/types/Empleado";

// Listar empleados con filtros y paginación
export async function listarEmpleados(filtros: EmpleadoFiltros): Promise<PaginaResultado<EmpleadoListado>> {
  const qs = queryString({
    busqueda: filtros.busqueda,
    estado: filtros.estado,
    esProfesional: filtros.esProfesional,
    sedeId: filtros.sedeId,
    servicioId: filtros.servicioId,
    pagina: filtros.pagina,
    tamañoPagina: filtros.tamañoPagina,
  });
  return apiRequest<PaginaResultado<EmpleadoListado>>(`/api/empleados${qs}`);
}

// Obtener detalle de un empleado
export async function obtenerEmpleado(id: string): Promise<EmpleadoDetalle> {
  return apiRequest<EmpleadoDetalle>(`/api/empleados/${id}`);
}

// Crear empleado
export async function crearEmpleado(request: CrearEmpleadoRequest): Promise<EmpleadoDetalle> {
  return apiRequest<EmpleadoDetalle>("/api/empleados", {
    method: "POST",
    body: JSON.stringify(request),
  });
}

// Actualizar empleado
export async function actualizarEmpleado(request: ActualizarEmpleadoRequest): Promise<EmpleadoDetalle> {
  return apiRequest<EmpleadoDetalle>(`/api/empleados/${request.id}`, {
    method: "PUT",
    body: JSON.stringify(request),
  });
}

// Cambiar estado (activar/desactivar)
export async function cambiarEstadoEmpleado(id: string, activo: boolean): Promise<void> {
  await apiRequest<void>(`/api/empleados/${id}/estado`, {
    method: "PATCH",
    body: JSON.stringify({ estaActivo: activo }),
  });
}

// Eliminar empleado (lógica, no física)
export async function eliminarEmpleado(id: string): Promise<void> {
  await apiRequest<void>(`/api/empleados/${id}`, { method: "DELETE" });
}

// Obtener sedes asignadas a un empleado
export async function obtenerSedesEmpleado(id: string): Promise<SedeAsignada[]> {
  return apiRequest<SedeAsignada[]>(`/api/empleados/${id}/sedes`);
}

// Actualizar sedes de un empleado
export async function actualizarSedesEmpleado(id: string, sedes: string[]): Promise<SedeAsignada[]> {
  return apiRequest<SedeAsignada[]>(`/api/empleados/${id}/sedes`, {
    method: "PUT",
    body: JSON.stringify({ sedes }),
  });
}

// Obtener servicios asignados a un profesional
export async function obtenerServiciosProfesional(id: string): Promise<ServicioAsignado[]> {
  return apiRequest<ServicioAsignado[]>(`/api/profesionales/${id}/servicios`);
}

// Actualizar servicios de un profesional
export async function actualizarServiciosProfesional(id: string, servicios: string[]): Promise<ServicioAsignado[]> {
  return apiRequest<ServicioAsignado[]>(`/api/profesionales/${id}/servicios`, {
    method: "PUT",
    body: JSON.stringify({ servicios }),
  });
}

// Listar profesionales (solo empleados con esProfesional = true)
export async function listarProfesionales(filtros: EmpleadoFiltros): Promise<PaginaResultado<ProfesionalSelector>> {
  const qs = queryString({
    busqueda: filtros.busqueda,
    estado: filtros.estado,
    sedeId: filtros.sedeId,
    servicioId: filtros.servicioId,
    pagina: filtros.pagina,
    tamañoPagina: filtros.tamañoPagina,
  });
  return apiRequest<PaginaResultado<ProfesionalSelector>>(`/api/profesionales${qs}`);
}

export async function obtenerEmpleadoDetalle(id: string): Promise<EmpleadoDetalle> {
  return apiRequest<EmpleadoDetalle>(`/api/empleados/${id}`);
}