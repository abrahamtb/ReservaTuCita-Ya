import React, { useEffect, useState } from 'react'
import { obtenerResumenPagoReserva } from '../../../api/pagosApi'
import type { ResumenPagoReserva } from '../types/Pago'
import { ResumenPagoCard } from '../components/ResumenPagoCard'
import { PagosTable } from '../components/PagosTable'
import { useParams } from 'react-router-dom'
import { useLocation } from 'react-router-dom'
import { useNavigate } from 'react-router-dom'

export const PagosReservaPage: React.FC = () => {
    const { reservaId } = useParams<{ reservaId: string }>()
    const [resumen, setResumen] = useState<ResumenPagoReserva | null>(null)
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState<string | null>(null)
    const navigate = useNavigate()
    const location = useLocation()

    useEffect(() => {
        if (!reservaId) return
        const fetchResumen = async () => {
            try {
                setLoading(true)
                const data = await obtenerResumenPagoReserva(reservaId!)
                setResumen(data)
            } catch (err: any) {
                setError(err.message ?? 'Error al cargar resumen')
            } finally {
                setLoading(false)
            }
        }
        fetchResumen()

        if (location.state?.refresh) {
            fetchResumen()
        }
    }, [reservaId])

    if (loading) return <p>Cargando resumen...</p>
    if (error) return <p className="text-red-600">{error}</p>
    if (!resumen) return <p>No se encontró información de pagos.</p>

    return (
        <div className="p-6">

            <div className="mb-4">
                <button
                    onClick={() => navigate('/pagos')}
                    className="bg-gray-500 text-white px-4 py-2 rounded hover:bg-gray-600"
                >
                    ← Volver a pagos globales
                </button>
            </div>



            <ResumenPagoCard
                resumen={resumen}
                onRegistrarPago={() => navigate(`/pagos/${resumen.codigoReserva}/registrar`)}
            />

            <h3 className="text-lg font-semibold mt-6 mb-2">Historial de pagos</h3>
            <PagosTable pagos={resumen.pagos ?? []}
                onRefresh={() => {
                    const fetchResumen = async () => {
                        try {
                            const data = await obtenerResumenPagoReserva(reservaId!)
                            setResumen(data)
                        } catch (err: any) {
                            setError(err.message ?? 'Error al actualizar resumen')
                        }
                    }
                    fetchResumen()
                }}
            />
        </div>
    )
}
