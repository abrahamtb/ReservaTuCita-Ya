import "./EliminarClienteModal.css";
import type { ClienteListado } from "../types/Cliente";

interface Props {
  cliente: ClienteListado;
  onCancel: () => void;
  onConfirm: () => void;
}

export function EliminarClienteModal({ cliente, onCancel, onConfirm }: Props) {
  return (
    <div className="modal-backdrop" onClick={onCancel}>
      <div className="modal-card" onClick={e => e.stopPropagation()}>
        <h2>Eliminar cliente</h2>
        <p>¿Estás seguro de que deseas eliminar a <strong>{cliente.nombres} {cliente.apellidos}</strong>?</p>
        <p className="warning">Esta acción no se puede deshacer.</p>
        <div className="modal-actions">
          <button className="btn-secondary" onClick={onCancel}>Cancelar</button>
          <button className="btn-danger" onClick={onConfirm}>Eliminar</button>
        </div>
      </div>
    </div>
  );
}
