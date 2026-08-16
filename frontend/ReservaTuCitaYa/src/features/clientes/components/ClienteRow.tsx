import type { ClienteListado } from '../types/Cliente';
import { useState } from 'react';

interface Props {
  cliente: ClienteListado;
  onVerDetalle: () => void;
}

interface Props {
  cliente: ClienteListado;
  onVerDetalle: () => void;
  onEditar: () => void;
}

interface Props {
  cliente: ClienteListado;
  onVerDetalle: () => void;
  onEditar: () => void;
  onEliminar: () => void; // 🔹 nuevo
}

interface Props {
  cliente: ClienteListado;
  onVerDetalle: () => void;
  onEditar: () => void;
  onEliminar: () => void;
  onCambiarEstado: () => void; // 🔹 nuevo
}

interface Props {
  cliente: ClienteListado;
  onVerDetalle: () => void;
  onEditar: () => void;
  onEliminar: () => void;
  onCambiarEstado: () => void;
}


export function ClienteRow({ cliente, onVerDetalle, onEditar, onEliminar, onCambiarEstado }: Props) {
  const [showMenu, setShowMenu] = useState(false);

  const editarCliente = () => {
    alert(`Editar cliente ${cliente.nombres}`);
  };

  const eliminarCliente = () => {
    if (confirm(`¿Eliminar cliente ${cliente.nombres}?`)) {
      alert("Cliente eliminado (simulado)");
    }
  };

  return (
    <tr className="cliente-row">
      <td>{cliente.tipoDocumento} {cliente.numeroDocumento}</td>
      <td>
        <div className="cliente-info">
          <div className="avatar">
            {cliente.nombres[0]}{cliente.apellidos[0]}
          </div>
          <div>
            <p className="cliente-nombre">{cliente.nombres} {cliente.apellidos}</p>
            <p className="cliente-detalle">Cliente frecuente</p>
          </div>
        </div>
      </td>
      <td>{cliente.telefono}</td>
      <td>{cliente.correo}</td>
      <td></td>
      <td>
        <button
          className={cliente.estado ? "estado-activo" : "estado-inactivo"}
          onClick={onCambiarEstado}
        >
          {cliente.estado ? "Activo" : "Inactivo"}
        </button>
      </td>

      <td style={{ position: 'absolute' }}>
        <button
          className="acciones-btn"
          onClick={() => setShowMenu(!showMenu)}
        >
          ...
        </button>
        {showMenu && (
          <div className="acciones-menu">
            <button onClick={onVerDetalle}>Ver detalle</button>
            <button onClick={onEditar}>Editar</button>
            <button className="danger" onClick={onEliminar}>Eliminar</button>
          </div>
        )}
      </td>
    </tr>
  );
}