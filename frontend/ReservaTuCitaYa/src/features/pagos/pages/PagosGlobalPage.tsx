import React, { useEffect, useState } from 'react'
import { listarPagos } from '../../../api/pagosApi'
import type { PagoDto } from '../types/Pago'
import { useNavigate } from 'react-router-dom'

export const PagosGlobalPage: React.FC = () => {
  const [pagos, setPagos] = useState<PagoDto[]>([])
  const [reservaFiltro, setReservaFiltro] = useState<string>('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const navigate = useNavigate()

  useEffect(() => {
    const fetchPagos = async () => {
      try {
        setLoading(true)
        const data = await listarPagos()
        setPagos(data)
      } catch (err: any) {
        setError(err.message ?? 'Error al cargar pagos')
      } finally {
        setLoading(false)
      }
    }
    fetchPagos()
  }, [])

  const pagosFiltrados = reservaFiltro
    ? pagos.filter((p) => p.reservaId === reservaFiltro)
    : pagos

  if (loading) return <p>Cargando pagos...</p>
  if (error) return <p className="text-red-600">{error}</p>

  return (
    <div className="p-6">
      <h2 className="text-xl font-bold mb-4">Gestión global de pagos</h2>

      {/* Filtro por reserva */}
      <div className="mb-4">
        <label className="block text-sm font-medium mb-1">Filtrar por reserva</label>
        <input
          type="text"
          value={reservaFiltro}
          onChange={(e) => setReservaFiltro(e.target.value)}
          placeholder="Ingrese código de reserva..."
          className="border rounded p-2 w-64"
        />
      </div>

      {/* Tabla de pagos */}
      <table className="w-full border-collapse text-sm">
        <thead>
          <tr className="bg-gray-100 text-left">
            <th className="p-2 border">Código Pago</th>
            <th className="p-2 border">Reserva</th>
            <th className="p-2 border">Cliente</th>
            <th className="p-2 border">Monto</th>
            <th className="p-2 border">Estado</th>
            <th className="p-2 border">Acciones</th>
          </tr>
        </thead>
        <tbody>
          {pagosFiltrados.map((pago) => (
            <tr key={pago.id} className="hover:bg-gray-50">
              <td className="p-2 border">{pago.codigo}</td>
              <td className="p-2 border">{pago.reservaId}</td>
              <td className="p-2 border">{pago.clienteNombre}</td>
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
                  onClick={() => navigate(`/pagos/${pago.reservaId}`)}
                >
                  Ver detalle
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
