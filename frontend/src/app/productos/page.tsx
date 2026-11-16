"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { api } from "@/services/api";
import { Producto, Rubro } from "@/types";

interface ProductoFormData {
  nombre: string;
  descripcion: string;
  codigo: string;
  rubroId: string;
  precioVenta: string;
  costo: string;
  stockMinimo: string;
}

interface Filtros {
  nombre: string;
  codigo: string;
  rubroId: string;
  activo: string; // "" = todos, "true" = activos, "false" = dados de baja
  conStock: string; // "" = todos, "true" = con stock, "false" = sin stock
}

const emptyForm: ProductoFormData = {
  nombre: "",
  descripcion: "",
  codigo: "",
  rubroId: "",
  precioVenta: "",
  costo: "",
  stockMinimo: "0",
};

const emptyFiltros: Filtros = {
  nombre: "",
  codigo: "",
  rubroId: "",
  activo: "",
  conStock: "",
};

export default function ProductosPage() {
  const [productos, setProductos] = useState<Producto[]>([]);
  const [rubros, setRubros] = useState<Rubro[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);

  // Búsqueda y filtros (RF-016)
  const [filtros, setFiltros] = useState<Filtros>(emptyFiltros);
  const [appliedFiltros, setAppliedFiltros] = useState<Filtros>(emptyFiltros);

  // Alta (RF-013) / modificación (RF-017)
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<Producto | null>(null);
  const [form, setForm] = useState<ProductoFormData>(emptyForm);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  // Imágenes (RF-020)
  const [uploadingId, setUploadingId] = useState<number | null>(null);
  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const [imageTargetId, setImageTargetId] = useState<number | null>(null);

  const buildQueryString = (f: Filtros): string => {
    const params = new URLSearchParams();
    if (f.nombre.trim()) params.set("nombre", f.nombre.trim());
    if (f.codigo.trim()) params.set("codigo", f.codigo.trim());
    if (f.rubroId) params.set("rubroId", f.rubroId);
    if (f.activo) params.set("activo", f.activo);
    if (f.conStock) params.set("conStock", f.conStock);
    const qs = params.toString();
    return qs ? `?${qs}` : "";
  };

  // Carga inicial (RF-015). Fetch encadenado en el efecto: no invocar
  // setState de forma directa dentro del mismo (react-hooks/set-state-in-effect).
  useEffect(() => {
    api
      .get<Producto[]>("/api/Productos")
      .then(setProductos)
      .catch((err) =>
        setError(err instanceof Error ? err.message : "Error al cargar los productos")
      )
      .finally(() => setLoading(false));
    api
      .get<Rubro[]>("/api/Rubros")
      .then(setRubros)
      .catch(() => undefined);
  }, []);

  const handleSearch = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await api.get<Producto[]>(
        `/api/Productos/buscar${buildQueryString(filtros)}`
      );
      setProductos(data);
      setAppliedFiltros(filtros);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error en la búsqueda");
    } finally {
      setLoading(false);
    }
  }, [filtros]);

  const handleClearFilters = useCallback(() => {
    setFiltros(emptyFiltros);
    setAppliedFiltros(emptyFiltros);
    setLoading(true);
    api
      .get<Producto[]>("/api/Productos")
      .then(setProductos)
      .catch((err) =>
        setError(err instanceof Error ? err.message : "Error al cargar los productos")
      )
      .finally(() => setLoading(false));
  }, []);

  const reload = useCallback(async () => {
    try {
      const qs = buildQueryString(appliedFiltros);
      const data = await api.get<Producto[]>(
        qs ? `/api/Productos/buscar${qs}` : "/api/Productos"
      );
      setProductos(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al recargar los productos");
    }
  }, [appliedFiltros]);

  const openCreate = () => {
    setEditing(null);
    setForm(emptyForm);
    setFormError(null);
    setShowForm(true);
  };

  const openEdit = (producto: Producto) => {
    setEditing(producto);
    setForm({
      nombre: producto.nombre,
      descripcion: producto.descripcion ?? "",
      codigo: producto.codigo,
      rubroId: String(producto.rubroId),
      precioVenta: String(producto.precioVenta),
      costo: String(producto.costo),
      stockMinimo: String(producto.stockMinimo),
    });
    setFormError(null);
    setShowForm(true);
  };

  const closeForm = () => {
    setShowForm(false);
    setEditing(null);
    setForm(emptyForm);
    setFormError(null);
  };

  const handleSubmit = async () => {
    setFormError(null);

    // Validación de interfaz (no reemplaza la del backend — RNF-051).
    if (!form.nombre.trim()) return setFormError("El nombre es requerido.");
    if (!form.codigo.trim()) return setFormError("El código es requerido.");
    if (!form.rubroId) return setFormError("El rubro es requerido.");

    const precioVenta = Number(form.precioVenta);
    const costo = Number(form.costo);
    const stockMinimo = Number(form.stockMinimo);
    if (Number.isNaN(precioVenta) || precioVenta < 0)
      return setFormError("El precio de venta no puede ser negativo.");
    if (Number.isNaN(costo) || costo < 0)
      return setFormError("El costo no puede ser negativo.");
    if (Number.isNaN(stockMinimo) || stockMinimo < 0)
      return setFormError("El stock mínimo no puede ser negativo.");

    const payload = {
      nombre: form.nombre.trim(),
      descripcion: form.descripcion.trim() || null,
      codigo: form.codigo.trim(),
      rubroId: Number(form.rubroId),
      precioVenta,
      costo,
      stockMinimo,
    };

    setSaving(true);
    try {
      if (editing) {
        await api.put(`/api/Productos/${editing.id}`, payload);
        setInfo("Producto modificado correctamente.");
      } else {
        await api.post("/api/Productos", payload);
        setInfo("Producto creado correctamente.");
      }
      closeForm();
      await reload();
      setTimeout(() => setInfo(null), 3000);
    } catch (err) {
      setFormError(err instanceof Error ? err.message : "Error al guardar el producto");
    } finally {
      setSaving(false);
    }
  };

  // Baja lógica (RF-018).
  const handleDelete = async (producto: Producto) => {
    if (!confirm(`¿Dar de baja el producto "${producto.nombre}"? Se conservará su información histórica.`)) {
      return;
    }
    setError(null);
    try {
      await api.delete(`/api/Productos/${producto.id}`);
      setInfo("Producto dado de baja correctamente.");
      await reload();
      setTimeout(() => setInfo(null), 3000);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al dar de baja el producto");
    }
  };

  // Imágenes (RF-020): carga y reemplazo.
  const triggerImageUpload = (productoId: number) => {
    setImageTargetId(productoId);
    fileInputRef.current?.click();
  };

  const handleImageChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    e.target.value = ""; // permite reseleccionar el mismo archivo
    if (!file || imageTargetId == null) return;

    setError(null);
    setUploadingId(imageTargetId);
    try {
      const formData = new FormData();
      formData.append("imagen", file);
      await api.upload<Producto>(`/api/Productos/${imageTargetId}/imagen`, formData);
      setInfo("Imagen subida correctamente.");
      await reload();
      setTimeout(() => setInfo(null), 3000);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al subir la imagen");
    } finally {
      setUploadingId(null);
      setImageTargetId(null);
    }
  };

  // Imágenes (RF-020): eliminación.
  const handleRemoveImage = async (producto: Producto) => {
    if (!confirm(`¿Eliminar la imagen de "${producto.nombre}"?`)) return;
    setError(null);
    setUploadingId(producto.id);
    try {
      await api.delete<Producto>(`/api/Productos/${producto.id}/imagen`);
      setInfo("Imagen eliminada correctamente.");
      await reload();
      setTimeout(() => setInfo(null), 3000);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al eliminar la imagen");
    } finally {
      setUploadingId(null);
    }
  };

  const fmt = (n: number) => `$${n.toLocaleString("es-AR", { minimumFractionDigits: 2 })}`;

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold text-gray-800">Productos</h1>
        <button
          onClick={openCreate}
          className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition-colors text-sm"
        >
          Nuevo producto
        </button>
      </div>

      {loading && <p className="text-gray-600">Cargando productos...</p>}

      {error && (
        <p className="text-red-600 bg-red-50 border border-red-200 rounded-md p-3 mb-4">
          {error}
        </p>
      )}

      {info && (
        <p className="text-green-700 bg-green-50 border border-green-200 rounded-md p-3 mb-4">
          {info}
        </p>
      )}

      {/* Búsqueda y filtros (RF-016) */}
      <div className="mb-6 bg-white border border-gray-200 rounded-lg p-4">
        <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-5 gap-3">
          <div>
            <label htmlFor="f-nombre" className="block text-xs font-medium text-gray-600 mb-1">
              Nombre
            </label>
            <input
              id="f-nombre"
              type="text"
              value={filtros.nombre}
              onChange={(e) => setFiltros({ ...filtros, nombre: e.target.value })}
              onKeyDown={(e) => e.key === "Enter" && handleSearch()}
              className="w-full px-2 py-1.5 border border-gray-300 rounded-md text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Buscar por nombre"
            />
          </div>
          <div>
            <label htmlFor="f-codigo" className="block text-xs font-medium text-gray-600 mb-1">
              Código
            </label>
            <input
              id="f-codigo"
              type="text"
              value={filtros.codigo}
              onChange={(e) => setFiltros({ ...filtros, codigo: e.target.value })}
              onKeyDown={(e) => e.key === "Enter" && handleSearch()}
              className="w-full px-2 py-1.5 border border-gray-300 rounded-md text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Buscar por código"
            />
          </div>
          <div>
            <label htmlFor="f-rubro" className="block text-xs font-medium text-gray-600 mb-1">
              Rubro
            </label>
            <select
              id="f-rubro"
              value={filtros.rubroId}
              onChange={(e) => setFiltros({ ...filtros, rubroId: e.target.value })}
              className="w-full px-2 py-1.5 border border-gray-300 rounded-md text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Todos</option>
              {rubros.map((r) => (
                <option key={r.id} value={r.id}>
                  {r.nombre}
                </option>
              ))}
            </select>
          </div>
          <div>
            <label htmlFor="f-estado" className="block text-xs font-medium text-gray-600 mb-1">
              Estado
            </label>
            <select
              id="f-estado"
              value={filtros.activo}
              onChange={(e) => setFiltros({ ...filtros, activo: e.target.value })}
              className="w-full px-2 py-1.5 border border-gray-300 rounded-md text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Todos</option>
              <option value="true">Activos</option>
              <option value="false">Dados de baja</option>
            </select>
          </div>
          <div>
            <label htmlFor="f-stock" className="block text-xs font-medium text-gray-600 mb-1">
              Stock
            </label>
            <select
              id="f-stock"
              value={filtros.conStock}
              onChange={(e) => setFiltros({ ...filtros, conStock: e.target.value })}
              className="w-full px-2 py-1.5 border border-gray-300 rounded-md text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Todos</option>
              <option value="true">Con disponibilidad</option>
              <option value="false">Sin disponibilidad</option>
            </select>
          </div>
        </div>
        <div className="flex gap-2 mt-3">
          <button
            onClick={handleSearch}
            className="bg-blue-600 text-white px-4 py-1.5 rounded-md hover:bg-blue-700 transition-colors text-sm"
          >
            Buscar
          </button>
          <button
            onClick={handleClearFilters}
            className="bg-gray-200 text-gray-700 px-4 py-1.5 rounded-md hover:bg-gray-300 transition-colors text-sm"
          >
            Limpiar
          </button>
        </div>
      </div>

      {/* Formulario alta/modificación (RF-013, RF-017) */}
      {showForm && (
        <div className="mb-6 bg-white border border-gray-200 rounded-lg p-4">
          <h2 className="text-lg font-semibold text-gray-800 mb-3">
            {editing ? "Modificar producto" : "Nuevo producto"}
          </h2>

          {formError && (
            <p className="text-red-600 bg-red-50 border border-red-200 rounded-md p-3 mb-3 text-sm">
              {formError}
            </p>
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
            <div>
              <label htmlFor="p-nombre" className="block text-sm font-medium text-gray-700 mb-1">
                Nombre *
              </label>
              <input
                id="p-nombre"
                type="text"
                maxLength={100}
                value={form.nombre}
                onChange={(e) => setForm({ ...form, nombre: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label htmlFor="p-codigo" className="block text-sm font-medium text-gray-700 mb-1">
                Código *
              </label>
              <input
                id="p-codigo"
                type="text"
                maxLength={50}
                value={form.codigo}
                onChange={(e) => setForm({ ...form, codigo: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label htmlFor="p-rubro" className="block text-sm font-medium text-gray-700 mb-1">
                Rubro *
              </label>
              <select
                id="p-rubro"
                value={form.rubroId}
                onChange={(e) => setForm({ ...form, rubroId: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">Seleccionar rubro</option>
                {rubros
                  .filter((r) => r.activo)
                  .map((r) => (
                    <option key={r.id} value={r.id}>
                      {r.nombre}
                    </option>
                  ))}
              </select>
            </div>
            <div>
              <label htmlFor="p-precio" className="block text-sm font-medium text-gray-700 mb-1">
                Precio de venta *
              </label>
              <input
                id="p-precio"
                type="number"
                min="0"
                step="0.01"
                value={form.precioVenta}
                onChange={(e) => setForm({ ...form, precioVenta: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label htmlFor="p-costo" className="block text-sm font-medium text-gray-700 mb-1">
                Costo *
              </label>
              <input
                id="p-costo"
                type="number"
                min="0"
                step="0.01"
                value={form.costo}
                onChange={(e) => setForm({ ...form, costo: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label htmlFor="p-stockmin" className="block text-sm font-medium text-gray-700 mb-1">
                Stock mínimo
              </label>
              <input
                id="p-stockmin"
                type="number"
                min="0"
                step="1"
                value={form.stockMinimo}
                onChange={(e) => setForm({ ...form, stockMinimo: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div className="md:col-span-2 lg:col-span-3">
              <label htmlFor="p-desc" className="block text-sm font-medium text-gray-700 mb-1">
                Descripción
              </label>
              <input
                id="p-desc"
                type="text"
                maxLength={500}
                value={form.descripcion}
                onChange={(e) => setForm({ ...form, descripcion: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </div>

          <div className="flex gap-2 mt-4">
            <button
              onClick={handleSubmit}
              disabled={saving}
              className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition-colors text-sm disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {saving ? "Guardando..." : editing ? "Guardar cambios" : "Crear producto"}
            </button>
            <button
              onClick={closeForm}
              disabled={saving}
              className="bg-gray-200 text-gray-700 px-4 py-2 rounded-md hover:bg-gray-300 transition-colors text-sm"
            >
              Cancelar
            </button>
          </div>
        </div>
      )}

      <input
        ref={fileInputRef}
        type="file"
        accept="image/jpeg,image/png,image/webp,image/gif"
        className="hidden"
        onChange={handleImageChange}
      />

      {!loading && !error && productos.length === 0 && (
        <p className="text-gray-600">No hay productos registrados.</p>
      )}

      {!loading && productos.length > 0 && (
        <div className="overflow-x-auto rounded-lg border border-gray-200 bg-white">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Imagen</th>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Nombre</th>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Código</th>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Rubro</th>
                <th className="px-3 py-3 text-right text-sm font-semibold text-gray-700">Precio</th>
                <th className="px-3 py-3 text-right text-sm font-semibold text-gray-700">Costo</th>
                <th className="px-3 py-3 text-right text-sm font-semibold text-gray-700">Stock</th>
                <th className="px-3 py-3 text-center text-sm font-semibold text-gray-700">Estado</th>
                <th className="px-3 py-3 text-right text-sm font-semibold text-gray-700">Acciones</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {productos.map((producto) => (
                <tr key={producto.id} className="hover:bg-gray-50">
                  <td className="px-3 py-2">
                    {producto.imagenUrl ? (
                      // eslint-disable-next-line @next/next/no-img-element
                      <img
                        src={producto.imagenUrl}
                        alt={producto.nombre}
                        className="h-10 w-10 rounded object-cover border border-gray-200"
                      />
                    ) : (
                      <span className="inline-block h-10 w-10 rounded bg-gray-100 text-gray-400 text-center leading-10 text-xs">
                        —
                      </span>
                    )}
                  </td>
                  <td className="px-3 py-2 text-sm text-gray-900">{producto.nombre}</td>
                  <td className="px-3 py-2 text-sm text-gray-600 font-mono">{producto.codigo}</td>
                  <td className="px-3 py-2 text-sm text-gray-600">{producto.rubroNombre}</td>
                  <td className="px-3 py-2 text-sm text-gray-900 text-right">{fmt(producto.precioVenta)}</td>
                  <td className="px-3 py-2 text-sm text-gray-600 text-right">{fmt(producto.costo)}</td>
                  <td className="px-3 py-2 text-sm text-right">
                    <span
                      className={
                        producto.stockActual <= producto.stockMinimo
                          ? "text-red-600 font-medium"
                          : "text-gray-900"
                      }
                      title={
                        producto.stockActual <= producto.stockMinimo
                          ? "Stock igual o inferior al mínimo"
                          : undefined
                      }
                    >
                      {producto.stockActual}
                    </span>
                    <span className="text-gray-400 text-xs"> / {producto.stockMinimo}mín</span>
                  </td>
                  <td className="px-3 py-2 text-center text-sm">
                    <span
                      className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${
                        producto.activo
                          ? "bg-green-100 text-green-700"
                          : "bg-gray-100 text-gray-500"
                      }`}
                    >
                      {producto.activo ? "Activo" : "Baja"}
                    </span>
                  </td>
                  <td className="px-3 py-2 text-sm text-right whitespace-nowrap">
                    <button
                      onClick={() => triggerImageUpload(producto.id)}
                      disabled={uploadingId === producto.id}
                      className="text-blue-600 hover:text-blue-800 mr-3 disabled:opacity-50"
                      title="Subir o reemplazar imagen"
                    >
                      {uploadingId === producto.id ? "Subiendo..." : "Imagen"}
                    </button>
                    <button
                      onClick={() => openEdit(producto)}
                      className="text-blue-600 hover:text-blue-800 mr-3"
                    >
                      Editar
                    </button>
                    {producto.imagenUrl && (
                      <button
                        onClick={() => handleRemoveImage(producto)}
                        disabled={uploadingId === producto.id}
                        className="text-orange-600 hover:text-orange-800 mr-3 disabled:opacity-50"
                        title="Eliminar imagen"
                      >
                        Quitar imagen
                      </button>
                    )}
                    {producto.activo && (
                      <button
                        onClick={() => handleDelete(producto)}
                        className="text-red-600 hover:text-red-800"
                      >
                        Dar de baja
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
