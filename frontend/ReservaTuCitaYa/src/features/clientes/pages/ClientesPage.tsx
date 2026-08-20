import { useEffect, useState } from 'react';
import type { ClienteListado } from '../types/Cliente';
import './ClientesPage.css';
import { ClienteRow } from '../components/ClienteRow';
import { useNavigate } from "react-router-dom";
import { ClienteDetalleModal } from '../components/ClienteDetalleModal';
import { ClienteEditarModal } from '../components/ClienteEditarModal';
import { EliminarClienteModal } from '../components/EliminarClienteModal';
import ConfirmarEstadoModal from '../components/ConfirmarEstadoModal';
import { Toast } from '../components/Toast';
import {
  listarClientes,
  cambiarEstadoCliente,
  eliminarCliente,
  actualizarCliente
} from "../../../api/clientesApi";


export default function ClientesPage() {
  const [clientes, setClientes] = useState<ClienteListado[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [clienteEditando, setClienteEditando] = useState<ClienteListado | null>(null);
  const [clienteEliminando, setClienteEliminando] = useState<ClienteListado | null>(null);
  const [clienteCambioEstado, setClienteCambioEstado] = useState<ClienteListado | null>(null);
  const [nuevoEstado, setNuevoEstado] = useState<boolean>(false);

  const [busqueda, setBusqueda] = useState("");
  const [estado, setEstado] = useState("");
  const [pagina, setPagina] = useState(1);
  const tamañoPagina = 5;

  const navigate = useNavigate();
  const [clienteSeleccionado, setClienteSeleccionado] = useState<ClienteListado | null>(null);
  const [toast, setToast] = useState<{ mensaje: string; tipo: "exito" | "error" | "info" } | null>(null);

  {/* Cargar clientes desde la API */ }
  useEffect(() => {
    async function fetchClientes() {
      try {
        const data = await listarClientes({ pagina: 1, tamañoPagina: 10 });
        setClientes(data.elementos);
      } catch (err) {
        setError("Error al cargar clientes");
      } finally {
        setLoading(false);
      }
    }
    fetchClientes();
  }, []);


  {/* Filtros */ }
  const filtrarClientes = () => {
    return clientes.filter(c =>
      (c.nombres.toLowerCase().includes(busqueda.toLowerCase()) ||
        c.apellidos.toLowerCase().includes(busqueda.toLowerCase()) ||
        c.numeroDocumento.includes(busqueda) ||
        c.correo.toLowerCase().includes(busqueda.toLowerCase())) &&
      (estado === "" || (estado === "activo" ? c.estado : !c.estado))
    );
  };

  {/* Cambio de estado */ }
  const solicitarCambioEstado = (cliente: ClienteListado) => {
    setClienteCambioEstado(cliente);
    setNuevoEstado(!cliente.estado);
  };

  const confirmarCambioEstado = async () => {
    if (!clienteCambioEstado) return;
    try {
      await cambiarEstadoCliente(clienteCambioEstado.id, nuevoEstado);
      setClientes(prev =>
        prev.map(c =>
          c.id === clienteCambioEstado.id ? { ...c, estado: nuevoEstado } : c
        )
      );
      setClienteCambioEstado(null);
      setToast({ mensaje: `Cliente ${nuevoEstado ? "activado" : "desactivado"} correctamente`, tipo: "exito" });
    } catch {
      setToast({ mensaje: "No se pudo cambiar el estado del cliente", tipo: "error" });
    }
  };


  {/* Eliminar cliente */ }
  const confirmarEliminar = async () => {
    if (!clienteEliminando) return;
    try {
      await eliminarCliente(clienteEliminando.id);
      setClientes(prev => prev.filter(c => c.id !== clienteEliminando.id));
      setClienteEliminando(null);
      setToast({ mensaje: "Cliente eliminado correctamente", tipo: "exito" });
    } catch {
      setToast({ mensaje: "No se pudo eliminar el cliente", tipo: "error" });
    }
  };


  {/* Guardar cliente editado */ }
  const guardarCliente = async (clienteActualizado: ClienteListado) => {
    try {
      const actualizado = await actualizarCliente(clienteActualizado.id, clienteActualizado);
      setClientes(prev =>
        prev.map(c => (c.id === actualizado.id ? actualizado : c))
      );
      setToast({ mensaje: "Cliente actualizado correctamente", tipo: "exito" });
    } catch {
      setToast({ mensaje: "No se pudo actualizar el cliente", tipo: "error" });
    }
  };


  {/* Paginación */ }
  const clientesFiltrados = filtrarClientes();
  const totalPaginas = Math.ceil(clientesFiltrados.length / tamañoPagina);
  const clientesPaginados = clientesFiltrados.slice(
    (pagina - 1) * tamañoPagina,
    pagina * tamañoPagina
  );

  if (loading) return <p>Cargando clientes...</p>;
  if (error) return <p>{error}</p>;

  return (
    <div className="clientes-container">
      <div className="clientes-header">
        <h1>Clientes</h1>
        <button
          className="btn-primary"
          onClick={() => navigate("/clientes/nuevo")}
        >
          + Nuevo cliente
        </button>
      </div>

      {/* Filtros */}
      <div className="clientes-filtros">
        <input
          type="text"
          placeholder="🔍︎ Nombre, documento o correo"
          className="input-busqueda"
          value={busqueda}
          onChange={e => {
            setBusqueda(e.target.value);
            setPagina(1);
          }}
        />
        <select
          className="select-estado"
          value={estado}
          onChange={e => {
            setEstado(e.target.value);
            setPagina(1);
          }}
        >
          <option value="">Todos</option>
          <option value="activo">Activos</option>
          <option value="inactivo">Inactivos</option>
        </select>
      </div>

      {/* Tabla */}
      <div className="tabla-wrapper">
        <table className="clientes-tabla">
          <thead>
            <tr>
              <th>Documento</th>
              <th>Cliente</th>
              <th>Teléfono</th>
              <th>Correo</th>
              <th>Última reserva</th>
              <th>Estado</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {clientesPaginados.map(c => (
              <ClienteRow
                key={c.id}
                cliente={c}
                onVerDetalle={() => setClienteSeleccionado(c)}
                onEditar={() => setClienteEditando(c)}
                onEliminar={() => setClienteEliminando(c)}
                onCambiarEstado={() => solicitarCambioEstado(c)}
              />
            ))}
          </tbody>
        </table>
      </div>

      {/* Paginación */}
      <div className="clientes-paginacion">
        <span>
          Mostrando {clientesPaginados.length} de {clientesFiltrados.length}
        </span>
        <div className="paginacion-botones">
          {Array.from({ length: totalPaginas }, (_, i) => (
            <button
              key={i + 1}
              className={pagina === i + 1 ? "pagina-activa" : ""}
              onClick={() => setPagina(i + 1)}
            >
              {i + 1}
            </button>
          ))}
        </div>
      </div>

      {/* Modales */}
      {clienteSeleccionado && (
        <ClienteDetalleModal
          cliente={clienteSeleccionado}
          onClose={() => setClienteSeleccionado(null)}
        />
      )}

      {clienteEditando && (
        <ClienteEditarModal
          cliente={clienteEditando}
          onClose={() => setClienteEditando(null)}
          onSave={guardarCliente}
        />
      )}

      {clienteEliminando && (
        <EliminarClienteModal
          cliente={clienteEliminando}
          onCancel={() => setClienteEliminando(null)}
          onConfirm={confirmarEliminar}
        />
      )}

      {clienteCambioEstado && (
        <ConfirmarEstadoModal
          cliente={clienteCambioEstado}
          nuevoEstado={nuevoEstado}
          onCancel={() => setClienteCambioEstado(null)}
          onConfirm={confirmarCambioEstado}
        />
      )}

      {toast && (
        <Toast
          mensaje={toast.mensaje}
          tipo={toast.tipo}
          onClose={() => setToast(null)}
        />
      )}
    </div>
  );
}
