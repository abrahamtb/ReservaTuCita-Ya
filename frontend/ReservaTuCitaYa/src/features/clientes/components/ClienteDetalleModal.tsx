import "./ClienteDetalleModal.css";
import type { ClienteListado } from "../types/Cliente";

interface Props {
  cliente: ClienteListado;
  onClose: () => void;
}

export function ClienteDetalleModal({ cliente, onClose }: Props) {
  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal-card" onClick={e => e.stopPropagation()}>
        <h2>{cliente.nombres} {cliente.apellidos}</h2>
        <p><strong>Documento:</strong> {cliente.tipoDocumento} {cliente.numeroDocumento}</p>
        <p><strong>Correo:</strong> {cliente.correo}</p>
        <p><strong>Teléfono:</strong> {cliente.telefono}</p>
        <p><strong>Estado:</strong> {cliente.estado ? "Activo" : "Inactivo"}</p>
        <hr />
        <p><strong>Última reserva:</strong> —</p>
        <p><strong>Observaciones:</strong> Prefiere confirmaciones por WhatsApp.</p>

        <div className="modal-actions">
          <button className="btn-secondary" onClick={onClose}>Cerrar</button>
        </div>
      </div>
    </div>
  );
}
