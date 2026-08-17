import React, { useState } from 'react'
import type { PagoDto } from '../types/Pago'
import { AnularPagoModal } from './AnularPagoModal'
import { PagoDetalleModal } from './PagoDetalleModal'
import { EditarPagoModal } from './EditarPagoModal'
import { ReembolsoPagoModal } from './ReembolsoPagoModal'

interface Props {
    pagos: PagoDto[]
    onRefresh: () => void
}

export const PagosTable: React.FC<Props> = ({ pagos, onRefresh }) => {
    const [selectedPago, setSelectedPago] = useState<PagoDto | null>(null)
    const [detallePago, setDetallePago] = useState<PagoDto | null>(null)
    const [editarPago, setEditarPago] = useState<PagoDto | null>(null)
    const [reembolsoPago, setReembolsoPago] = useState<PagoDto | null>(null)
    if (!pagos || pagos.length === 0) {
        return <p className="text-gray-500">Aún no se han registrado pagos para esta reserva.</p>
    }

    return (
        <div className="mt-4">
            <table className="w-full border-collapse text-sm">
                <thead>
                    <tr className="bg-gray-100 text-left">
                        <th className="p-2 border">Código</th>
                        <th className="p-2 border">Fecha</th>
                        <th className="p-2 border">Método</th>
                        <th className="p-2 border">N° Operación</th>
                        <th className="p-2 border">Monto</th>
                        <th className="p-2 border">Estado</th>
                        <th className="p-2 border">Acciones</th>
                    </tr>
                </thead>
                <tbody>
                    {pagos.map((pago) => (
                        <tr key={pago.id} className="hover:bg-gray-50">
                            <td className="p-2 border">{pago.codigo}</td>
                            <td className="p-2 border">{new Date(pago.fechaPago).toLocaleDateString()}</td>
                            <td className="p-2 border">{pago.metodoPago}</td>
                            <td className="p-2 border">{pago.numeroOperacion || '-'}</td>
                            <td className="p-2 border">S/ {pago.monto.toFixed(2)}</td>
                            <td className="p-2 border">
                                {pago.estadoMovimiento === 'Anulado' ? (
                                    <span className="bg-red-500 text-white px-2 py-1 rounded">Anulado</span>
                                ) : (
                                    <span className="bg-green-500 text-white px-2 py-1 rounded">Registrado</span>
                                )}
                            </td>
                            <td className="p-2 border">
                                <button
                                    className="text-blue-600 hover:underline"
                                    onClick={() => setDetallePago(pago)}
                                >
                                    Ver detalle
                                </button>
                                {pago.estadoMovimiento !== 'Anulado' && (
                                    <>
                                        <button
                                            className="text-red-600 hover:underline ml-2"
                                            onClick={() => setSelectedPago(pago)}
                                        >
                                            Anular
                                        </button>
                                        <button
                                            className="text-yellow-600 hover:underline ml-2"
                                            onClick={() => setEditarPago(pago)}
                                        >
                                            Editar
                                        </button>
                                        <button
                                            className="text-purple-600 hover:underline ml-2"
                                            onClick={() => setReembolsoPago(pago)}
                                        >
                                            Reembolsar
                                        </button>
                                    </>
                                )}
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>

            {selectedPago && (
                <AnularPagoModal
                    pago={selectedPago}
                    onClose={() => setSelectedPago(null)}
                    onSuccess={onRefresh} // refresca datos al éxito
                />
            )}

            {detallePago && (
                <PagoDetalleModal
                    pago={detallePago}
                    onClose={() => setDetallePago(null)}
                />
            )}
            {editarPago && (
                <EditarPagoModal
                    pago={editarPago}
                    onClose={() => setEditarPago(null)}
                    onSuccess={onRefresh}
                />
            )}
            {reembolsoPago && (
  <ReembolsoPagoModal
    pago={reembolsoPago}
    onClose={() => setReembolsoPago(null)}
    onSuccess={onRefresh}
  />
)}
        </div>
    )
}
