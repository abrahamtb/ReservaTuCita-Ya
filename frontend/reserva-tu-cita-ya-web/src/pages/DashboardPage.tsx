import { Link } from 'react-router-dom'
export function DashboardPage() { return <><h1>Panel administrativo</h1><p className="text-secondary">Gestiona las organizaciones y sus sedes, categorías y servicios.</p><Link className="btn btn-primary" to="/organizaciones">Ver organizaciones</Link></> }
