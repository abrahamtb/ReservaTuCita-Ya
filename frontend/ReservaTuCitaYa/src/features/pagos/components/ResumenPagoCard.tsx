import React from 'react'
import type { ResumenPagoReserva } from '../types/Pago'
import { useNavigate } from 'react-router-dom'

interface Props {
    resumen: ResumenPagoReserva
    onRegistrarPago?: () => void
    onVerMovimientos?: () => void
}

export const ResumenPagoCard: React.FC<Props> = ({ resumen, onRegistrarPago}) => {
    const {
        codigoReserva,
        clienteNombre,
        servicioNombre,
        precioTotal,
        adelantoRequerido,
        totalPagadoNeto,
        totalReembolsado,
        saldoPendiente,
        estadoPago,
    } = resumen
    const navigate = useNavigate()
    const renderEstadoBadge = (estado: string) => {
        const color =
            estado === 'SinPago' ? 'bg-gray-400'
                : estado === 'Parcial' ? 'bg-yellow-400'
                    : estado === 'Pagado' ? 'bg-green-500'
                        : estado === 'ReembolsoParcial' ? 'bg-blue-400'
                            : 'bg-red-500'

        return (
            <span className={`px-2 py-1 rounded text-white text-sm ${color}`}>
                {estado}
            </span>
        )
    }

    return (
        <div className="border rounded-lg shadow p-4 bg-white">
            <h2 className="text-lg font-bold mb-2">Resumen económico</h2>
            <div className="grid grid-cols-2 gap-2 text-sm">
                <div><strong>Reserva:</strong> {codigoReserva}</div>
                <div><strong>Cliente:</strong> {clienteNombre}</div>
                <div><strong>Servicio:</strong> {servicioNombre}</div>
                <div><strong>Precio total:</strong> S/ {precioTotal.toFixed(2)}</div>
                <div><strong>Adelanto requerido:</strong> S/ {adelantoRequerido.toFixed(2)}</div>
                <div><strong>Pagado:</strong> S/ {totalPagadoNeto.toFixed(2)}</div>
                <div><strong>Reembolsado:</strong> S/ {totalReembolsado.toFixed(2)}</div>
                <div><strong>Saldo:</strong> S/ {saldoPendiente.toFixed(2)}</div>
                <div><strong>Estado:</strong> {renderEstadoBadge(estadoPago)}</div>
            </div>

            <div className="mt-4 flex gap-2">
                {saldoPendiente > 0 ? (
                    <button
                        onClick={onRegistrarPago}
                        className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700"
                    >
                        💳 Registrar pago
                    </button>
                ) : (
                    <p className="text-green-600 font-semibold">
                        Reserva pagada completamente.
                    </p>
                )}
            </div>
        </div>
    )
}
