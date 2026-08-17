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
  permisos?: string[]
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

export interface ReporteFiltrosBase {
  fechaDesde: string
  fechaHasta: string
  sedeId?: string
  organizacionId?: string
  pagina: number
  tamanoPagina: number
}

export interface ReporteReservasFiltros extends ReporteFiltrosBase {
  profesionalId?: string
  servicioId?: string
  estado?: EstadoReserva | ''
  clienteId?: string
}

export interface ReporteReservasResumen {
  totalReservas: number
  confirmadasReprogramadas: number
  atendidas: number
  canceladas: number
  noAsistieron: number
}

export interface ReporteReservaItem {
  reservaId: string
  codigo: string
  fecha: string
  hora: string
  cliente: string
  servicio: string
  sede: string
  profesional?: string | null
  estado: EstadoReserva
  cantidadParticipantes: number
  precioTotal: number
}

export interface ReporteReservasResponse {
  fechaDesde: string
  fechaHasta: string
  indicadores: ReporteReservasResumen
  reservasPorEstado: { estado: EstadoReserva; cantidad: number }[]
  elementos: ReporteReservaItem[]
  paginaActual: number
  tamanoPagina: number
  totalElementos: number
  totalPaginas: number
}

export interface ReporteIngresosFiltros extends ReporteFiltrosBase {
  metodoPagoId?: string
}

export interface ReporteIngresosResumen {
  ingresosBrutos: number
  reembolsos: number
  ingresosNetos: number
  cantidadPagos: number
  ticketPromedio?: number | null
}

export interface MovimientoEconomico {
  fecha: string
  codigoMovimiento: string
  codigoReserva: string
  cliente: string
  sede: string
  tipo: 'Pago' | 'Reembolso'
  metodo?: string | null
  numeroOperacion?: string | null
  monto: number
}

export interface ReporteIngresosResponse {
  fechaDesde: string
  fechaHasta: string
  indicadores: ReporteIngresosResumen
  elementos: MovimientoEconomico[]
  paginaActual: number
  tamanoPagina: number
  totalElementos: number
  totalPaginas: number
}

export interface ReporteAtencionesFiltros extends ReporteFiltrosBase {
  profesionalId?: string
  servicioId?: string
  estado?: EstadoReserva | ''
  resultado?: ResultadoAtencion | ''
}

export interface ReporteAtencionesResumen {
  reservasProgramadas: number
  atendidas: number
  noAsistieron: number
  atencionParcialInterrumpida: number
  porcentajeAsistencia?: number | null
  sinDatos: boolean
}

export interface ReporteAtencionItem {
  reservaId: string
  codigoReserva: string
  fecha: string
  horaProgramada: string
  cliente: string
  servicio: string
  profesional?: string | null
  horaLlegada?: string | null
  horaInicioReal?: string | null
  horaFinReal?: string | null
  duracionRealMinutos?: number | null
  resultado?: ResultadoAtencion | null
  estado: EstadoReserva
}

export interface ReporteAtencionesResponse {
  fechaDesde: string
  fechaHasta: string
  indicadores: ReporteAtencionesResumen
  elementos: ReporteAtencionItem[]
  paginaActual: number
  tamanoPagina: number
  totalElementos: number
  totalPaginas: number
}

export interface EmpleadoOpcion {
  id: string
  nombreCompleto: string
}

export interface MetodoPagoOpcion {
  id: string
  nombre: string
}


export interface IndicadorComparativo {
  valorActual: number
  valorAnterior: number
  variacionPorcentaje?: number | null
  sinBaseComparacion: boolean
}

export interface DashboardIndicadores {
  reservasHoy: IndicadorComparativo
  porAtenderHoy: IndicadorComparativo
  atencionesCompletadas: IndicadorComparativo
  cancelaciones: IndicadorComparativo
  clientesNuevos: IndicadorComparativo
  ingresosNetos: IndicadorComparativo
}

export interface ReservaPorDia {
  fecha: string
  cantidad: number
}

export interface ReservasPorEstado {
  estado: EstadoReserva
  cantidad: number
}

export interface IngresoPorDia {
  fecha: string
  ingresosBrutos: number
  reembolsos: number
  ingresosNetos: number
}

export interface TopServicioDashboard {
  servicioId: string
  nombre: string
  cantidadReservas: number
  porcentajeSobreTotal: number
}

export interface ProximaReservaDashboard {
  reservaId: string
  codigo: string
  horaInicio: string
  cliente: string
  servicio: string
  profesional?: string | null
  estado: EstadoReserva
}

export interface DashboardFiltros {
  fechaDesde: string
  fechaHasta: string
  sedeId?: string
  organizacionId?: string
}

export interface DashboardResumen extends DashboardIndicadores {
  fechaDesde: string
  fechaHasta: string
  sedeId?: string | null
  fechaHoraConsulta: string
  reservasPorDia: ReservaPorDia[]
  reservasPorEstado: ReservasPorEstado[]
  ingresosPorDia: IngresoPorDia[]
  topServicios: TopServicioDashboard[]
  proximasReservas: ProximaReservaDashboard[]
}
