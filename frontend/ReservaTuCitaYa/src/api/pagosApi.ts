import { apiRequest, queryString } from './apiClient';
import type { CrearPagoRequest, AnularPagoRequest, ResumenPagoReserva, PagoDto, EditarPagoRequest, ReembolsoPagoRequest } from '../features/pagos/types/Pago';

export const listarMetodosPago = () =>
    apiRequest('/api/metodospago');

export const obtenerResumenPagoReserva = (reservaId: string) =>
    apiRequest<ResumenPagoReserva>(`/api/pagos/resumen/${reservaId}`);

export const listarPagosReserva = (reservaId: string) =>
    apiRequest(`/api/pagos/${reservaId}`);

export const registrarPago = (reservaId: string, data: CrearPagoRequest) =>
    apiRequest(`/api/pagos/${reservaId}`, {
        method: 'POST',
        body: JSON.stringify(data),
    });

export const anularPago = (pagoId: string, data: AnularPagoRequest) =>
    apiRequest(`/api/pagos/anular/${pagoId}`, {
        method: 'PUT',
        body: JSON.stringify(data),
    });

export const listarReembolsosReserva = (reservaId: string) =>
    apiRequest(`/api/reembolsos/${reservaId}`);

export const registrarReembolso = async (pagoId: string, request: ReembolsoPagoRequest) =>
  apiRequest(`/api/pagos/${pagoId}/reembolsos`, {
    method: 'POST',
    body: JSON.stringify(request),
  })

export const listarPagos = () =>
    apiRequest<PagoDto[]>('/api/pagos')

export const editarPago = async (pagoId: string, data: EditarPagoRequest) =>
    apiRequest(`/api/pagos/${pagoId}`, {
        method: 'PUT',
        body: JSON.stringify(data),
    })
