import { useParams } from 'react-router-dom'
import { BloqueosRecursoPanel } from './BloqueosRecursoPanel'
import { RecursoDetailPage } from './RecursoPages'

export function RecursoDetailWithBloqueosPage() {
  const { id = '' } = useParams()
  return <>
    <RecursoDetailPage />
    {id ? <BloqueosRecursoPanel recursoId={id} /> : null}
  </>
}
