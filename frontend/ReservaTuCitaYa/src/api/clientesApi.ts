import {apiRequest} from './apiClient';
import {
  ClienteListado,
  ClienteDetalle,
  CrearClienteRequest,
  ActualizarClienteRequest,
  ClienteFiltros,
  PaginaResultado
} from '../features/clientes/types/Cliente';

export async function listarClientes(filtros: ClienteFiltros): Promise<PaginaResultado<ClienteListado>> {
  return apiRequest('/clientes', { method: 'GET' });
}

export async function obtenerCliente(id: number): Promise<ClienteDetalle> {
  return apiRequest(`/clientes/${id}`, { method: 'GET' });
}

export async function crearCliente(data: CrearClienteRequest): Promise<ClienteDetalle> {
  return apiRequest('/clientes', {
    method: 'POST',
    body: JSON.stringify(data),
  });
}

export async function actualizarCliente(id: number, data: ActualizarClienteRequest): Promise<ClienteDetalle> {
  return apiRequest(`/clientes/${id}`, {
    method: 'PUT',
    body: JSON.stringify(data),
  });
}

export async function cambiarEstadoCliente(id: number, activo: boolean): Promise<void> {
  return apiRequest(`/clientes/${id}/estado`, {
    method: 'PATCH',
    body: JSON.stringify({ activo }),
  });
}

export async function eliminarCliente(id: number): Promise<void> {
  return apiRequest(`/clientes/${id}`, { method: 'DELETE' });
}

