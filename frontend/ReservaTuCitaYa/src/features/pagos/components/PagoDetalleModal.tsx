import React from 'react'
import type { PagoDto } from '../types/Pago'

interface Props {
  pago: PagoDto
  onClose: () => void
}

export const PagoDetalleModal: React.FC<Props> = ({ pago, onClose }) => {
  return (
    <div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-lg p-6 w-96">
        <h2 className="text-lg font-bold mb-4">Detalle del pago</h2>

        <p className="text-sm mb-2"><strong>Código:</strong> {pago.codigo}</p>
        <p className="text-sm mb-2"><strong>Fecha:</strong> {new Date(pago.fechaPago).toLocaleString()}</p>
        <p className="text-sm mb-2"><strong>Método:</strong> {pago.metodoPago}</p>
        <p className="text-sm mb-2"><strong>N° Operación:</strong> {pago.numeroOperacion || '—'}</p>
        <p className="text-sm mb-2"><strong>Monto:</strong> S/ {pago.monto.toFixed(2)}</p>
        <p className="text-sm mb-2"><strong>Estado:</strong> {pago.estadoMovimiento}</p>
        <p className="text-sm mb-2"><strong>Observación:</strong> {pago.observacion || '—'}</p>

        <div className="flex justify-end mt-4">
          <button
            onClick={onClose}
            className="px-4 py-2 rounded bg-gray-300 hover:bg-gray-400"
          >
            Cerrar
          </button>
        </div>
      </div>
    </div>
  )
}
