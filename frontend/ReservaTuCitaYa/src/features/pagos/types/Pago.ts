
export type EstadoPagoReserva =
  | 'SinPago'
  | 'Parcial'
  | 'Pagado'
  | 'ReembolsoParcial'
  | 'Reembolsado';

export interface MetodoPago {
  id: string;
  nombre: string;
  estaActivo: boolean;
}

export interface CrearPagoRequest {
  metodoPagoId: string;
  monto: number;
  fechaPago: string;
  numeroOperacion?: string;
  observacion?: string;
}

export interface AnularPagoRequest {
  motivo: string;
}

export interface PagoDto {
  id: string;
  codigo: string;
  reservaId: string;
  metodoPago: string;
  monto: number;
  fechaPago: string;
  numeroOperacion?: string;
  observacion?: string;
  estadoMovimiento: string;
  usuario?: string;
  clienteNombre?: string;
}

export interface ResumenPagoReserva {
  reservaId: string;
  codigoReserva: string;
  clienteNombre: string;
  servicioNombre: string;
  precioTotal: number;
  adelantoRequerido: number;
  totalPagadoBruto: number;
  totalReembolsado: number;
  totalPagadoNeto: number;
  saldoPendiente: number;
  estadoPago: EstadoPagoReserva;
  pagos?: PagoDto[];
  reembolsos?: ReembolsoDto[];
}

export interface ReembolsoDto {
  id: string;
  codigo: string;
  reservaId: string;
  metodoPago: string;
  monto: number;
  fechaReembolso: string;
  numeroOperacion?: string;
  motivo: string;
  observacion?: string;
}

export interface EditarPagoRequest {
  numeroOperacion?: string
  observacion?: string
}

export interface ReembolsoPagoRequest {
  monto: number
  motivo: string
}
