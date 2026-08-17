import React, { useState } from 'react'
import type { PagoDto, EditarPagoRequest } from '../types/Pago'
import { editarPago } from '../../../api/pagosApi'

interface Props {
  pago: PagoDto
  onClose: () => void
  onSuccess: () => void
}

export const EditarPagoModal: React.FC<Props> = ({ pago, onClose, onSuccess }) => {
  const [numeroOperacion, setNumeroOperacion] = useState(pago.numeroOperacion || '')
  const [observacion, setObservacion] = useState(pago.observacion || '')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async () => {
    try {
      setLoading(true)
      const request: EditarPagoRequest = {
        numeroOperacion: numeroOperacion || undefined,
        observacion: observacion || undefined,
      }
      await editarPago(pago.id, request)
      onSuccess()
      onClose()
    } catch (err: any) {
      setError(err.message ?? 'Error al editar el pago.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-lg p-6 w-96">
        <h2 className="text-lg font-bold mb-4">Editar pago</h2>

        <p className="text-sm mb-2"><strong>Código:</strong> {pago.codigo}</p>
        <p className="text-sm mb-2"><strong>Monto:</strong> S/ {pago.monto.toFixed(2)}</p>
        <p className="text-sm mb-4"><strong>Método:</strong> {pago.metodoPago}</p>

        <label className="block text-sm font-medium mb-1">Número de operación</label>
        <input
          type="text"
          value={numeroOperacion}
          onChange={(e) => setNumeroOperacion(e.target.value)}
          className="w-full border rounded p-2 text-sm mb-3"
        />

        <label className="block text-sm font-medium mb-1">Observación</label>
        <textarea
          value={observacion}
          onChange={(e) => setObservacion(e.target.value)}
          className="w-full border rounded p-2 text-sm"
          rows={3}
        />

        {error && <p className="text-red-600 text-sm mt-2">{error}</p>}

        <div className="flex justify-end gap-2 mt-4">
          <button
            onClick={onClose}
            className="px-4 py-2 rounded bg-gray-300 hover:bg-gray-400"
            disabled={loading}
          >
            Volver
          </button>
          <button
            onClick={handleSubmit}
            className="px-4 py-2 rounded bg-blue-600 text-white hover:bg-blue-700"
            disabled={loading}
          >
            {loading ? 'Guardando...' : 'Guardar cambios'}
          </button>
        </div>
      </div>
    </div>
  )
}
