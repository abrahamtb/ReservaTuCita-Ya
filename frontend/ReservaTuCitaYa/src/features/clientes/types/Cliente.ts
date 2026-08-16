export interface ClienteListado {
  id: number;
  nombres: string;
  apellidos: string;
  tipoDocumento: string;
  numeroDocumento: string;
  correo: string;
  telefono: string;
  estado: boolean;
}

export interface ClienteDetalle extends ClienteListado {
  direccion: string;
  fechaNacimiento: string;
  observaciones?: string;
  organizacionId: number;
}

export interface CrearClienteRequest {
  tipoDocumento: string;
  numeroDocumento: string;
  nombres: string;
  apellidos: string;
  correo: string;
  telefono: string;
  direccion: string;
  fechaNacimiento: string;
  observaciones?: string;
}

export interface ActualizarClienteRequest {
  id: number;
  tipoDocumento: string;
  numeroDocumento: string;
  nombres: string;
  apellidos: string;
  correo: string;
  telefono: string;
  direccion?: string;
  fechaNacimiento?: string;
  observaciones?: string;
  estado: boolean;
}


export interface ClienteFiltros {
  estado?: boolean;
  busqueda?: string;
  pagina?: number;
  tamañoPagina?: number;
}

export interface PaginaResultado<T> {
  total: number;
  paginaActual: number;
  tamañoPagina: number;
  elementos: T[];
}

