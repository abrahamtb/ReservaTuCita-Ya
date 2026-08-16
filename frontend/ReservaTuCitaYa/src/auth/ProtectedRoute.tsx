import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "./useAuth";

export function ProtectedRoute() {
  const { user, loading } = useAuth();

  // ⚙️ Modo desarrollo: si está activado en .env, siempre deja pasar
  if (import.meta.env.VITE_DISABLE_AUTH === "true") {
    return <Outlet />;
  }

  // Mientras carga la sesión, puedes mostrar un loader
  if (loading) {
    return <div className="app-loading">Comprobando sesión...</div>;
  }

  // Si hay usuario autenticado, deja pasar; si no, redirige al login
  return user ? <Outlet /> : <Navigate to="/login" replace />;
}
