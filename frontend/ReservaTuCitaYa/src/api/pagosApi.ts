import { apiRequest } from './apiClient'

export interface MetodoPago { id: string; nombre: string; estaActivo: boolean }
export interface Pago { id: string; codigo: string; reservaId: string; metodoPago: string; monto: number; fechaPago: string; numeroOperacion?: string | null; observacion?: string | null; estaAnulado: boolean }
export interface Reembolso { id: string; codigo: string; reservaId: string; metodoPago?: string | null; monto: number; fechaReembolso: string; numeroOperacion?: string | null; motivo: string; observacion?: string | null }
export interface ResumenPago { reservaId: string; codigoReserva: string; precioTotal: number; adelantoRequerido: number; totalPagadoBruto: number; totalReembolsado: number; totalPagadoNeto: number; saldoPendiente: number; estadoPago: string; pagos: Pago[]; reembolsos: Reembolso[] }
export interface PagoRequest { metodoPagoId: string; monto: number; fechaPago: string; numeroOperacion?: string; observacion?: string }
export interface ReembolsoRequest { metodoPagoId: string; monto: number; fechaReembolso: string; numeroOperacion?: string; motivo: string; observacion?: string }

export const listarMetodosPago = (signal?: AbortSignal) => apiRequest<MetodoPago[]>('/api/metodospago', { signal })
export const obtenerResumenPago = (reservaId: string, signal?: AbortSignal) => apiRequest<ResumenPago>(`/api/pagos/resumen/${reservaId}`, { signal })
export const registrarPago = (reservaId: string, data: PagoRequest) => apiRequest<Pago>(`/api/pagos/${reservaId}`, { method: 'POST', body: JSON.stringify(data) })
export const anularPago = (pagoId: string, motivo: string) => apiRequest<Pago>(`/api/pagos/anular/${pagoId}`, { method: 'PUT', body: JSON.stringify({ motivo }) })
export const registrarReembolso = (reservaId: string, data: ReembolsoRequest) => apiRequest<Reembolso>(`/api/reembolsos/${reservaId}`, { method: 'POST', body: JSON.stringify(data) })
