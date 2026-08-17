import React, { useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { registrarPago } from '../../../api/pagosApi'
import type { CrearPagoRequest } from '../types/Pago'

export const RegistrarPagoPage: React.FC = () => {
    const { reservaId } = useParams<{ reservaId: string }>()
    const navigate = useNavigate()

    const [metodoPagoId, setMetodoPagoId] = useState('')
    const [monto, setMonto] = useState<number>(0)
    const [fechaPago, setFechaPago] = useState('')
    const [numeroOperacion, setNumeroOperacion] = useState('')
    const [observacion, setObservacion] = useState('')
    const [loading, setLoading] = useState(false)
    const [error, setError] = useState<string | null>(null)

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()

        if (!metodoPagoId || monto <= 0 || !fechaPago) {
            setError('Método de pago, monto y fecha son obligatorios.')
            return
        }

        try {
            setLoading(true)
            const request: CrearPagoRequest = {
                metodoPagoId,
                monto,
                fechaPago,
                numeroOperacion: numeroOperacion || undefined,
                observacion: observacion || undefined,
            }
            await registrarPago(reservaId!, request)
            navigate(`/pagos/${reservaId}`, { state: { refresh: true } })
        } catch (err: any) {
            setError(err.message ?? 'Error al registrar pago.')
        } finally {
            setLoading(false)
        }
    }

    return (
        <div className="p-6 max-w-lg mx-auto">
            <h2 className="text-xl font-bold mb-4">Registrar pago</h2>

            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="block text-sm font-medium mb-1">Método de pago</label>
                    <select
                        value={metodoPagoId}
                        onChange={(e) => setMetodoPagoId(e.target.value)}
                        className="border rounded p-2 w-full"
                    >
                        <option value="">Seleccionar método</option>
                        <option value="TARJETA">Tarjeta</option>
                        <option value="EFECTIVO">Efectivo</option>
                        <option value="TRANSFERENCIA">Transferencia</option>
                    </select>
                </div>

                <div>
                    <label className="block text-sm font-medium mb-1">Monto</label>
                    <input
                        type="number"
                        value={monto}
                        onChange={(e) => setMonto(parseFloat(e.target.value))}
                        className="border rounded p-2 w-full"
                        min="0"
                        step="0.01"
                    />
                </div>

                <div>
                    <label className="block text-sm font-medium mb-1">Fecha de pago</label>
                    <input
                        type="date"
                        value={fechaPago}
                        onChange={(e) => setFechaPago(e.target.value)}
                        className="border rounded p-2 w-full"
                    />
                </div>

                <div>
                    <label className="block text-sm font-medium mb-1">Número de operación (opcional)</label>
                    <input
                        type="text"
                        value={numeroOperacion}
                        onChange={(e) => setNumeroOperacion(e.target.value)}
                        className="border rounded p-2 w-full"
                    />
                </div>

                <div>
                    <label className="block text-sm font-medium mb-1">Observación (opcional)</label>
                    <textarea
                        value={observacion}
                        onChange={(e) => setObservacion(e.target.value)}
                        className="border rounded p-2 w-full"
                        rows={3}
                    />
                </div>

                {error && <p className="text-red-600 text-sm">{error}</p>}

                <div className="flex justify-end gap-2">
                    <button
                        type="button"
                        onClick={() => navigate(`/pagos/${reservaId}`)}
                        className="px-4 py-2 rounded bg-gray-300 hover:bg-gray-400"
                    >
                        Cancelar
                    </button>
                    <button
                        type="submit"
                        disabled={loading}
                        className="px-4 py-2 rounded bg-green-600 text-white hover:bg-green-700"
                    >
                        {loading ? 'Registrando...' : 'Registrar pago'}
                    </button>
                </div>
            </form>
        </div>
    )
}
