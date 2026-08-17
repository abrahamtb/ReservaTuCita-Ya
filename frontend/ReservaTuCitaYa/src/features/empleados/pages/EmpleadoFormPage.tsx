import { useState, useEffect } from "react";
import { CrearEmpleadoRequest, SedeAsignada, ServicioAsignado } from "../types/Empleado";
import { crearEmpleado } from "../../../api/empleadosApi";
import { listSedes } from "../../../api/sedesApi";
import { listServices } from "../../../api/serviciosApi";
import "./EmpleadoFormPage.css";
import { useNavigate } from "react-router-dom";

export default function EmpleadoFormPage() {
    const [formData, setFormData] = useState<CrearEmpleadoRequest>({
        nombres: "",
        apellidos: "",
        tipoDocumento: "DNI",
        numeroDocumento: "",
        correo: "",
        telefono: "",
        cargo: "",
        especialidad: "",
        esProfesional: false,
        estado: "Activo",
        sedes: [],
        servicios: []
    });

    const [errores, setErrores] = useState<{ [key: string]: string }>({});
    const [todasSedes, setTodasSedes] = useState<SedeAsignada[]>([]);
    const [todosServicios, setTodosServicios] = useState<ServicioAsignado[]>([]);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        async function fetchData() {
            try {
                const organizationId = "org-001";
                const sedesData = await listSedes(organizationId, { estado: "Activos" });
                setTodasSedes((sedesData.registros ?? sedesData).map((s: any) => ({
                    sedeId: s.id,
                    nombre: s.nombre,
                    activa: false
                })));

                const serviciosData = await listServices(organizationId, { estado: "Activos" });
                setTodosServicios((serviciosData.registros ?? serviciosData).map((srv: any) => ({
                    servicioId: srv.id,
                    nombre: srv.nombre,
                    activo: false
                })));
            } catch (error) {
                console.error("Error al cargar sedes/servicios:", error);
            }
        }
        fetchData();
    }, []);

    const navigate = useNavigate();

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const { name, value, type } = e.target;
        const checked = (e.target as HTMLInputElement).checked;
        setFormData((prev) => ({
            ...prev,
            [name]: type === "checkbox" ? checked : value,
        }));
    };

    const toggleSede = (sedeId: string) => {
        setFormData((prev) => ({
            ...prev,
            sedes: prev.sedes.some(s => s.sedeId === sedeId)
                ? prev.sedes.filter(s => s.sedeId !== sedeId)
                : [...prev.sedes, { sedeId, activa: true }]
        }));
    };

    const toggleServicio = (servicioId: string) => {
        setFormData((prev) => ({
            ...prev,
            servicios: prev.servicios.some(s => s.servicioId === servicioId)
                ? prev.servicios.filter(s => s.servicioId !== servicioId)
                : [...prev.servicios, { servicioId, activo: true }]
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
            const nuevoEmpleado = await crearEmpleado(formData);
            console.log("Empleado creado:", nuevoEmpleado);
            navigate("/empleados");
        } catch (error) {
            console.error("Error al crear empleado:", error);
            setErrores({ general: "No se pudo crear el empleado. Intenta nuevamente." });
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="empleado-form-page">
            <h1>Nuevo empleado</h1>

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
                        /> Es profesional
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
                                    checked={formData.sedes.some(sel => sel.sedeId === s.sedeId)}
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
                                            checked={formData.servicios.some(sel => sel.servicioId === s.servicioId)}
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
                <p className="error-text general-error">{errores.general}</p>
            )}

            {/* Botones */}
            <div className="form-actions">
                <button className="btn-primary" onClick={handleGuardar} disabled={loading}>
                    {loading ? "Guardando..." : "Crear empleado"}
                </button>
                <button className="btn-outline" onClick={() => window.history.back()}>Cancelar</button>
            </div>
        </div>
    );
}