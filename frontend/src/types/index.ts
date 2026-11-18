export interface Rubro {
  id: number;
  nombre: string;
  descripcion?: string;
  activo: boolean;
  fechaCreacion: string;
  fechaModificacion?: string;
}

export interface Producto {
  id: number;
  nombre: string;
  descripcion?: string;
  codigo: string;
  rubroId: number;
  rubroNombre: string;
  precioVenta: number;
  costo: number;
  stockActual: number;
  stockMinimo: number;
  activo: boolean;
  fechaCreacion: string;
  fechaModificacion?: string;
}

export interface Cliente {
  id: number;
  nombre: string;
  apellido: string;
  documento?: string;
  telefono?: string;
  email?: string;
  direccion?: string;
  activo: boolean;
  fechaCreacion: string;
}

export interface Proveedor {
  id: number;
  razonSocial: string;
  cuit?: string;
  telefono?: string;
  email?: string;
  direccion?: string;
  activo: boolean;
  fechaCreacion: string;
}

export interface Stock {
  id: number;
  productoId: number;
  productoNombre: string;
  productoCodigo: string;
  cantidad: number;
  stockMinimo: number;
  stockBajo: boolean;
}

export interface DetalleIngresoMercaderia {
  id?: number;
  productoId: number;
  productoNombre?: string;
  cantidad: number;
  costoUnitario: number;
}

export interface IngresoMercaderia {
  id: number;
  fecha: string;
  proveedorId: number;
  proveedorNombre: string;
  observaciones?: string;
  detalles: DetalleIngresoMercaderia[];
}

export interface DetalleVenta {
  id?: number;
  productoId: number;
  productoNombre?: string;
  cantidad: number;
  precioUnitario: number;
  subtotal: number;
}

export interface Venta {
  id: number;
  fecha: string;
  usuarioId: number;
  usuarioNombre: string;
  clienteId?: number | null;
  clienteNombre?: string | null;
  total: number;
  metodoPago: string;
  anulada: boolean;
  detalles: DetalleVenta[];
}

export interface Usuario {
  id: number;
  firebaseUid: string;
  email: string;
  nombre: string;
  apellido: string;
  rolId: number;
  rol?: Rol;
  activo: boolean;
  fechaCreacion: string;
}

export interface Rol {
  id: number;
  nombre: string;
  descripcion?: string;
  permisos: Permiso[];
}

export interface Permiso {
  id: number;
  nombre: string;
  descripcion?: string;
}

export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}
