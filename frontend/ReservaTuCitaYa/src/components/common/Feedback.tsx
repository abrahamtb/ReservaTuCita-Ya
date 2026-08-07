import { ApiError } from '../../api/apiClient'

export function Loading() { return <div className="alert alert-light border">Cargando…</div> }
export function Empty({ message = 'No se encontraron resultados.' }: { message?: string }) {
  return <div className="alert alert-secondary">{message}</div>
}
export function ErrorAlert({ error }: { error: unknown }) {
  const message = error instanceof ApiError ? error.message : error instanceof Error ? error.message : 'Ocurrió un error inesperado.'
  return <div className="alert alert-danger" role="alert">{message}</div>
}
export function SuccessAlert({ message }: { message?: string }) {
  return message ? <div className="alert alert-success" role="status">{message}</div> : null
}
export function StatusBadge({ active }: { active: boolean }) {
  return <span className={`badge ${active ? 'text-bg-success' : 'text-bg-secondary'}`}>{active ? 'Activo' : 'Inactivo'}</span>
}
