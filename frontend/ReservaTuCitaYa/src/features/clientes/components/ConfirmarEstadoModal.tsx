import "./ConfirmarEstadoModal.css";
import type { ClienteListado } from "../types/Cliente";

interface Props {
  cliente: ClienteListado;
  nuevoEstado: boolean;
  onCancel: () => void;
  onConfirm: () => void;
}

export default function ConfirmarEstadoModal({ cliente, nuevoEstado, onCancel, onConfirm }: Props) {
  return (
    <div className="modal-backdrop" onClick={onCancel}>
      <div className="modal-card" onClick={e => e.stopPropagation()}>
        <h2>{nuevoEstado ? "Activar cliente" : "Desactivar cliente"}</h2>
        <p>
          ¿Deseas {nuevoEstado ? "activar" : "desactivar"} la cuenta de{" "}
          <strong>{cliente.nombres} {cliente.apellidos}</strong>?
        </p>
        <p className="warning">
          Esta acción modificará el estado del cliente en el sistema.
        </p>
        <div className="modal-actions">
          <button className="btn-secondary" onClick={onCancel}>Cancelar</button>
          <button className="btn-primary" onClick={onConfirm}>
            {nuevoEstado ? "Activar" : "Desactivar"}
          </button>
        </div>
      </div>
    </div>
  );
}
