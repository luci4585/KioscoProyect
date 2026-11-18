"use client";

import { useCallback, useEffect, useState } from "react";
import { api } from "@/services/api";
import { Stock } from "@/types";

interface MovimientoForm {
  productoId: number;
  productoNombre: string;
  tipo: "ingreso" | "egreso";
  cantidad: string;
}

export default function StockPage() {
  const [stocks, setStocks] = useState<Stock[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);

  // Vista: "todos" (RF-021) o "alertas" (RF-026)
  const [vista, setVista] = useState<"todos" | "alertas">("todos");

  // Movimientos manuales: ingreso (RF-022) / egreso (RF-023)
  const [movimiento, setMovimiento] = useState<MovimientoForm | null>(null);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const loadStock = useCallback(async (v: "todos" | "alertas") => {
    try {
      const endpoint = v === "alertas" ? "/api/Stock/alertas" : "/api/Stock";
      const data = await api.get<Stock[]>(endpoint);
      setStocks(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cargar el stock");
    } finally {
      setLoading(false);
    }
  }, []);

  // Carga inicial (RF-021). Fetch encadenado en el efecto: no invocar
  // setState de forma directa dentro del mismo (react-hooks/set-state-in-effect).
  useEffect(() => {
    api
      .get<Stock[]>("/api/Stock")
      .then(setStocks)
      .catch((err) =>
        setError(err instanceof Error ? err.message : "Error al cargar el stock")
      )
      .finally(() => setLoading(false));
  }, []);

  const cambiarVista = async (v: "todos" | "alertas") => {
    setVista(v);
    setLoading(true);
    await loadStock(v);
  };

  const openMovimiento = (stock: Stock, tipo: "ingreso" | "egreso") => {
    setMovimiento({
      productoId: stock.productoId,
      productoNombre: stock.productoNombre,
      tipo,
      cantidad: "",
    });
    setFormError(null);
  };

  const closeMovimiento = () => {
    setMovimiento(null);
    setFormError(null);
  };

  const handleSubmit = async () => {
    setFormError(null);
    if (!movimiento) return;

    const cantidad = Number(movimiento.cantidad);
    if (!Number.isInteger(cantidad) || cantidad <= 0) {
      setFormError("La cantidad debe ser un número entero mayor a cero.");
      return;
    }

    setSaving(true);
    try {
      await api.post(`/api/Stock/${movimiento.tipo}`, {
        productoId: movimiento.productoId,
        cantidad,
      });
      setInfo(
        movimiento.tipo === "ingreso"
          ? `Ingreso de ${cantidad} unidades registrado.`
          : `Egreso de ${cantidad} unidades registrado.`
      );
      closeMovimiento();
      await loadStock(vista);
      setTimeout(() => setInfo(null), 3000);
    } catch (err) {
      setFormError(err instanceof Error ? err.message : "Error al registrar el movimiento");
    } finally {
      setSaving(false);
    }
  };

  const fmtCantidad = (s: Stock) => (
    <span
      className={s.stockBajo ? "text-red-600 font-medium" : "text-gray-900"}
      title={s.stockBajo ? "Stock igual o inferior al mínimo" : undefined}
    >
      {s.cantidad}
    </span>
  );

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold text-gray-800">Stock</h1>
        <div className="flex gap-2">
          <button
            onClick={() => cambiarVista("todos")}
            className={`px-4 py-2 rounded-md text-sm transition-colors ${
              vista === "todos"
                ? "bg-blue-600 text-white"
                : "bg-gray-200 text-gray-700 hover:bg-gray-300"
            }`}
          >
            Todos
          </button>
          <button
            onClick={() => cambiarVista("alertas")}
            className={`px-4 py-2 rounded-md text-sm transition-colors ${
              vista === "alertas"
                ? "bg-red-600 text-white"
                : "bg-gray-200 text-gray-700 hover:bg-gray-300"
            }`}
          >
            Alertas de stock bajo
          </button>
        </div>
      </div>

      {loading && <p className="text-gray-600">Cargando stock...</p>}

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

      {/* Movimiento manual (RF-022 / RF-023) */}
      {movimiento && (
        <div className="mb-6 bg-white border border-gray-200 rounded-lg p-4">
          <h2 className="text-lg font-semibold text-gray-800 mb-3">
            {movimiento.tipo === "ingreso" ? "Ingreso de stock" : "Egreso de stock"} —{" "}
            {movimiento.productoNombre}
          </h2>

          {formError && (
            <p className="text-red-600 bg-red-50 border border-red-200 rounded-md p-3 mb-3 text-sm">
              {formError}
            </p>
          )}

          <div className="flex flex-wrap items-end gap-3">
            <div>
              <label htmlFor="mov-cantidad" className="block text-sm font-medium text-gray-700 mb-1">
                Cantidad *
              </label>
              <input
                id="mov-cantidad"
                type="number"
                min={1}
                step={1}
                value={movimiento.cantidad}
                onChange={(e) => setMovimiento({ ...movimiento, cantidad: e.target.value })}
                onKeyDown={(e) => e.key === "Enter" && handleSubmit()}
                className="w-32 px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Ej: 10"
              />
            </div>
            <button
              onClick={handleSubmit}
              disabled={saving}
              className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition-colors text-sm disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {saving ? "Registrando..." : "Confirmar"}
            </button>
            <button
              onClick={closeMovimiento}
              disabled={saving}
              className="bg-gray-200 text-gray-700 px-4 py-2 rounded-md hover:bg-gray-300 transition-colors text-sm"
            >
              Cancelar
            </button>
          </div>
        </div>
      )}

      {!loading && !error && stocks.length === 0 && vista === "todos" && (
        <p className="text-gray-600">No hay productos con stock registrado.</p>
      )}

      {!loading && stocks.length === 0 && vista === "alertas" && (
        <p className="text-green-700 bg-green-50 border border-green-200 rounded-md p-3">
          No hay productos con stock bajo. ✓
        </p>
      )}

      {!loading && stocks.length > 0 && (
        <div className="overflow-x-auto rounded-lg border border-gray-200 bg-white">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Producto</th>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Código</th>
                <th className="px-3 py-3 text-right text-sm font-semibold text-gray-700">Cantidad</th>
                <th className="px-3 py-3 text-right text-sm font-semibold text-gray-700">Mínimo</th>
                <th className="px-3 py-3 text-center text-sm font-semibold text-gray-700">Estado</th>
                <th className="px-3 py-3 text-right text-sm font-semibold text-gray-700">Acciones</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {stocks.map((s) => (
                <tr key={s.id} className="hover:bg-gray-50">
                  <td className="px-3 py-2 text-sm text-gray-900">{s.productoNombre}</td>
                  <td className="px-3 py-2 text-sm text-gray-600 font-mono">{s.productoCodigo}</td>
                  <td className="px-3 py-2 text-sm text-right">{fmtCantidad(s)}</td>
                  <td className="px-3 py-2 text-sm text-gray-600 text-right">{s.stockMinimo}</td>
                  <td className="px-3 py-2 text-center text-sm">
                    <span
                      className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${
                        s.stockBajo
                          ? "bg-red-100 text-red-700"
                          : "bg-green-100 text-green-700"
                      }`}
                    >
                      {s.stockBajo ? "Stock bajo" : "OK"}
                    </span>
                  </td>
                  <td className="px-3 py-2 text-sm text-right whitespace-nowrap">
                    <button
                      onClick={() => openMovimiento(s, "ingreso")}
                      className="text-green-600 hover:text-green-800 mr-3"
                      title="Registrar ingreso (RF-022)"
                    >
                      Ingresar
                    </button>
                    <button
                      onClick={() => openMovimiento(s, "egreso")}
                      className="text-orange-600 hover:text-orange-800"
                      title="Registrar egreso (RF-023)"
                      disabled={s.cantidad === 0}
                    >
                      Egresar
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
