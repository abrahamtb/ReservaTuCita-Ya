import "./EmpleadoDetalleModal.css";
import { EmpleadoDetalle } from "../types/Empleado";

interface Props {
  empleado: EmpleadoDetalle;
  onClose: () => void;
}

export default function EmpleadoDetalleModal({ empleado, onClose }: Props) {
  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal-card" onClick={(e) => e.stopPropagation()}>
        <h2>{empleado.nombres} {empleado.apellidos}</h2>
        <p><strong>Documento:</strong> {empleado.tipoDocumento} {empleado.numeroDocumento}</p>
        <p><strong>Correo:</strong> {empleado.correo}</p>
        <p><strong>Teléfono:</strong> {empleado.telefono}</p>
        <p><strong>Cargo:</strong> {empleado.cargo}</p>
        {empleado.esProfesional && empleado.especialidad && (
          <p><strong>Especialidad:</strong> {empleado.especialidad}</p>
        )}
        <p><strong>Estado:</strong> {empleado.estado}</p>

        {/* 🔹 Sedes */}
        <div className="sedes">
          <strong>Sedes asignadas:</strong>
          {empleado.sedes.length > 0 ? (
            empleado.sedes.map((s) => (
              <span key={s.sedeId} className="badge-sede">{s.nombre}</span>
            ))
          ) : (
            <span className="badge-sede">Sin sede</span>
          )}
        </div>

        {/* 🔹 Servicios */}
        {empleado.esProfesional && (
          <div className="servicios">
            <strong>Servicios asignados:</strong>
            {empleado.servicios.length > 0 ? (
              empleado.servicios.map((s) => (
                <span key={s.servicioId} className="badge-servicio">{s.nombre}</span>
              ))
            ) : (
              <span className="badge-servicio">Sin servicios</span>
            )}
          </div>
        )}

        <button className="btn-primary" onClick={onClose}>Cerrar</button>
      </div>
    </div>
  );
}
