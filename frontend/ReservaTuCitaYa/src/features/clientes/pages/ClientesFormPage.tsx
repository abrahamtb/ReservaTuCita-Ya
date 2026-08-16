import { useState } from "react";
import "./ClienteFormPage.css";
import { useNavigate } from "react-router-dom";
import { crearCliente } from "../../../api/clientesApi";



export default function ClienteFormPage() {
    const [formData, setFormData] = useState({
        tipoDocumento: "DNI",
        numeroDocumento: "",
        nombres: "",
        apellidos: "",
        telefono: "",
        correo: "",
        fechaNacimiento: "",
        direccion: "",
        observaciones: "",
        estado: true
    });

    const navigate = useNavigate();

    const [errors, setErrors] = useState<Record<string, string>>({});

    const handleChange = (
        e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>
    ) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    const validate = () => {
        const newErrors: Record<string, string> = {};

        if (!formData.numeroDocumento) {
            newErrors.numeroDocumento = "El número de documento es obligatorio";
        } else if (formData.numeroDocumento.length < 8) {
            newErrors.numeroDocumento = "Debe tener al menos 8 dígitos";
        }

        if (!formData.nombres) {
            newErrors.nombres = "Los nombres son obligatorios";
        }

        if (!formData.apellidos) {
            newErrors.apellidos = "Los apellidos son obligatorios";
        }

        if (!formData.telefono) {
            newErrors.telefono = "El teléfono es obligatorio";
        }

        if (!formData.correo) {
            newErrors.correo = "El correo es obligatorio";
        } else if (!/\S+@\S+\.\S+/.test(formData.correo)) {
            newErrors.correo = "Formato de correo inválido";
        }

        if (formData.fechaNacimiento) {
            const fecha = new Date(formData.fechaNacimiento);
            if (isNaN(fecha.getTime())) {
                newErrors.fechaNacimiento = "Fecha inválida";
            }
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (validate()) {
            try {
                await crearCliente(formData); 
                alert("Cliente creado correctamente");
                navigate("/clientes");
            } catch (err) {
                alert("No se pudo crear el cliente");
            }
        }
    };



    const handleCancelar = () => {
        window.history.back();
    };

    return (
        <div className="cliente-form-container">
            <h1>Nuevo Cliente</h1>

            <div className="cliente-form-content">
                <form className="cliente-form" onSubmit={handleSubmit}>
                    <p className="form-subtitle">Completa la información solicitada</p>

                    <div className="form-grid">
                        <div>
                            <label>Tipo de documento *</label>
                            <select
                                name="tipoDocumento"
                                value={formData.tipoDocumento}
                                onChange={handleChange}
                            >
                                <option value="DNI">DNI</option>
                                <option value="Pasaporte">Pasaporte</option>
                                <option value="Carnet de extranjería">Carnet de extranjería</option>
                            </select>
                        </div>

                        <div>
                            <label>Número de documento *</label>
                            <input
                                name="numeroDocumento"
                                value={formData.numeroDocumento}
                                onChange={handleChange}
                            />
                            {errors.numeroDocumento && (
                                <span className="error-text">{errors.numeroDocumento}</span>
                            )}
                        </div>

                        <div>
                            <label>Nombres *</label>
                            <input
                                name="nombres"
                                value={formData.nombres}
                                onChange={handleChange}
                            />
                            {errors.nombres && (
                                <span className="error-text">{errors.nombres}</span>
                            )}
                        </div>

                        <div>
                            <label>Apellidos *</label>
                            <input
                                name="apellidos"
                                value={formData.apellidos}
                                onChange={handleChange}
                            />
                            {errors.apellidos && (
                                <span className="error-text">{errors.apellidos}</span>
                            )}
                        </div>

                        <div>
                            <label>Teléfono *</label>
                            <input
                                name="telefono"
                                value={formData.telefono}
                                onChange={handleChange}
                            />
                            {errors.telefono && (
                                <span className="error-text">{errors.telefono}</span>
                            )}
                        </div>

                        <div>
                            <label>Correo *</label>
                            <input
                                type="email"
                                name="correo"
                                value={formData.correo}
                                onChange={handleChange}
                            />
                            {errors.correo && (
                                <span className="error-text">{errors.correo}</span>
                            )}
                        </div>

                        <div>
                            <label>Fecha de nacimiento</label>
                            <input
                                type="date"
                                name="fechaNacimiento"
                                value={formData.fechaNacimiento}
                                onChange={handleChange}
                            />
                            {errors.fechaNacimiento && (
                                <span className="error-text">{errors.fechaNacimiento}</span>
                            )}
                        </div>

                        <div>
                            <label>Dirección</label>
                            <input
                                name="direccion"
                                value={formData.direccion}
                                onChange={handleChange}
                            />
                        </div>

                        <div className="observaciones">
                            <label>Observaciones</label>
                            <textarea
                                name="observaciones"
                                value={formData.observaciones}
                                onChange={handleChange}
                            />
                        </div>
                    </div>

                    <div className="form-actions">
                        <button
                            type="button"
                            className="btn-secondary"
                            onClick={handleCancelar}
                        >
                            Cancelar
                        </button>
                        <button type="submit" className="btn-primary">
                            Guardar
                        </button>
                    </div>
                </form>

                <aside className="estado-box">
                    <h3>Estado</h3>
                    <span
                        className={formData.estado ? "estado-activo" : "estado-inactivo"}
                    >
                        {formData.estado ? "Activo" : "Inactivo"}
                    </span>
                    <p>{formData.estado ? "Cuenta activa" : "Cuenta desactivada"}</p>
                </aside>
            </div>
        </div>
    );
}
