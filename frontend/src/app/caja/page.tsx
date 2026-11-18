"use client";

import { useCallback, useEffect, useState } from "react";
import { api } from "@/services/api";
import { Caja, MovimientoCaja } from "@/types";

interface MovimientoForm {
  tipoMovimiento: string;
  monto: string;
  descripcion: string;
}

const emptyMovimiento: MovimientoForm = { tipoMovimiento: "Ingreso", monto: "", descripcion: "" };

const fmtMoney = (n: number) => `$${n.toLocaleString("es-AR", { minimumFractionDigits: 2 })}`;

export default function CajaPage() {
  const [cajaAbierta, setCajaAbierta] = useState<Caja | null>(null);
  const [historial, setHistorial] = useState<Caja[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);

  // Apertura (RF-051)
  const [montoApertura, setMontoApertura] = useState("");
  const [abriendo, setAbriendo] = useState(false);

  // Movimientos (RF-052)
  const [movimiento, setMovimiento] = useState<MovimientoForm>(emptyMovimiento);
  const [registrando, setRegistrando] = useState(false);

  // Cierre (RF-056 / RF-057)
  const [showCierre, setShowCierre] = useState(false);
  const [montoFinal, setMontoFinal] = useState("");
  const [cerrando, setCerrando] = useState(false);

  // Detalle del historial (RF-058)
  const [detalle, setDetalle] = useState<Caja | null>(null);

  const [formError, setFormError] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      const [abierta, cajas] = await Promise.all([
        api.get<Caja | null>("/api/Cajas/abierta"),
        api.get<Caja[]>("/api/Cajas"),
      ]);
      setCajaAbierta(abierta);
      setHistorial(cajas);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cargar la caja");
    } finally {
      setLoading(false);
    }
  }, []);

  // Carga inicial (RF-055, RF-058). Fetch encadenado en el efecto: no invocar
  // setState de forma directa dentro del mismo (react-hooks/set-state-in-effect).
  useEffect(() => {
    Promise.all([
      api.get<Caja | null>("/api/Cajas/abierta"),
      api.get<Caja[]>("/api/Cajas"),
    ])
      .then(([abierta, cajas]) => {
        setCajaAbierta(abierta);
        setHistorial(cajas);
      })
      .catch((err) =>
        setError(err instanceof Error ? err.message : "Error al cargar la caja")
      )
      .finally(() => setLoading(false));
  }, []);

  const mostrarInfo = (msg: string) => {
    setInfo(msg);
    setTimeout(() => setInfo(null), 4000);
  };

  // RF-051: apertura con monto inicial.
  const handleAbrir = async () => {
    setFormError(null);
    const monto = Number(montoApertura);
    if (Number.isNaN(monto) || monto < 0) {
      setFormError("El monto inicial no puede ser negativo.");
      return;
    }
    setAbriendo(true);
    try {
      await api.post("/api/Cajas/abrir", { montoInicial: monto });
      mostrarInfo(`Caja abierta con monto inicial ${fmtMoney(monto)}.`);
      setMontoApertura("");
      await load();
    } catch (err) {
      setFormError(err instanceof Error ? err.message : "Error al abrir la caja");
    } finally {
      setAbriendo(false);
    }
  };

  // RF-052: registro de ingresos y egresos.
  const handleRegistrarMovimiento = async () => {
    setFormError(null);
    if (!cajaAbierta) return;
    const monto = Number(movimiento.monto);
    if (Number.isNaN(monto) || monto <= 0) {
      setFormError("El monto debe ser mayor a cero.");
      return;
    }
    setRegistrando(true);
    try {
      await api.post(`/api/Cajas/${cajaAbierta.id}/movimientos`, {
        tipoMovimiento: movimiento.tipoMovimiento,
        monto,
        descripcion: movimiento.descripcion.trim() || null,
      });
      mostrarInfo(
        `${movimiento.tipoMovimiento} de ${fmtMoney(monto)} registrado en caja.`
      );
      setMovimiento(emptyMovimiento);
      await load();
    } catch (err) {
      setFormError(err instanceof Error ? err.message : "Error al registrar el movimiento");
    } finally {
      setRegistrando(false);
    }
  };

  // RF-056 / RF-057: cierre con monto informado y diferencia.
  const handleCerrar = async () => {
    setFormError(null);
    if (!cajaAbierta) return;
    const monto = Number(montoFinal);
    if (Number.isNaN(monto) || monto < 0) {
      setFormError("El monto final informado no puede ser negativo.");
      return;
    }
    setCerrando(true);
    try {
      const caja = await api.post<Caja>(`/api/Cajas/${cajaAbierta.id}/cerrar`, {
        montoFinalInformado: monto,
      });
      const dif = caja.diferencia ?? 0;
      mostrarInfo(
        dif === 0
          ? `Caja cerrada sin diferencias (saldo ${fmtMoney(caja.saldoCalculado)}).`
          : `Caja cerrada con diferencia ${fmtMoney(dif)} (calculado ${fmtMoney(caja.saldoCalculado)}).`
      );
      setShowCierre(false);
      setMontoFinal("");
      setDetalle(null);
      await load();
    } catch (err) {
      setFormError(err instanceof Error ? err.message : "Error al cerrar la caja");
    } finally {
      setCerrando(false);
    }
  };

  const verDetalle = async (id: number) => {
    setError(null);
    try {
      const data = await api.get<Caja>(`/api/Cajas/${id}`);
      setDetalle(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cargar el detalle");
    }
  };

  const fmtFecha = (iso: string) => new Date(iso).toLocaleString("es-AR");

  const renderMovimientos = (movs: MovimientoCaja[]) => (
    <table className="min-w-full divide-y divide-gray-200">
      <thead className="bg-gray-50">
        <tr>
          <th className="px-3 py-2 text-left text-sm font-semibold text-gray-700">Fecha</th>
          <th className="px-3 py-2 text-left text-sm font-semibold text-gray-700">Tipo</th>
          <th className="px-3 py-2 text-left text-sm font-semibold text-gray-700">Descripción</th>
          <th className="px-3 py-2 text-left text-sm font-semibold text-gray-700">Responsable</th>
          <th className="px-3 py-2 text-right text-sm font-semibold text-gray-700">Monto</th>
        </tr>
      </thead>
      <tbody className="divide-y divide-gray-100">
        {movs.map((m) => (
          <tr key={m.id} className="hover:bg-gray-50">
            <td className="px-3 py-2 text-sm text-gray-900">{fmtFecha(m.fecha)}</td>
            <td className="px-3 py-2 text-sm">
              <span
                className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${
                  m.tipoMovimiento === "Ingreso"
                    ? "bg-green-100 text-green-700"
                    : "bg-red-100 text-red-700"
                }`}
              >
                {m.tipoMovimiento}
              </span>
            </td>
            <td className="px-3 py-2 text-sm text-gray-600">{m.descripcion || "—"}</td>
            <td className="px-3 py-2 text-sm text-gray-600">{m.usuarioNombre}</td>
            <td className="px-3 py-2 text-sm text-gray-900 text-right">{fmtMoney(m.monto)}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold text-gray-800">Caja</h1>
        {cajaAbierta && !showCierre && (
          <button
            onClick={() => {
              setShowCierre(true);
              setFormError(null);
            }}
            className="bg-red-600 text-white px-4 py-2 rounded-md hover:bg-red-700 transition-colors text-sm"
          >
            Cerrar caja (RF-056)
          </button>
        )}
      </div>

      {loading && <p className="text-gray-600">Cargando caja...</p>}

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

      {formError && (
        <p className="text-red-600 bg-red-50 border border-red-200 rounded-md p-3 mb-4 text-sm">
          {formError}
        </p>
      )}

      {/* Sin caja abierta → apertura (RF-051) */}
      {!loading && !cajaAbierta && !showCierre && (
        <div className="mb-6 bg-white border border-gray-200 rounded-lg p-4 max-w-md">
          <h2 className="text-lg font-semibold text-gray-800 mb-3">Abrir caja</h2>
          <label htmlFor="caja-monto" className="block text-sm font-medium text-gray-700 mb-1">
            Monto inicial *
          </label>
          <input
            id="caja-monto"
            type="number"
            min={0}
            step="0.01"
            value={montoApertura}
            onChange={(e) => setMontoApertura(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && handleAbrir()}
            className="w-full px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500 mb-3"
            placeholder="Ej: 1000"
          />
          <button
            onClick={handleAbrir}
            disabled={abriendo}
            className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition-colors text-sm disabled:opacity-50"
          >
            {abriendo ? "Abriendo..." : "Abrir caja"}
          </button>
        </div>
      )}

      {/* Caja abierta: resumen (RF-055) + movimientos (RF-052) + cierre (RF-056/057) */}
      {!loading && cajaAbierta && (
        <div className="mb-6 bg-white border border-gray-200 rounded-lg p-4">
          <div className="flex items-center justify-between mb-3">
            <h2 className="text-lg font-semibold text-gray-800">
              Caja #{cajaAbierta.id} — abierta {fmtFecha(cajaAbierta.fechaApertura)}
            </h2>
            <span className="text-sm text-gray-600">
              Apertura: {cajaAbierta.usuarioAperturaNombre}
            </span>
          </div>

          {/* Resumen RF-055 con la fórmula del RoadMap §13 */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-3 mb-4">
            <div className="bg-gray-50 rounded-md p-3">
              <p className="text-xs text-gray-500">Monto inicial</p>
              <p className="text-lg font-semibold text-gray-900">{fmtMoney(cajaAbierta.montoInicial)}</p>
            </div>
            <div className="bg-green-50 rounded-md p-3">
              <p className="text-xs text-gray-600">Ingresos</p>
              <p className="text-lg font-semibold text-green-700">{fmtMoney(cajaAbierta.totalIngresos)}</p>
            </div>
            <div className="bg-red-50 rounded-md p-3">
              <p className="text-xs text-gray-600">Egresos</p>
              <p className="text-lg font-semibold text-red-700">{fmtMoney(cajaAbierta.totalEgresos)}</p>
            </div>
            <div className="bg-blue-50 rounded-md p-3">
              <p className="text-xs text-gray-600">Saldo (inicial + ingresos − egresos)</p>
              <p className="text-lg font-semibold text-blue-800">{fmtMoney(cajaAbierta.saldoCalculado)}</p>
            </div>
          </div>

          {/* Registro de movimientos (RF-052) */}
          {!showCierre && (
            <div className="flex flex-wrap items-end gap-3 border-t border-gray-200 pt-3">
              <div>
                <label htmlFor="mov-tipo" className="block text-xs text-gray-600 mb-1">
                  Tipo
                </label>
                <select
                  id="mov-tipo"
                  value={movimiento.tipoMovimiento}
                  onChange={(e) => setMovimiento({ ...movimiento, tipoMovimiento: e.target.value })}
                  className="px-2 py-1.5 border border-gray-300 rounded-md text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value="Ingreso">Ingreso</option>
                  <option value="Egreso">Egreso</option>
                </select>
              </div>
              <div>
                <label htmlFor="mov-monto" className="block text-xs text-gray-600 mb-1">
                  Monto *
                </label>
                <input
                  id="mov-monto"
                  type="number"
                  min={0.01}
                  step="0.01"
                  value={movimiento.monto}
                  onChange={(e) => setMovimiento({ ...movimiento, monto: e.target.value })}
                  className="w-28 px-2 py-1.5 border border-gray-300 rounded-md text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div className="flex-1 min-w-40">
                <label htmlFor="mov-desc" className="block text-xs text-gray-600 mb-1">
                  Descripción
                </label>
                <input
                  id="mov-desc"
                  type="text"
                  maxLength={200}
                  value={movimiento.descripcion}
                  onChange={(e) => setMovimiento({ ...movimiento, descripcion: e.target.value })}
                  className="w-full px-2 py-1.5 border border-gray-300 rounded-md text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Ej: Insumos, Cobro proveedor..."
                />
              </div>
              <button
                onClick={handleRegistrarMovimiento}
                disabled={registrando}
                className="bg-blue-600 text-white px-4 py-1.5 rounded-md hover:bg-blue-700 transition-colors text-sm disabled:opacity-50"
              >
                {registrando ? "Registrando..." : "Registrar"}
              </button>
            </div>
          )}

          {/* Cierre (RF-056 / RF-057) */}
          {showCierre && (
            <div className="border-t border-gray-200 pt-3 flex flex-wrap items-end gap-3">
              <div>
                <label htmlFor="cierre-monto" className="block text-sm font-medium text-gray-700 mb-1">
                  Monto final informado (físico) *
                </label>
                <input
                  id="cierre-monto"
                  type="number"
                  min={0}
                  step="0.01"
                  value={montoFinal}
                  onChange={(e) => setMontoFinal(e.target.value)}
                  className="w-40 px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Ej: 1280"
                />
              </div>
              <button
                onClick={handleCerrar}
                disabled={cerrando}
                className="bg-red-600 text-white px-4 py-2 rounded-md hover:bg-red-700 transition-colors text-sm disabled:opacity-50"
              >
                {cerrando ? "Cerrando..." : "Confirmar cierre"}
              </button>
              <button
                onClick={() => {
                  setShowCierre(false);
                  setMontoFinal("");
                  setFormError(null);
                }}
                disabled={cerrando}
                className="bg-gray-200 text-gray-700 px-4 py-2 rounded-md hover:bg-gray-300 transition-colors text-sm"
              >
                Cancelar
              </button>
              <span className="text-xs text-gray-500">
                Saldo calculado: {fmtMoney(cajaAbierta.saldoCalculado)} — la diferencia se
                informa tras el cierre (RF-057)
              </span>
            </div>
          )}

          {cajaAbierta.movimientos.length > 0 && (
            <div className="mt-4 overflow-x-auto rounded-lg border border-gray-200">
              {renderMovimientos(cajaAbierta.movimientos)}
            </div>
          )}
        </div>
      )}

      {/* Detalle del historial (RF-054 / RF-058) */}
      {detalle && (
        <div className="mb-6 bg-white border border-gray-200 rounded-lg p-4">
          <div className="flex items-center justify-between mb-3">
            <h2 className="text-lg font-semibold text-gray-800">
              Caja #{detalle.id} — {fmtFecha(detalle.fechaApertura)}
              {detalle.fechaCierre ? ` → ${fmtFecha(detalle.fechaCierre)}` : " (abierta)"}
            </h2>
            <button
              onClick={() => setDetalle(null)}
              className="text-sm text-gray-600 hover:text-gray-800"
            >
              Cerrar
            </button>
          </div>
          <p className="text-sm text-gray-600 mb-3">
            Inicial {fmtMoney(detalle.montoInicial)} · Ingresos {fmtMoney(detalle.totalIngresos)} ·
            Egresos {fmtMoney(detalle.totalEgresos)} · Saldo {fmtMoney(detalle.saldoCalculado)}
            {detalle.montoFinal != null && <> · Informado {fmtMoney(detalle.montoFinal)}</>}
            {detalle.diferencia != null && (
              <>
                {" "}
                · Diferencia{" "}
                <strong className={detalle.diferencia === 0 ? "text-green-700" : "text-red-600"}>
                  {fmtMoney(detalle.diferencia)}
                </strong>
              </>
            )}
          </p>
          {detalle.movimientos.length > 0 ? (
            <div className="overflow-x-auto rounded-lg border border-gray-200">
              {renderMovimientos(detalle.movimientos)}
            </div>
          ) : (
            <p className="text-sm text-gray-500">Sin movimientos en este período.</p>
          )}
        </div>
      )}

      {/* Historial (RF-058) */}
      <div className="bg-white border border-gray-200 rounded-lg p-4">
        <h2 className="text-lg font-semibold text-gray-800 mb-3">Historial de cajas</h2>
        {!loading && historial.length === 0 && (
          <p className="text-gray-600">No hay cajas registradas.</p>
        )}
        {!loading && historial.length > 0 && (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-3 py-2 text-left text-sm font-semibold text-gray-700">#</th>
                  <th className="px-3 py-2 text-left text-sm font-semibold text-gray-700">Apertura</th>
                  <th className="px-3 py-2 text-left text-sm font-semibold text-gray-700">Cierre</th>
                  <th className="px-3 py-2 text-right text-sm font-semibold text-gray-700">Inicial</th>
                  <th className="px-3 py-2 text-right text-sm font-semibold text-gray-700">Final</th>
                  <th className="px-3 py-2 text-right text-sm font-semibold text-gray-700">Diferencia</th>
                  <th className="px-3 py-2 text-center text-sm font-semibold text-gray-700">Estado</th>
                  <th className="px-3 py-2 text-right text-sm font-semibold text-gray-700">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {historial.map((c) => (
                  <tr key={c.id} className="hover:bg-gray-50">
                    <td className="px-3 py-2 text-sm text-gray-900 font-mono">{c.id}</td>
                    <td className="px-3 py-2 text-sm text-gray-900">{fmtFecha(c.fechaApertura)}</td>
                    <td className="px-3 py-2 text-sm text-gray-600">
                      {c.fechaCierre ? fmtFecha(c.fechaCierre) : "—"}
                    </td>
                    <td className="px-3 py-2 text-sm text-gray-900 text-right">
                      {fmtMoney(c.montoInicial)}
                    </td>
                    <td className="px-3 py-2 text-sm text-gray-900 text-right">
                      {c.montoFinal != null ? fmtMoney(c.montoFinal) : "—"}
                    </td>
                    <td className="px-3 py-2 text-sm text-right">
                      {c.diferencia != null ? (
                        <span className={c.diferencia === 0 ? "text-green-700" : "text-red-600"}>
                          {fmtMoney(c.diferencia)}
                        </span>
                      ) : (
                        "—"
                      )}
                    </td>
                    <td className="px-3 py-2 text-center text-sm">
                      <span
                        className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${
                          c.abierta
                            ? "bg-green-100 text-green-700"
                            : "bg-gray-100 text-gray-500"
                        }`}
                      >
                        {c.abierta ? "Abierta" : "Cerrada"}
                      </span>
                    </td>
                    <td className="px-3 py-2 text-sm text-right">
                      <button
                        onClick={() => verDetalle(c.id)}
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
    </div>
  );
}
