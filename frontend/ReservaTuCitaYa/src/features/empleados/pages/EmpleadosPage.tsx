import { useEffect, useState } from "react";
import "./EmpleadosPage.css";
import { EmpleadoListado, EmpleadoFiltros, PaginaResultado } from "../types/Empleado";
import EmpleadoCard from "../components/EmpleadoCard";
import { listarEmpleados } from "../../../api/empleadosApi";
import { listSedes } from "../../../api/sedesApi";
import { listServices } from "../../../api/serviciosApi";
import { useNavigate } from "react-router-dom";


export default function EmpleadosPage() {
  const [filtros, setFiltros] = useState<EmpleadoFiltros>({
    busqueda: "",
    estado: "Activo",
    sedeId: "",
    servicioId: "",
    esProfesional: false,
    pagina: 1,
    tamañoPagina: 12,
  });

  const [data, setData] = useState<PaginaResultado<EmpleadoListado> | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [sedes, setSedes] = useState<{ id: string; nombre: string }[]>([]);
  const [servicios, setServicios] = useState<{ id: string; nombre: string }[]>([]);

  useEffect(() => {
    async function cargarFiltros() {
      try {
        const organizationId = "org-001";
        const sedesData = await listSedes(organizationId, { estado: "Activos" });
        const serviciosData = await listServices(organizationId, { estado: "Activos" });

        setSedes(sedesData.registros ?? sedesData);
        setServicios(serviciosData.registros ?? serviciosData);
      } catch (error) {
        console.error("Error al cargar sedes/servicios:", error);
        setError("No se pudieron cargar los filtros.");
      }
    }
    cargarFiltros();
  }, []);


  useEffect(() => {
    async function cargarEmpleados() {
      setLoading(true);
      setError(null);
      try {
        const empleadosData = await listarEmpleados(filtros);
        setData(empleadosData);
      } catch (error) {
        console.error("Error al cargar empleados:", error);
        setError("No se pudieron cargar los empleados.");
      } finally {
        setLoading(false);
      }
    }
    cargarEmpleados();
  }, [filtros]);

  const navigate = useNavigate();


  const handleToggleEstado = (id: string) => {
    setData((prev) => {
      if (!prev) return prev;
      const registros = prev.registros.map((e) =>
        e.id === id ? { ...e, estado: e.estado === "Activo" ? "Inactivo" : "Activo" } : e
      );
      return { ...prev, registros };
    });
  };

  const handleEditar = (id: string) => {
    alert(`Editar empleado con id: ${id}`);
  };

  const handleEliminar = (id: string) => {
    setData((prev) => {
      if (!prev) return prev;
      const registros = prev.registros.filter((e) => e.id !== id);
      return { ...prev, registros };
    });
  };

  const handleVerPerfil = (id: string) => {
    alert(`Ver perfil de empleado con id: ${id}`);
  };

  return (
    <div className="empleados-page">
      <header className="page-header">
        <h1>Empleados</h1>
        <button
  className="btn-primary"
  onClick={() => navigate("/empleados/nuevo")}
>
  + Nuevo empleado
</button>

      </header>

      {/* Filtros */}
      <section className="filters">
        <div className="filter-group">
          <label>Sede</label>
          <select
            value={filtros.sedeId}
            onChange={(e) => setFiltros({ ...filtros, sedeId: e.target.value })}
          >
            <option value="">Todas</option>
            {sedes.map((s) => (
              <option key={s.id} value={s.id}>
                {s.nombre}
              </option>
            ))}
          </select>
        </div>

        <div className="filter-group">
          <label>Servicio</label>
          <select
            value={filtros.servicioId}
            onChange={(e) => setFiltros({ ...filtros, servicioId: e.target.value })}
          >
            <option value="">Todos</option>
            {servicios.map((s) => (
              <option key={s.id} value={s.id}>
                {s.nombre}
              </option>
            ))}
          </select>
        </div>

        <div className="filter-group">
          <label>Estado</label>
          <select
            value={filtros.estado}
            onChange={(e) => setFiltros({ ...filtros, estado: e.target.value })}
          >
            <option value="Activo">Activo</option>
            <option value="Inactivo">Inactivo</option>
          </select>
        </div>

        <div className="filter-group checkbox">
          <label>
            <input
              type="checkbox"
              checked={filtros.esProfesional}
              onChange={(e) => setFiltros({ ...filtros, esProfesional: e.target.checked })}
            />
            Solo profesionales
          </label>
        </div>
      </section>

      {/* Listado */}
      <section className="cards-container">
        {loading && <p>Cargando empleados...</p>}
        {error && <p className="error">{error}</p>}
        {!loading && data && data.registros.length === 0 && <p>No se encontraron empleados.</p>}
        {!loading && data && data.registros.length > 0 && (
          <div className="cards-grid">
            {data.registros.map((empleado) => (
              <EmpleadoCard
                key={empleado.id}
                empleado={empleado}
                onToggleEstado={handleToggleEstado}
                onEditar={handleEditar}
                onEliminar={handleEliminar}
                onVerPerfil={handleVerPerfil}
              />
            ))}
          </div>
        )}
      </section>
    </div>
  );
}
