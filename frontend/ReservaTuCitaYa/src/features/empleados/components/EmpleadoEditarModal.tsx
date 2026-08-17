import { useState, useEffect } from "react";
import "./EmpleadoEditarModal.css";
import { EmpleadoDetalle, SedeAsignada, ServicioAsignado } from "../types/Empleado";
import { actualizarEmpleado, actualizarServiciosProfesional, actualizarSedesEmpleado, obtenerSedesEmpleado, obtenerServiciosProfesional } from "../../../api/empleadosApi";
import { listSedes } from "../../../api/sedesApi";
import { listServices } from "../../../api/serviciosApi";

interface Props {
    empleado: EmpleadoDetalle;
    onClose: () => void;
    onGuardar: (actualizado: EmpleadoDetalle) => void;
}

export default function EmpleadoEditarModal({ empleado, onClose, onGuardar }: Props) {
    const [formData, setFormData] = useState<EmpleadoDetalle>(empleado);
    const [errores, setErrores] = useState<{ [key: string]: string }>({});
    const [todasSedes, setTodasSedes] = useState<SedeAsignada[]>([]);
    const [todosServicios, setTodosServicios] = useState<ServicioAsignado[]>([]);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        async function fetchData() {
            try {
                const organizationId = "org-001";
                const sedesData = await listSedes(organizationId, { estado: "Activos" });
                setTodasSedes(
                    (sedesData.registros ?? sedesData).map((sede: any) => ({
                        sedeId: sede.id,
                        nombre: sede.nombre,
                        activa: formData.sedes.some(sel => sel.sedeId === sede.id && sel.activa),
                    }))
                );

                const serviciosData = await listServices(organizationId, { estado: "Activos" });
                setTodosServicios(
                    (serviciosData.registros ?? serviciosData).map((servicio: any) => ({
                        servicioId: servicio.id,
                        nombre: servicio.nombre,
                        activo: formData.servicios.some(sel => sel.servicioId === servicio.id && sel.activo),
                    }))
                );

            } catch (error) {
                console.error("Error al cargar sedes/servicios:", error);
            }
        }
        fetchData();
    }, []);


    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const { name, value, type } = e.target;
        const checked = (e.target as HTMLInputElement).checked;
        setFormData((prev) => ({
            ...prev,
            [name]: type === "checkbox" ? checked : value,
        }));
    };

    const validarFormulario = (): boolean => {
        const nuevosErrores: { [key: string]: string } = {};

        if (!formData.nombres.trim()) nuevosErrores.nombres = "El nombre es obligatorio.";
        if (!formData.apellidos.trim()) nuevosErrores.apellidos = "El apellido es obligatorio.";
        if (!formData.numeroDocumento.trim()) nuevosErrores.numeroDocumento = "El número de documento es obligatorio.";
        if (formData.tipoDocumento === "DNI" && formData.numeroDocumento.length !== 8)
            nuevosErrores.numeroDocumento = "El DNI debe tener 8 dígitos.";
        if (!formData.correo.trim()) nuevosErrores.correo = "El correo es obligatorio.";
        if (!formData.cargo.trim()) nuevosErrores.cargo = "El cargo es obligatorio.";
        if (formData.esProfesional && !formData.especialidad?.trim())
            nuevosErrores.especialidad = "La especialidad es obligatoria para profesionales.";
        setErrores(nuevosErrores);
        return Object.keys(nuevosErrores).length === 0;
    };

    const handleGuardar = async () => {
        if (!validarFormulario()) return;
        setLoading(true);
        try {
            const actualizado = await actualizarEmpleado(formData);
            const sedesSeleccionadas = todasSedes.filter(s =>
                formData.sedes.some(sel => sel.sedeId === s.sedeId && sel.activa)
            ).map(s => s.sedeId);
            await actualizarSedesEmpleado(formData.id, sedesSeleccionadas);

            if (formData.esProfesional) {
                const serviciosSeleccionados = todosServicios.filter(s =>
                    formData.servicios.some(sel => sel.servicioId === s.servicioId && sel.activo)
                ).map(s => s.servicioId);
                await actualizarServiciosProfesional(formData.id, serviciosSeleccionados);
            }

            onGuardar(actualizado);
            onClose();
        } catch (error) {
            console.error("Error al guardar cambios:", error);
            setErrores({ general: "No se pudo guardar los cambios. Intenta nuevamente." });
        } finally {
            setLoading(false);
        }
    };

    const toggleSede = (sedeId: string) => {
        setFormData((prev) => ({
            ...prev,
            sedes: prev.sedes.map((s) =>
                s.sedeId === sedeId ? { ...s, activa: !s.activa } : s
            ),
        }));
    };

    const toggleServicio = (servicioId: string) => {
        setFormData((prev) => ({
            ...prev,
            servicios: prev.servicios.map((s) =>
                s.servicioId === servicioId ? { ...s, activo: !s.activo } : s
            ),
        }));
    };

    return (
        <div className="modal-backdrop" onClick={onClose}>
            <div className="modal-card" onClick={(e) => e.stopPropagation()}>
                <h2>Editar empleado</h2>

                <div className="form-grid">
                    {/* Identificación */}
                    <div className="input-group">
                        <label>Tipo de documento</label>
                        <select name="tipoDocumento" value={formData.tipoDocumento} onChange={handleChange}>
                            <option value="DNI">DNI</option>
                            <option value="CE">Carnet de extranjería</option>
                            <option value="PAS">Pasaporte</option>
                        </select>
                    </div>

                    <div className="input-group">
                        <label>Número de documento</label>
                        <input name="numeroDocumento" value={formData.numeroDocumento} onChange={handleChange} />
                        {errores.numeroDocumento && <p className="error-text">{errores.numeroDocumento}</p>}
                    </div>

                    {/* Datos personales */}
                    <div className="input-group">
                        <label>Nombres</label>
                        <input name="nombres" value={formData.nombres} onChange={handleChange} />
                        {errores.nombres && <p className="error-text">{errores.nombres}</p>}
                    </div>

                    <div className="input-group">
                        <label>Apellidos</label>
                        <input name="apellidos" value={formData.apellidos} onChange={handleChange} />
                        {errores.apellidos && <p className="error-text">{errores.apellidos}</p>}
                    </div>

                    <div className="input-group">
                        <label>Correo</label>
                        <input name="correo" value={formData.correo} onChange={handleChange} />
                        {errores.correo && <p className="error-text">{errores.correo}</p>}
                    </div>

                    <div className="input-group">
                        <label>Teléfono</label>
                        <input name="telefono" value={formData.telefono} onChange={handleChange} />
                    </div>

                    {/* Laborales */}
                    <div className="input-group">
                        <label>Cargo</label>
                        <input name="cargo" value={formData.cargo} onChange={handleChange} />
                        {errores.cargo && <p className="error-text">{errores.cargo}</p>}
                    </div>

                    <div className="input-group">
                        <label>Especialidad</label>
                        <input name="especialidad" value={formData.especialidad ?? ""} onChange={handleChange} />
                        {errores.especialidad && <p className="error-text">{errores.especialidad}</p>}
                    </div>

                    <div className="input-group">
                        <label>Estado</label>
                        <select name="estado" value={formData.estado} onChange={handleChange}>
                            <option value="Activo">Activo</option>
                            <option value="Inactivo">Inactivo</option>
                        </select>
                    </div>

                    <div className="input-group">
                        <label>
                            <input
                                type="checkbox"
                                name="esProfesional"
                                checked={formData.esProfesional}
                                onChange={handleChange}
                            />Es profesional
                        </label>
                    </div>

                    {/* Asignaciones */}
                    <div className="asignaciones">
                        <strong>Sedes disponibles</strong>
                        <div className="chips">
                            {todasSedes.map((s) => (
                                <label key={s.sedeId} className="chip">
                                    <input
                                        type="checkbox"
                                        checked={formData.sedes.some(sel => sel.sedeId === s.sedeId && sel.activa)}
                                        onChange={() => toggleSede(s.sedeId)}
                                    />
                                    {s.nombre}
                                </label>
                            ))}
                        </div>

                        {formData.esProfesional && (
                            <>
                                <strong>Servicios disponibles</strong>
                                <div className="chips">
                                    {todosServicios.map((s) => (
                                        <label key={s.servicioId} className="chip">
                                            <input
                                                type="checkbox"
                                                checked={formData.servicios.some(sel => sel.servicioId === s.servicioId && sel.activo)}
                                                onChange={() => toggleServicio(s.servicioId)}
                                            />
                                            {s.nombre}
                                        </label>
                                    ))}
                                </div>
                            </>
                        )}
                    </div>
                </div>

                {/* Error general */}
                {errores.general && (
                    <p className="error-text">{errores.general}</p>
                )}

                {/* Botones */}
                <div className="modal-actions">
                    <button className="btn-primary" onClick={handleGuardar} disabled={loading}>
                        {loading ? "Guardando..." : "Guardar cambios"}
                    </button>

                    <button className="btn-outline" onClick={onClose}>Cancelar</button>
                </div>
            </div>
        </div>
    );
}