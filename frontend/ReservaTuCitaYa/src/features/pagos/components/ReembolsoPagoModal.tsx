import React, { useState } from 'react'
import type { PagoDto, ReembolsoPagoRequest } from '../types/Pago'
import { registrarReembolso } from '../../../api/pagosApi'

interface Props {
  pago: PagoDto
  onClose: () => void
  onSuccess: () => void
}

export const ReembolsoPagoModal: React.FC<Props> = ({ pago, onClose, onSuccess }) => {
  const [monto, setMonto] = useState<number>(pago.monto) // por defecto el monto total
  const [motivo, setMotivo] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async () => {
    if (monto <= 0 || monto > pago.monto) {
      setError('El monto debe ser mayor a 0 y no superar el monto original.')
      return
    }
    if (!motivo.trim()) {
      setError('El motivo del reembolso es obligatorio.')
      return
    }

    try {
      setLoading(true)
      const request: ReembolsoPagoRequest = { monto, motivo }
      await registrarReembolso(pago.id, request)
      onSuccess()
      onClose()
    } catch (err: any) {
      setError(err.message ?? 'Error al registrar reembolso.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-lg p-6 w-96">
        <h2 className="text-lg font-bold mb-4">Reembolsar pago</h2>

        <p className="text-sm mb-2"><strong>Código:</strong> {pago.codigo}</p>
        <p className="text-sm mb-2"><strong>Monto original:</strong> S/ {pago.monto.toFixed(2)}</p>
        <p className="text-sm mb-4"><strong>Método:</strong> {pago.metodoPago}</p>

        <label className="block text-sm font-medium mb-1">Monto a reembolsar</label>
        <input
          type="number"
          value={monto}
          onChange={(e) => setMonto(parseFloat(e.target.value))}
          className="w-full border rounded p-2 text-sm mb-3"
          min="0"
          step="0.01"
        />

        <label className="block text-sm font-medium mb-1">Motivo del reembolso</label>
        <textarea
          value={motivo}
          onChange={(e) => setMotivo(e.target.value)}
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
            {loading ? 'Procesando...' : 'Confirmar reembolso'}
          </button>
        </div>
      </div>
    </div>
  )
}
