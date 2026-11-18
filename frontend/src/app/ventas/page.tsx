"use client";

import { useCallback, useEffect, useState } from "react";
import { api } from "@/services/api";
import { Cliente, Producto, Venta } from "@/types";

interface DetalleForm {
  productoId: string;
  cantidad: string;
}

interface VentaFormData {
  clienteId: string; // "" = sin cliente (RF-045: asociación opcional)
  metodoPago: string;
  detalles: DetalleForm[];
}

const METODOS_PAGO = ["Efectivo", "Débito", "Crédito", "Transferencia"]; // RF-044

const emptyDetalle: DetalleForm = { productoId: "", cantidad: "" };

const emptyForm: VentaFormData = {
  clienteId: "",
  metodoPago: "Efectivo",
  detalles: [{ ...emptyDetalle }],
};

const fmtMoney = (n: number) => `$${n.toLocaleString("es-AR", { minimumFractionDigits: 2 })}`;

export default function VentasPage() {
  const [ventas, setVentas] = useState<Venta[]>([]);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [productos, setProductos] = useState<Producto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);

  // Alta de venta (RF-040)
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState<VentaFormData>(emptyForm);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  // Detalle de una venta (RF-049)
  const [detalle, setDetalle] = useState<Venta | null>(null);

  const loadVentas = useCallback(async () => {
    try {
      const data = await api.get<Venta[]>("/api/Ventas");
      setVentas(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cargar las ventas");
    } finally {
      setLoading(false);
    }
  }, []);

  // Carga inicial (RF-048). Fetch encadenado en el efecto: no invocar setState
  // de forma directa dentro del mismo (react-hooks/set-state-in-effect).
  useEffect(() => {
    api
      .get<Venta[]>("/api/Ventas")
      .then(setVentas)
      .catch((err) =>
        setError(err instanceof Error ? err.message : "Error al cargar las ventas")
      )
      .finally(() => setLoading(false));
    api
      .get<Cliente[]>("/api/Clientes")
      .then(setClientes)
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

  // Total estimado en pantalla (referencia): el backend calcula el definitivo
  // con el precio vigente (RF-042, RF-043).
  const totalEstimado = form.detalles.reduce((sum, d) => {
    const producto = productos.find((p) => String(p.id) === d.productoId);
    const cantidad = Number(d.cantidad);
    if (!producto || !Number.isFinite(cantidad)) return sum;
    return sum + producto.precioVenta * cantidad;
  }, 0);

  const handleSubmit = async () => {
    setFormError(null);

    if (!form.metodoPago) return setFormError("El método de pago es requerido (RF-044).");
    if (form.detalles.length === 0)
      return setFormError("La venta debe tener al menos un producto (RF-041).");

    const detalles: { productoId: number; cantidad: number }[] = [];
    for (const [i, d] of form.detalles.entries()) {
      if (!d.productoId) return setFormError(`Detalle ${i + 1}: seleccione un producto.`);
      const cantidad = Number(d.cantidad);
      if (!Number.isInteger(cantidad) || cantidad <= 0)
        return setFormError(`Detalle ${i + 1}: la cantidad debe ser un entero mayor a cero.`);
      detalles.push({ productoId: Number(d.productoId), cantidad });
    }

    setSaving(true);
    try {
      const venta = await api.post<Venta>("/api/Ventas", {
        clienteId: form.clienteId ? Number(form.clienteId) : null,
        metodoPago: form.metodoPago,
        detalles,
      });
      setInfo(`Venta #${venta.id} registrada por ${fmtMoney(venta.total)}. Stock actualizado.`);
      closeForm();
      await loadVentas();
      setTimeout(() => setInfo(null), 4000);
    } catch (err) {
      setFormError(err instanceof Error ? err.message : "Error al registrar la venta");
    } finally {
      setSaving(false);
    }
  };

  // Anulación (RF-050): conserva la información y restituye el stock.
  const handleAnular = async (venta: Venta) => {
    if (!confirm(`¿Anular la venta #${venta.id} por ${fmtMoney(venta.total)}? Se restituirá el stock.`)) {
      return;
    }
    setError(null);
    try {
      await api.delete(`/api/Ventas/${venta.id}`);
      setInfo(`Venta #${venta.id} anulada. Stock restituido.`);
      if (detalle?.id === venta.id) setDetalle(null);
      await loadVentas();
      setTimeout(() => setInfo(null), 4000);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al anular la venta");
    }
  };

  const verDetalle = async (id: number) => {
    setError(null);
    try {
      const data = await api.get<Venta>(`/api/Ventas/${id}`);
      setDetalle(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cargar el detalle");
    }
  };

  const fmtFecha = (iso: string) => new Date(iso).toLocaleString("es-AR");

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold text-gray-800">Ventas</h1>
        <button
          onClick={openCreate}
          className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition-colors text-sm"
        >
          Nueva venta
        </button>
      </div>

      {loading && <p className="text-gray-600">Cargando ventas...</p>}

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

      {/* Formulario de alta (RF-040 a RF-047) */}
      {showForm && (
        <div className="mb-6 bg-white border border-gray-200 rounded-lg p-4">
          <h2 className="text-lg font-semibold text-gray-800 mb-3">Nueva venta</h2>

          {formError && (
            <p className="text-red-600 bg-red-50 border border-red-200 rounded-md p-3 mb-3 text-sm">
              {formError}
            </p>
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 gap-3 mb-4">
            <div>
              <label htmlFor="v-cliente" className="block text-sm font-medium text-gray-700 mb-1">
                Cliente (opcional — RF-045)
              </label>
              <select
                id="v-cliente"
                value={form.clienteId}
                onChange={(e) => setForm({ ...form, clienteId: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">Sin cliente</option>
                {clientes.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.nombre} {c.apellido}
                    {c.documento ? ` (${c.documento})` : ""}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label htmlFor="v-metodo" className="block text-sm font-medium text-gray-700 mb-1">
                Método de pago * (RF-044)
              </label>
              <select
                id="v-metodo"
                value={form.metodoPago}
                onChange={(e) => setForm({ ...form, metodoPago: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                {METODOS_PAGO.map((m) => (
                  <option key={m} value={m}>
                    {m}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <p className="text-sm font-medium text-gray-700 mb-2">Productos vendidos * (RF-041)</p>
          <div className="space-y-2 mb-3">
            {form.detalles.map((d, i) => {
              const producto = productos.find((p) => String(p.id) === d.productoId);
              const cantidad = Number(d.cantidad);
              const subtotal = producto && Number.isFinite(cantidad) ? producto.precioVenta * cantidad : 0;
              return (
                <div key={i} className="flex flex-wrap items-end gap-2 border border-gray-200 rounded-md p-2">
                  <div className="flex-1 min-w-48">
                    <label htmlFor={`v-prod-${i}`} className="block text-xs text-gray-600 mb-1">
                      Producto
                    </label>
                    <select
                      id={`v-prod-${i}`}
                      value={d.productoId}
                      onChange={(e) => updateDetalle(i, { productoId: e.target.value })}
                      className="w-full px-2 py-1.5 border border-gray-300 rounded-md text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                      <option value="">Seleccionar</option>
                      {productos.map((p) => (
                        <option key={p.id} value={p.id}>
                          {p.nombre} ({p.codigo}) — {fmtMoney(p.precioVenta)}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div className="w-24">
                    <label htmlFor={`v-cant-${i}`} className="block text-xs text-gray-600 mb-1">
                      Cantidad
                    </label>
                    <input
                      id={`v-cant-${i}`}
                      type="number"
                      min={1}
                      step={1}
                      value={d.cantidad}
                      onChange={(e) => updateDetalle(i, { cantidad: e.target.value })}
                      className="w-full px-2 py-1.5 border border-gray-300 rounded-md text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                  </div>
                  <div className="w-28 text-sm text-gray-700 pb-1.5">
                    <span className="block text-xs text-gray-600">Subtotal (ref.)</span>
                    {fmtMoney(subtotal)}
                  </div>
                  <button
                    onClick={() => removeDetalle(i)}
                    disabled={form.detalles.length === 1}
                    className="px-2 py-1.5 text-sm text-red-600 hover:text-red-800 disabled:opacity-40"
                    title="Quitar producto de la venta"
                  >
                    Quitar
                  </button>
                </div>
              );
            })}
          </div>

          <div className="flex items-center gap-3 mb-4">
            <button
              onClick={addDetalle}
              className="bg-gray-200 text-gray-700 px-3 py-1.5 rounded-md hover:bg-gray-300 transition-colors text-sm"
            >
              + Agregar producto
            </button>
            <span className="text-sm text-gray-700">
              Total estimado: <strong>{fmtMoney(totalEstimado)}</strong>
              <span className="text-gray-400 text-xs"> (el definitivo lo calcula el backend — RF-043)</span>
            </span>
          </div>

          <div className="flex gap-2">
            <button
              onClick={handleSubmit}
              disabled={saving}
              className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition-colors text-sm disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {saving ? "Registrando..." : "Registrar venta"}
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

      {/* Detalle de una venta (RF-049) */}
      {detalle && (
        <div className="mb-6 bg-white border border-gray-200 rounded-lg p-4">
          <div className="flex items-center justify-between mb-3">
            <h2 className="text-lg font-semibold text-gray-800">
              Venta #{detalle.id} — {fmtMoney(detalle.total)}
            </h2>
            <button
              onClick={() => setDetalle(null)}
              className="text-sm text-gray-600 hover:text-gray-800"
            >
              Cerrar
            </button>
          </div>
          <p className="text-sm text-gray-600 mb-3">
            {fmtFecha(detalle.fecha)} · Vendedor: {detalle.usuarioNombre} · Cliente:{" "}
            {detalle.clienteNombre || "Sin cliente"} · {detalle.metodoPago}
            {detalle.anulada ? " · ANULADA" : ""}
          </p>
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-3 py-2 text-left text-sm font-semibold text-gray-700">Producto</th>
                <th className="px-3 py-2 text-right text-sm font-semibold text-gray-700">Cantidad</th>
                <th className="px-3 py-2 text-right text-sm font-semibold text-gray-700">Precio unitario</th>
                <th className="px-3 py-2 text-right text-sm font-semibold text-gray-700">Subtotal</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {detalle.detalles.map((d, i) => (
                <tr key={d.id ?? i}>
                  <td className="px-3 py-2 text-sm text-gray-900">{d.productoNombre}</td>
                  <td className="px-3 py-2 text-sm text-gray-900 text-right">{d.cantidad}</td>
                  <td className="px-3 py-2 text-sm text-gray-600 text-right">{fmtMoney(d.precioUnitario)}</td>
                  <td className="px-3 py-2 text-sm text-gray-900 text-right">{fmtMoney(d.subtotal)}</td>
                </tr>
              ))}
            </tbody>
            <tfoot>
              <tr className="bg-gray-50">
                <td colSpan={3} className="px-3 py-2 text-sm font-semibold text-gray-700 text-right">
                  Total (RF-043)
                </td>
                <td className="px-3 py-2 text-sm font-bold text-gray-900 text-right">
                  {fmtMoney(detalle.total)}
                </td>
              </tr>
            </tfoot>
          </table>
        </div>
      )}

      {!loading && !error && ventas.length === 0 && (
        <p className="text-gray-600">No hay ventas registradas.</p>
      )}

      {!loading && ventas.length > 0 && (
        <div className="overflow-x-auto rounded-lg border border-gray-200 bg-white">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">#</th>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Fecha</th>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Cliente</th>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Vendedor</th>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Pago</th>
                <th className="px-3 py-3 text-right text-sm font-semibold text-gray-700">Total</th>
                <th className="px-3 py-3 text-center text-sm font-semibold text-gray-700">Estado</th>
                <th className="px-3 py-3 text-right text-sm font-semibold text-gray-700">Acciones</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {ventas.map((v) => (
                <tr key={v.id} className="hover:bg-gray-50">
                  <td className="px-3 py-2 text-sm text-gray-900 font-mono">{v.id}</td>
                  <td className="px-3 py-2 text-sm text-gray-900">{fmtFecha(v.fecha)}</td>
                  <td className="px-3 py-2 text-sm text-gray-600">{v.clienteNombre || "—"}</td>
                  <td className="px-3 py-2 text-sm text-gray-600">{v.usuarioNombre}</td>
                  <td className="px-3 py-2 text-sm text-gray-600">{v.metodoPago}</td>
                  <td className="px-3 py-2 text-sm text-gray-900 text-right">{fmtMoney(v.total)}</td>
                  <td className="px-3 py-2 text-center text-sm">
                    <span
                      className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${
                        v.anulada
                          ? "bg-gray-100 text-gray-500"
                          : "bg-green-100 text-green-700"
                      }`}
                    >
                      {v.anulada ? "Anulada" : "Activa"}
                    </span>
                  </td>
                  <td className="px-3 py-2 text-sm text-right whitespace-nowrap">
                    <button
                      onClick={() => verDetalle(v.id)}
                      className="text-blue-600 hover:text-blue-800 mr-3"
                    >
                      Detalle
                    </button>
                    {!v.anulada && (
                      <button
                        onClick={() => handleAnular(v)}
                        className="text-red-600 hover:text-red-800"
                        title="Anular venta (RF-050)"
                      >
                        Anular
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
