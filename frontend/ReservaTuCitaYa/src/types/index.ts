export type EstadoFiltro = 'Todos' | 'Activos' | 'Inactivos'
export type ModalidadServicio = 'NoDefinido' | 'Presencial' | 'Virtual' | 'Domicilio'

export interface PageResult<T> {
  elementos: T[]
  paginaActual: number
  tamanoPagina: number
  totalElementos: number
  totalPaginas: number
  tieneAnterior: boolean
  tieneSiguiente: boolean
}

export interface AuthUser {
  id: string
  email: string
  roles: string[]
  permisos: string[]
  organizacion?: Option | null
  clienteId?: string | null
  empleadoId?: string | null
}
export interface Option { id: string; nombre: string }

export interface Organization {
  id: string; tipoOrganizacionId?: string; tipoOrganizacion: string; nombreComercial: string
  razonSocial?: string; numeroDocumento: string; telefono?: string; correo?: string
  direccionPrincipal?: string; logoUrl?: string; estaActivo: boolean
  fechaCreacion?: string; fechaModificacion?: string; cantidadSedesActivas?: number
}
export interface OrganizationRequest {
  tipoOrganizacionId: string; nombreComercial: string; razonSocial?: string
  numeroDocumento: string; telefono?: string; correo?: string; direccionPrincipal?: string; logoUrl?: string
}

export interface Sede {
  id: string; organizacionId: string; organizacion?: string; nombre: string; direccion: string
  telefono?: string; correo?: string; referencia?: string; estaActivo: boolean
  fechaCreacion?: string; fechaModificacion?: string
}
export type SedeRequest = Pick<Sede, 'nombre' | 'direccion' | 'telefono' | 'correo' | 'referencia'>

export interface Categoria {
  id: string; organizacionId: string; organizacion: string; nombre: string; descripcion?: string
  cantidadServicios: number; cantidadServiciosActivos?: number; estaActivo: boolean
  fechaCreacion?: string; fechaModificacion?: string
}
export interface CategoriaRequest { nombre: string; descripcion?: string }

export interface SedeAsignacion {
  sedeId: string; nombre?: string; sede?: string; estaActivo?: boolean; sedeActiva?: boolean
  estaAsignada?: boolean; precioEspecial?: number; precioAplicable?: number
}
export interface Servicio {
  id: string; organizacionId: string; organizacion?: string; categoriaServicioId?: string; categoria: string
  nombre: string; descripcion?: string; duracionMinutos: number; precio: number; montoAdelanto: number
  modalidad: ModalidadServicio; esGrupal: boolean; capacidadMaxima: number; requiereProfesional?: boolean
  requiereRecurso?: boolean; permiteCancelacion?: boolean; permiteReprogramacion?: boolean
  horasLimiteCancelacion?: number; tiempoPreparacionMinutos?: number; tiempoPosteriorMinutos?: number
  cantidadSedes?: number; sedes?: SedeAsignacion[]; estaActivo: boolean
}
export interface ServicioRequest {
  categoriaServicioId: string; nombre: string; descripcion?: string; duracionMinutos: number
  precio: number; montoAdelanto: number; modalidad: ModalidadServicio; esGrupal: boolean
  capacidadMaxima: number; requiereProfesional: boolean; requiereRecurso: boolean
  permiteCancelacion: boolean; permiteReprogramacion: boolean; horasLimiteCancelacion: number
  tiempoPreparacionMinutos: number; tiempoPosteriorMinutos: number
  sedes: { sedeId: string; precioEspecial?: number }[]
}

export interface ProblemDetails {
  title?: string; detail?: string; status?: number; errors?: Record<string, string[]>
}


export type EstadoReserva =
  | 'NoDefinido' | 'Pendiente' | 'Confirmada' | 'Presente' | 'EnAtencion'
  | 'Atendida' | 'Reprogramada' | 'Cancelada' | 'NoAsistio'

export type ResultadoAtencion = 'Completada' | 'Parcial' | 'Interrumpida'

export interface EmpleadoLista {
  id: string
  organizacionId: string
  numeroDocumento: string
  nombres: string
  apellidos: string
  nombreCompleto: string
  correo?: string | null
  telefono?: string | null
  cargo: string
  especialidad?: string | null
  esProfesional: boolean
  cantidadSedes: number
  cantidadServicios: number
  estaActivo: boolean
}

export interface AgendaReserva {
  reservaId: string
  codigoReserva: string
  clienteId: string
  clienteNombre: string
  servicioId: string
  servicioNombre: string
  sedeId: string
  sedeNombre: string
  horaInicio: string
  horaFin: string
  estado: EstadoReserva
  cantidadParticipantes: number
  atencionId?: string | null
  fechaHoraPresencia?: string | null
  fechaHoraInicioReal?: string | null
  fechaHoraFinReal?: string | null
}

export interface AgendaProfesional {
  profesionalId: string
  profesionalNombre: string
  fecha: string
  totalReservas: number
  reservas: AgendaReserva[]
}

export interface EntidadResumen { id: string; nombre: string }

export interface AtencionDetalle {
  id: string
  reservaId: string
  organizacionId: string
  codigoReserva: string
  estadoReserva: EstadoReserva
  cliente: EntidadResumen
  servicio: EntidadResumen
  sede: EntidadResumen
  profesional?: EntidadResumen | null
  fecha: string
  horaInicioProgramada: string
  horaFinProgramada: string
  fechaHoraPresencia?: string | null
  fechaHoraInicioReal?: string | null
  fechaHoraFinReal?: string | null
  minutosEspera?: number | null
  duracionRealMinutos?: number | null
  resultado?: ResultadoAtencion | null
  observaciones?: string | null
  recomendaciones?: string | null
  proximoServicio?: EntidadResumen | null
  proximaFechaSugerida?: string | null
}

export interface FinalizarAtencionRequest {
  resultado: ResultadoAtencion
  observaciones?: string | null
  recomendaciones?: string | null
  proximoServicioId?: string | null
  proximaFechaSugerida?: string | null
}

export interface HistorialReserva {
  id: string
  estadoAnterior?: EstadoReserva | null
  estadoNuevo: EstadoReserva
  tipoAccion: string
  motivo?: string | null
  observacion?: string | null
  fechaAccion: string
}

export interface ReservaDetalle {
  id: string
  organizacionId: string
  codigo: string
  estado: EstadoReserva
  cliente: EntidadResumen
  servicio: EntidadResumen
  sede: EntidadResumen
  profesional?: EntidadResumen | null
  recurso?: EntidadResumen | null
  fecha: string
  horaInicio: string
  horaFinServicio: string
  duracionMinutos: number
  cantidadParticipantes: number
  observaciones?: string | null
  historial: HistorialReserva[]
}

export interface MarcarPresenteRespuesta {
  reservaId: string
  atencionId: string
  codigoReserva: string
  estado: EstadoReserva
  fechaHoraPresencia: string
}

export interface IniciarAtencionRespuesta {
  reservaId: string
  atencionId: string
  codigoReserva: string
  estado: EstadoReserva
  fechaHoraInicioReal: string
}

export interface FinalizarAtencionRespuesta {
  reservaId: string
  atencionId: string
  codigoReserva: string
  estado: EstadoReserva
  resultado: ResultadoAtencion
  fechaHoraFinReal: string
}

export type MarcarNoAsistioRequest = Record<string, never>

export interface MarcarNoAsistioRespuesta {
  reservaId: string
  codigoReserva: string
  estado: EstadoReserva
  fechaHoraRegistro: string
}
