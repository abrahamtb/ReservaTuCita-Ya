import { Link } from 'react-router-dom'
export function AccessDeniedPage() { return <main className="container py-5"><div className="alert alert-warning"><h1 className="h3">Acceso denegado</h1><p>Tu sesión no tiene permisos administrativos.</p><Link to="/login">Volver</Link></div></main> }
