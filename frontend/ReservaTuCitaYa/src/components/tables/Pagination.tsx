export function Pagination({ page, total, onChange }: { page: number; total: number; onChange: (page: number) => void }) {
  if (total <= 1) return null
  return <nav aria-label="Paginación"><ul className="pagination">
    <li className={`page-item ${page <= 1 ? 'disabled' : ''}`}><button className="page-link" onClick={() => onChange(page - 1)}>Anterior</button></li>
    <li className="page-item disabled"><span className="page-link">Página {page} de {total}</span></li>
    <li className={`page-item ${page >= total ? 'disabled' : ''}`}><button className="page-link" onClick={() => onChange(page + 1)}>Siguiente</button></li>
  </ul></nav>
}
