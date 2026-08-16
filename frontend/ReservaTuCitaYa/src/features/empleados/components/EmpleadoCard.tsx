import { useEffect, useState } from "react";
import {
    EmpleadoDetalle,
    EmpleadoListado,
    SedeAsignada,
    ServicioAsignado,
} from "../types/Empleado";
import {
    obtenerServiciosProfesional,
    actualizarServiciosProfesional,
    obtenerSedesEmpleado,
    cambiarEstadoEmpleado,
    obtenerEmpleado,
    eliminarEmpleado,
} from "../../../api/empleadosApi";
import "./EmpleadoCard.css";
import EmpleadoDetalleModal from "./EmpleadoDetalleModal";
import EmpleadoEditarModal from "./EmpleadoEditarModal";

interface EmpleadoCardProps {
    empleado: EmpleadoListado;
    onToggleEstado: (id: string) => void;
    onEditar: (id: string) => void;
    onEliminar: (id: string) => void;
    onVerPerfil: (id: string) => void;
}

export default function EmpleadoCard({
    empleado,
    onToggleEstado,
    onEditar,
    onEliminar,
    onVerPerfil,
}: EmpleadoCardProps) {
    const [menuAbierto, setMenuAbierto] = useState(false);
    const [servicios, setServicios] = useState<ServicioAsignado[]>([]);
    const [sedes, setSedes] = useState<SedeAsignada[]>([]);
    const [confirmar, setConfirmar] = useState(false);
    const [mensaje, setMensaje] = useState("");
    const [detalleEmpleado, setDetalleEmpleado] = useState<EmpleadoDetalle | null>(null);
    const [loadingDetalle, setLoadingDetalle] = useState(false);
    const [empleadoEditar, setEmpleadoEditar] = useState<EmpleadoDetalle | null>(null);
    const [loadingEditar, setLoadingEditar] = useState(false);
    const [empleadoEliminar, setEmpleadoEliminar] = useState<EmpleadoDetalle | null>(null);
    const [mostrarExito, setMostrarExito] = useState(false);

    // 🔹 Cargar servicios y sedes desde el backend
    useEffect(() => {
        async function fetchServicios() {
            try {
                const data = await obtenerServiciosProfesional(empleado.id);
                setServicios(data);
            } catch (error) {
                console.error("Error al cargar servicios:", error);
            }
        }

        async function fetchSedes() {
            try {
                const data = await obtenerSedesEmpleado(empleado.id);
                setSedes(data);
            } catch (error) {
                console.error("Error al cargar sedes:", error);
            }
        }

        if (empleado.esProfesional) {
            fetchServicios();
        } else {
            setServicios([]); // asegura que aparezca “Sin servicios”
        }

        fetchSedes();
    }, [empleado.id, empleado.esProfesional]);

    // 🔹 Handler para actualizar servicios
    const handleEditarServicios = async (nuevosServicios: string[]) => {
        try {
            const actualizados = await actualizarServiciosProfesional(empleado.id, nuevosServicios);
            setServicios(actualizados);
        } catch (error) {
            console.error("Error al actualizar servicios:", error);
        }
    };

    // 🔹 Handler para cambiar estado
    const handleToggleEstado = async () => {
        try {
            const nuevoActivo = empleado.estado === "Activo" ? false : true;
            await cambiarEstadoEmpleado(empleado.id, nuevoActivo);

            onToggleEstado(empleado.id);

            setMensaje(`Estado cambiado a ${nuevoActivo ? "Activo" : "Inactivo"} correctamente.`);
            setTimeout(() => setMensaje(""), 3000);
        } catch (error) {
            console.error("Error al cambiar estado:", error);
            setMensaje("Error al actualizar estado.");
            setTimeout(() => setMensaje(""), 3000);
        } finally {
            setConfirmar(false);
        }
    };

    const handleVerPerfil = async () => {
        try {
            setLoadingDetalle(true);
            const data = await obtenerEmpleado(empleado.id); // ✅ llamada real
            setDetalleEmpleado(data);
        } catch (error) {
            console.error("Error al obtener detalle del empleado:", error);
        } finally {
            setLoadingDetalle(false);
        }
    };

    const handleEliminar = async () => {
        if (!empleadoEliminar) return;
        try {
            await eliminarEmpleado(empleadoEliminar.id); // ✅ DELETE real
            setEmpleadoEliminar(null);
            setMostrarExito(true);
            onEliminar(empleadoEliminar.id); // refresca listado
        } catch (error) {
            console.error("Error al eliminar empleado:", error);
        }
    };

    return (
        <div className="empleado-card">
            {/* Encabezado */}
            <div className="card-header">
                <div className="avatar">
                    {empleado.nombres[0]}
                    {empleado.apellidos[0]}
                </div>

                <div className="info">
                    <h3>
                        {empleado.nombres} {empleado.apellidos}
                    </h3>
                    {!empleado.esProfesional && <p>{empleado.cargo}</p>}
                    {empleado.esProfesional && empleado.especialidad && <p>{empleado.especialidad}</p>}
                </div>

                <div className="estado-container">
                    <button
                        className={`estado-toggle ${empleado.estado === "Activo" ? "activo" : "inactivo"}`}
                        onClick={() => setConfirmar(true)}
                    >
                        {empleado.estado}
                    </button>
                    {empleado.esProfesional && <span className="badge">Profesional</span>}
                </div>
            </div>

            {/* Cuerpo */}
            <div className="card-body">
                {/* 🔹 Sedes */}
                <div className="sede">
                    <strong>⚲ Sede{sedes.length > 0 ? "s asignadas:" : " no asignadas"}</strong>
                    <div className="sedes-lista">
                        {sedes.length > 0 ? (
                            sedes.map((s) => (
                                <span key={s.sedeId} className="badge-sede">
                                    {s.nombre}
                                </span>
                            ))
                        ) : (
                            <span className="badge-sede">Sin sede</span>
                        )}
                    </div>
                </div>

                {/* 🔹 Servicios */}
                {empleado.esProfesional && (
                    <div className="servicios">
                        <strong>Servicios asignados</strong>
                        <div className="servicios-lista">
                            {servicios.length > 0 ? (
                                servicios.map((s) => (
                                    <span key={s.servicioId} className="badge-servicio">
                                        {s.nombre}
                                    </span>
                                ))
                            ) : (
                                <span className="badge-servicio">Sin servicios</span>
                            )}
                        </div>
                    </div>
                )}
            </div>

            {/* Pie */}
            <div className="card-footer">
                <button className="btn-outline" onClick={handleVerPerfil}>
                    Ver perfil
                </button>

                <div className="menu">
                    <button className="menu-btn" onClick={() => setMenuAbierto(!menuAbierto)}>⋯</button>
                    {menuAbierto && (
                        <div className="menu-dropdown">
                            <button
                                onClick={async () => {
                                    try {
                                        setLoadingEditar(true);
                                        const detalle = await obtenerEmpleado(empleado.id); // ✅ llamada real
                                        setEmpleadoEditar(detalle);
                                    } catch (error) {
                                        console.error("Error al obtener detalle del empleado:", error);
                                    } finally {
                                        setLoadingEditar(false);
                                    }
                                }}
                            >
                                Editar
                            </button>

                            <button
                                onClick={async () => {
                                    try {
                                        const detalle = await obtenerEmpleado(empleado.id); // ✅ llamada real
                                        setEmpleadoEliminar(detalle);
                                    } catch (error) {
                                        console.error("Error al preparar eliminación:", error);
                                    }
                                }}
                            >
                                Eliminar
                            </button>
                        </div>
                    )}
                </div>
            </div>

            {/* Modal de confirmación */}
            {confirmar && (
                <div className="modal-confirmacion">
                    <p>¿Seguro que deseas cambiar el estado de {empleado.nombres}?</p>
                    <button className="btn-primary" onClick={handleToggleEstado}>Confirmar</button>
                    <button className="btn-outline" onClick={() => setConfirmar(false)}>Cancelar</button>
                </div>
            )}

            {/* Toast */}
            {mensaje && <div className="toast">{mensaje}</div>}

            {detalleEmpleado && (
                <EmpleadoDetalleModal
                    empleado={detalleEmpleado}
                    onClose={() => setDetalleEmpleado(null)}
                />
            )}

            {empleadoEditar && (
                <EmpleadoEditarModal
                    empleado={empleadoEditar}
                    onClose={() => setEmpleadoEditar(null)}
                    onGuardar={(actualizado) => {
                        console.log("Empleado actualizado:", actualizado);
                        setEmpleadoEditar(null);
                    }}
                />
            )}

            {empleadoEliminar && (
                <div className="modal-backdrop" onClick={() => setEmpleadoEliminar(null)}>
                    <div className="modal-card" onClick={(e) => e.stopPropagation()}>
                        <h3>¿Eliminar empleado?</h3>
                        <p>Esta acción dará de baja definitiva al empleado.</p>
                        <div className="modal-actions">
                            <button className="btn-danger" onClick={handleEliminar}>Eliminar</button>
                            <button className="btn-outline" onClick={() => setEmpleadoEliminar(null)}>Cancelar</button>
                        </div>
                    </div>
                </div>
            )}

            {mostrarExito && (
                <div className="modal-backdrop" onClick={() => setMostrarExito(false)}>
                    <div className="modal-card" onClick={(e) => e.stopPropagation()}>
                        <h3>Empleado eliminado</h3>
                        <p>El registro fue dado de baja correctamente.</p>
                        <div className="modal-actions">
                            <button className="btn-primary" onClick={() => setMostrarExito(false)}>Cerrar</button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}