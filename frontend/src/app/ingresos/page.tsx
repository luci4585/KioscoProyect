"use client";

import { useCallback, useEffect, useState } from "react";
import { api } from "@/services/api";
import { IngresoMercaderia, Producto, Proveedor } from "@/types";

interface DetalleForm {
  productoId: string;
  cantidad: string;
  costoUnitario: string;
}

interface IngresoFormData {
  proveedorId: string;
  observaciones: string;
  detalles: DetalleForm[];
}

const emptyDetalle: DetalleForm = { productoId: "", cantidad: "", costoUnitario: "" };

const emptyForm: IngresoFormData = {
  proveedorId: "",
  observaciones: "",
  detalles: [{ ...emptyDetalle }],
};

const fmtMoney = (n: number) => `$${n.toLocaleString("es-AR", { minimumFractionDigits: 2 })}`;

export default function IngresosPage() {
  const [ingresos, setIngresos] = useState<IngresoMercaderia[]>([]);
  const [proveedores, setProveedores] = useState<Proveedor[]>([]);
  const [productos, setProductos] = useState<Producto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);

  // Alta de ingreso (RF-036)
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState<IngresoFormData>(emptyForm);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  // Detalle de un ingreso existente
  const [detalle, setDetalle] = useState<IngresoMercaderia | null>(null);

  const loadIngresos = useCallback(async () => {
    try {
      const data = await api.get<IngresoMercaderia[]>("/api/IngresosMercaderia");
      setIngresos(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cargar los ingresos");
    } finally {
      setLoading(false);
    }
  }, []);

  // Carga inicial. Fetch encadenado en el efecto: no invocar setState de forma
  // directa dentro del mismo (react-hooks/set-state-in-effect).
  useEffect(() => {
    api
      .get<IngresoMercaderia[]>("/api/IngresosMercaderia")
      .then(setIngresos)
      .catch((err) =>
        setError(err instanceof Error ? err.message : "Error al cargar los ingresos")
      )
      .finally(() => setLoading(false));
    api
      .get<Proveedor[]>("/api/Proveedores")
      .then(setProveedores)
      .catch(() => undefined);
    api
      .get<Producto[]>("/api/Productos")
      .then(setProductos)
      .catch(() => undefined);
  }, []);

  const openCreate = () => {
    setForm(emptyForm);
    setFormError(null);
    setDetalle(null);
    setShowForm(true);
  };

  const closeForm = () => {
    setShowForm(false);
    setForm(emptyForm);
    setFormError(null);
  };

  const addDetalle = () => {
    setForm({ ...form, detalles: [...form.detalles, { ...emptyDetalle }] });
  };

  const removeDetalle = (index: number) => {
    if (form.detalles.length === 1) return;
    setForm({ ...form, detalles: form.detalles.filter((_, i) => i !== index) });
  };

  const updateDetalle = (index: number, patch: Partial<DetalleForm>) => {
    const detalles = form.detalles.map((d, i) => (i === index ? { ...d, ...patch } : d));
    setForm({ ...form, detalles });
  };

  const handleSubmit = async () => {
    setFormError(null);

    // Validación de interfaz (no reemplaza la del backend — RNF-051).
    if (!form.proveedorId) return setFormError("El proveedor es requerido (RF-037).");
    if (form.detalles.length === 0)
      return setFormError("Debe incluir al menos un producto.");

    const detalles: { productoId: number; cantidad: number; costoUnitario: number }[] = [];
    for (const [i, d] of form.detalles.entries()) {
      if (!d.productoId) return setFormError(`Detalle ${i + 1}: seleccione un producto.`);
      const cantidad = Number(d.cantidad);
      if (!Number.isInteger(cantidad) || cantidad <= 0)
        return setFormError(`Detalle ${i + 1}: la cantidad debe ser un entero mayor a cero.`);
      const costo = Number(d.costoUnitario);
      if (Number.isNaN(costo) || costo < 0)
        return setFormError(`Detalle ${i + 1}: el costo unitario no puede ser negativo (RF-039).`);
      detalles.push({ productoId: Number(d.productoId), cantidad, costoUnitario: costo });
    }

    setSaving(true);
    try {
      await api.post("/api/IngresosMercaderia", {
        proveedorId: Number(form.proveedorId),
        observaciones: form.observaciones.trim() || null,
        detalles,
      });
      setInfo("Ingreso de mercadería registrado. Stock actualizado correctamente.");
      closeForm();
      await loadIngresos();
      setTimeout(() => setInfo(null), 3000);
    } catch (err) {
      setFormError(err instanceof Error ? err.message : "Error al registrar el ingreso");
    } finally {
      setSaving(false);
    }
  };

  const verDetalle = async (id: number) => {
    setError(null);
    try {
      const data = await api.get<IngresoMercaderia>(`/api/IngresosMercaderia/${id}`);
      setDetalle(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cargar el detalle");
    }
  };

  const fmtFecha = (iso: string) => new Date(iso).toLocaleString("es-AR");

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold text-gray-800">Ingresos de mercadería</h1>
        <button
          onClick={openCreate}
          className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition-colors text-sm"
        >
          Nuevo ingreso
        </button>
      </div>

      {loading && <p className="text-gray-600">Cargando ingresos...</p>}

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

      {/* Formulario de alta (RF-036 a RF-039) */}
      {showForm && (
        <div className="mb-6 bg-white border border-gray-200 rounded-lg p-4">
          <h2 className="text-lg font-semibold text-gray-800 mb-3">Nuevo ingreso de mercadería</h2>

          {formError && (
            <p className="text-red-600 bg-red-50 border border-red-200 rounded-md p-3 mb-3 text-sm">
              {formError}
            </p>
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 gap-3 mb-4">
            <div>
              <label htmlFor="ing-proveedor" className="block text-sm font-medium text-gray-700 mb-1">
                Proveedor * (RF-037)
              </label>
              <select
                id="ing-proveedor"
                value={form.proveedorId}
                onChange={(e) => setForm({ ...form, proveedorId: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">Seleccionar proveedor</option>
                {proveedores.map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.razonSocial}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label htmlFor="ing-obs" className="block text-sm font-medium text-gray-700 mb-1">
                Observaciones
              </label>
              <input
                id="ing-obs"
                type="text"
                maxLength={500}
                value={form.observaciones}
                onChange={(e) => setForm({ ...form, observaciones: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Ej: Pedido semanal"
              />
            </div>
          </div>

          <p className="text-sm font-medium text-gray-700 mb-2">Productos del ingreso *</p>
          <div className="space-y-2 mb-3">
            {form.detalles.map((d, i) => (
              <div key={i} className="flex flex-wrap items-end gap-2 border border-gray-200 rounded-md p-2">
                <div className="flex-1 min-w-48">
                  <label htmlFor={`det-prod-${i}`} className="block text-xs text-gray-600 mb-1">
                    Producto
                  </label>
                  <select
                    id={`det-prod-${i}`}
                    value={d.productoId}
                    onChange={(e) => updateDetalle(i, { productoId: e.target.value })}
                    className="w-full px-2 py-1.5 border border-gray-300 rounded-md text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  >
                    <option value="">Seleccionar</option>
                    {productos.map((p) => (
                      <option key={p.id} value={p.id}>
                        {p.nombre} ({p.codigo})
                      </option>
                    ))}
                  </select>
                </div>
                <div className="w-24">
                  <label htmlFor={`det-cant-${i}`} className="block text-xs text-gray-600 mb-1">
                    Cantidad
                  </label>
                  <input
                    id={`det-cant-${i}`}
                    type="number"
                    min={1}
                    step={1}
                    value={d.cantidad}
                    onChange={(e) => updateDetalle(i, { cantidad: e.target.value })}
                    className="w-full px-2 py-1.5 border border-gray-300 rounded-md text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div className="w-32">
                  <label htmlFor={`det-costo-${i}`} className="block text-xs text-gray-600 mb-1">
                    Costo unitario (RF-039)
                  </label>
                  <input
                    id={`det-costo-${i}`}
                    type="number"
                    min={0}
                    step="0.01"
                    value={d.costoUnitario}
                    onChange={(e) => updateDetalle(i, { costoUnitario: e.target.value })}
                    className="w-full px-2 py-1.5 border border-gray-300 rounded-md text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <button
                  onClick={() => removeDetalle(i)}
                  disabled={form.detalles.length === 1}
                  className="px-2 py-1.5 text-sm text-red-600 hover:text-red-800 disabled:opacity-40"
                  title="Quitar producto del ingreso"
                >
                  Quitar
                </button>
              </div>
            ))}
          </div>

          <button
            onClick={addDetalle}
            className="bg-gray-200 text-gray-700 px-3 py-1.5 rounded-md hover:bg-gray-300 transition-colors text-sm mb-4"
          >
            + Agregar producto
          </button>

          <div className="flex gap-2">
            <button
              onClick={handleSubmit}
              disabled={saving}
              className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition-colors text-sm disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {saving ? "Registrando..." : "Registrar ingreso"}
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

      {/* Detalle de un ingreso */}
      {detalle && (
        <div className="mb-6 bg-white border border-gray-200 rounded-lg p-4">
          <div className="flex items-center justify-between mb-3">
            <h2 className="text-lg font-semibold text-gray-800">
              Ingreso #{detalle.id} — {detalle.proveedorNombre}
            </h2>
            <button
              onClick={() => setDetalle(null)}
              className="text-sm text-gray-600 hover:text-gray-800"
            >
              Cerrar
            </button>
          </div>
          <p className="text-sm text-gray-600 mb-3">
            {fmtFecha(detalle.fecha)}
            {detalle.observaciones ? ` · ${detalle.observaciones}` : ""}
          </p>
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-3 py-2 text-left text-sm font-semibold text-gray-700">Producto</th>
                <th className="px-3 py-2 text-right text-sm font-semibold text-gray-700">Cantidad</th>
                <th className="px-3 py-2 text-right text-sm font-semibold text-gray-700">Costo unitario</th>
                <th className="px-3 py-2 text-right text-sm font-semibold text-gray-700">Subtotal</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {detalle.detalles.map((d, i) => (
                <tr key={d.id ?? i}>
                  <td className="px-3 py-2 text-sm text-gray-900">{d.productoNombre}</td>
                  <td className="px-3 py-2 text-sm text-gray-900 text-right">{d.cantidad}</td>
                  <td className="px-3 py-2 text-sm text-gray-600 text-right">{fmtMoney(d.costoUnitario)}</td>
                  <td className="px-3 py-2 text-sm text-gray-900 text-right">
                    {fmtMoney(d.cantidad * d.costoUnitario)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {!loading && !error && ingresos.length === 0 && (
        <p className="text-gray-600">No hay ingresos de mercadería registrados.</p>
      )}

      {!loading && ingresos.length > 0 && (
        <div className="overflow-x-auto rounded-lg border border-gray-200 bg-white">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Fecha</th>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Proveedor</th>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Observaciones</th>
                <th className="px-3 py-3 text-right text-sm font-semibold text-gray-700">Acciones</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {ingresos.map((ing) => (
                <tr key={ing.id} className="hover:bg-gray-50">
                  <td className="px-3 py-2 text-sm text-gray-900">{fmtFecha(ing.fecha)}</td>
                  <td className="px-3 py-2 text-sm text-gray-900">{ing.proveedorNombre}</td>
                  <td className="px-3 py-2 text-sm text-gray-600">{ing.observaciones || "—"}</td>
                  <td className="px-3 py-2 text-sm text-right">
                    <button
                      onClick={() => verDetalle(ing.id)}
                      className="text-blue-600 hover:text-blue-800"
                    >
                      Ver detalle
                    </button>
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
