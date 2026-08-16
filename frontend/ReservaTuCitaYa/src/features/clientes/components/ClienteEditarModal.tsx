import "./ClienteEditarModal.css";
import type { ClienteListado } from "../types/Cliente";
import { useState } from "react";

interface Props {
  cliente: ClienteListado;
  onClose: () => void;
  onSave: (clienteActualizado: ClienteListado) => void;
}

export function ClienteEditarModal({ cliente, onClose, onSave }: Props) {
  const [formData, setFormData] = useState<ClienteListado>(cliente);

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSave(formData);
    onClose();
  };

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal-card" onClick={e => e.stopPropagation()}>
        <h2>Editar cliente</h2>
        <form onSubmit={handleSubmit} className="form-grid">
          <div>
            <label>Nombres *</label>
            <input
              name="nombres"
              value={formData.nombres}
              onChange={handleChange}
            />
          </div>
          <div>
            <label>Apellidos *</label>
            <input
              name="apellidos"
              value={formData.apellidos}
              onChange={handleChange}
            />
          </div>
          <div>
            <label>Teléfono *</label>
            <input
              name="telefono"
              value={formData.telefono}
              onChange={handleChange}
            />
          </div>
          <div>
            <label>Correo *</label>
            <input
              type="email"
              name="correo"
              value={formData.correo}
              onChange={handleChange}
            />
          </div>
          <div>
            <label>Estado</label>
            <select
              name="estado"
              value={formData.estado ? "activo" : "inactivo"}
              onChange={e =>
                setFormData(prev => ({
                  ...prev,
                  estado: e.target.value === "activo"
                }))
              }
            >
              <option value="activo">Activo</option>
              <option value="inactivo">Inactivo</option>
            </select>
          </div>

          <div className="form-actions">
            <button type="button" className="btn-secondary" onClick={onClose}>
              Cancelar
            </button>
            <button type="submit" className="btn-primary">
              Guardar cambios
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
