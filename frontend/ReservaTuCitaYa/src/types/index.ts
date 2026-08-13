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

export interface AuthUser { id: string; email: string; roles: string[] }
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
