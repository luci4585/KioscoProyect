"use client";

import { useCallback, useEffect, useState } from "react";
import { api, API_BASE_URL } from "@/services/api";
import { getIdToken } from "@/lib/auth";
import {
  Caja,
  InformeGanancia,
  InformeVentas,
  ProductoVendido,
  ResumenDiario,
  Stock,
  Usuario,
} from "@/types";

const fmtMoney = (n: number) => `$${n.toLocaleString("es-AR", { minimumFractionDigits: 2 })}`;
const fmtFecha = (iso: string) => new Date(iso).toLocaleString("es-AR");

const hoy = () => new Date().toISOString().slice(0, 10);

const METODOS_PAGO = ["Efectivo", "Débito", "Crédito", "Transferencia"];

// RF-072 / RNF-059: tipos de exportación disponibles.
const EXPORTACIONES = [
  { tipo: "ventas", label: "Ventas" },
  { tipo: "productos", label: "Productos" },
  { tipo: "stock", label: "Stock" },
  { tipo: "movimientos-caja", label: "Movimientos de caja" },
  { tipo: "ingresos-mercaderia", label: "Ingresos de mercadería" },
  { tipo: "informes-diarios", label: "Informes diarios" },
  { tipo: "ganancias", label: "Ganancias por período" },
];

export default function InformesPage() {
  // RF-064 / RF-071: resumen del día seleccionado
  const [fecha, setFecha] = useState(hoy());
  const [resumen, setResumen] = useState<ResumenDiario | null>(null);

  // RF-059 a RF-063: informes por período
  const [desde, setDesde] = useState(hoy());
  const [hasta, setHasta] = useState(hoy());
  const [informeVentas, setInformeVentas] = useState<InformeVentas | null>(null);
  const [ganancia, setGanancia] = useState<InformeGanancia | null>(null);
  const [masVendidos, setMasVendidos] = useState<ProductoVendido[]>([]);
  const [cajasPeriodo, setCajasPeriodo] = useState<Caja[]>([]);
  const [stockBajo, setStockBajo] = useState<Stock[] | null>(null);
  const [top, setTop] = useState("10");

  // RF-060: filtros de ventas del período
  const [filtroMetodo, setFiltroMetodo] = useState("");
  const [filtroCliente, setFiltroCliente] = useState("");
  const [filtroUsuario, setFiltroUsuario] = useState("");
  const [usuarios, setUsuarios] = useState<Usuario[]>([]);
  const [clientes, setClientes] = useState<{ id: number; nombre: string; apellido: string }[]>([]);

  const [loading, setLoading] = useState(true);
  const [exportando, setExportando] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);

  const loadResumen = useCallback(async (f: string) => {
    try {
      const data = await api.get<ResumenDiario>(`/api/Informes/resumen?fecha=${f}`);
      setResumen(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cargar el resumen");
    } finally {
      setLoading(false);
    }
  }, []);

  const loadPeriodo = useCallback(
    async (d: string, h: string, metodo: string, cliente: string, usuario: string, topN: string) => {
      setError(null);
      try {
        const qs = `desde=${d}&hasta=${h}`;
        const ventasQs =
          `${qs}` +
          (metodo ? `&metodoPago=${encodeURIComponent(metodo)}` : "") +
          (cliente ? `&clienteId=${cliente}` : "") +
          (usuario ? `&usuarioId=${usuario}` : "");
        const [ventas, gan, topVendidos, cajas] = await Promise.all([
          api.get<InformeVentas>(`/api/Informes/ventas?${ventasQs}`),
          api.get<InformeGanancia>(`/api/Informes/ganancia?${qs}`),
          api.get<ProductoVendido[]>(
            `/api/Informes/mas-vendidos?${qs}&top=${encodeURIComponent(topN || "0")}`
          ),
          api.get<Caja[]>(`/api/Informes/caja?${qs}`),
        ]);
        setInformeVentas(ventas);
        setGanancia(gan);
        setMasVendidos(topVendidos);
        setCajasPeriodo(cajas);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Error al cargar los informes");
      }
    },
    []
  );

  // Carga inicial (RF-071 + listas para filtros RF-060). Fetch encadenado en
  // el efecto: no invocar setState directo dentro del mismo.
  useEffect(() => {
    api
      .get<ResumenDiario>(`/api/Informes/resumen?fecha=${hoy()}`)
      .then(setResumen)
      .catch((err) =>
        setError(err instanceof Error ? err.message : "Error al cargar el resumen")
      )
      .finally(() => setLoading(false));
    api
      .get<InformeVentas>(`/api/Informes/ventas?desde=${hoy()}&hasta=${hoy()}`)
      .then(setInformeVentas)
      .catch(() => undefined);
    api
      .get<InformeGanancia>(`/api/Informes/ganancia?desde=${hoy()}&hasta=${hoy()}`)
      .then(setGanancia)
      .catch(() => undefined);
    api
      .get<ProductoVendido[]>(`/api/Informes/mas-vendidos?desde=${hoy()}&hasta=${hoy()}&top=10`)
      .then(setMasVendidos)
      .catch(() => undefined);
    api
      .get<Caja[]>(`/api/Informes/caja?desde=${hoy()}&hasta=${hoy()}`)
      .then(setCajasPeriodo)
      .catch(() => undefined);
    api
      .get<{ id: number; nombre: string; apellido: string }[]>("/api/Clientes")
      .then(setClientes)
      .catch(() => undefined);
    api
      .get<Usuario[]>("/api/usuarios")
      .then(setUsuarios)
      .catch(() => undefined); // requiere usuarios.gestionar: si no, el filtro queda vacío
  }, []);

  const buscarFecha = async () => {
    setLoading(true);
    await loadResumen(fecha);
  };

  const buscarPeriodo = async () => {
    setLoading(true);
    await loadPeriodo(desde, hasta, filtroMetodo, filtroCliente, filtroUsuario, top);
    setLoading(false);
  };

  const mostrarInfo = (msg: string) => {
    setInfo(msg);
    setTimeout(() => setInfo(null), 4000);
  };

  // RF-072: descarga del Excel generado en el backend (RNF-058).
  const exportar = async (tipo: string) => {
    setError(null);
    setExportando(tipo);
    try {
      const token = await getIdToken();
      const response = await fetch(
        `${API_BASE_URL}/api/Informes/exportar/${tipo}?desde=${desde}&hasta=${hasta}`,
        { headers: { Authorization: `Bearer ${token}` } }
      );
      if (!response.ok) {
        const body = await response.json().catch(() => null);
        throw new Error(body?.error ?? `Error al exportar (HTTP ${response.status})`);
      }
      const blob = await response.blob();
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = `informe_${tipo}_${desde.replace(/-/g, "")}_${hasta.replace(/-/g, "")}.xlsx`;
      document.body.appendChild(a);
      a.click();
      a.remove();
      URL.revokeObjectURL(url);
      mostrarInfo(`Exportación '${tipo}' descargada.`);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al exportar");
    } finally {
      setExportando(null);
    }
  };

  const stockBajoSection = async () => {
    setError(null);
    try {
      const data = await api.get<Stock[]>("/api/Stock/alertas");
      setStockBajo(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cargar el stock bajo");
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold text-gray-800">Informes y dashboard</h1>
      </div>

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

      {/* ── RF-071: indicadores del día ─────────────────────────────────── */}
      <div className="mb-4 bg-white border border-gray-200 rounded-lg p-4">
        <div className="flex flex-wrap items-end gap-3 mb-4">
          <div>
            <label htmlFor="inf-fecha" className="block text-sm font-medium text-gray-700 mb-1">
              Fecha del resumen (RF-064 / RF-070)
            </label>
            <input
              id="inf-fecha"
              type="date"
              value={fecha}
              onChange={(e) => setFecha(e.target.value)}
              className="px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <button
            onClick={buscarFecha}
            disabled={loading}
            className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition-colors text-sm disabled:opacity-50"
          >
            {loading ? "Cargando..." : "Ver resumen"}
          </button>
        </div>

        {resumen && (
          <>
            <div className="grid grid-cols-2 md:grid-cols-5 gap-3 mb-4">
              <div className="bg-blue-50 rounded-md p-3">
                <p className="text-xs text-gray-600">Total vendido</p>
                <p className="text-lg font-semibold text-blue-800">{fmtMoney(resumen.totalVentas)}</p>
              </div>
              <div className="bg-green-50 rounded-md p-3">
                <p className="text-xs text-gray-600">Ganancia (RF-065)</p>
                <p className="text-lg font-semibold text-green-700">{fmtMoney(resumen.ganancia)}</p>
              </div>
              <div className="bg-gray-50 rounded-md p-3">
                <p className="text-xs text-gray-600">Dinero disponible en caja (RF-067)</p>
                <p className="text-lg font-semibold text-gray-900">{fmtMoney(resumen.montoFinalCaja)}</p>
              </div>
              <div className="bg-gray-50 rounded-md p-3">
                <p className="text-xs text-gray-600">Cantidad de ventas</p>
                <p className="text-lg font-semibold text-gray-900">{resumen.cantidadVentas}</p>
              </div>
              <div className="bg-gray-50 rounded-md p-3">
                <p className="text-xs text-gray-600">Productos vendidos</p>
                <p className="text-lg font-semibold text-gray-900">{resumen.productosVendidos}</p>
              </div>
            </div>

            {/* RF-064 / RF-068: detalle del resumen y cierre diario */}
            <div className="grid grid-cols-2 md:grid-cols-4 gap-3 text-sm">
              <div className="border border-gray-200 rounded-md p-2">
                <p className="text-xs text-gray-500">Monto inicial de caja</p>
                <p className="font-semibold text-gray-900">{fmtMoney(resumen.montoInicialCaja)}</p>
              </div>
              <div className="border border-gray-200 rounded-md p-2">
                <p className="text-xs text-gray-500">Total de ingresos</p>
                <p className="font-semibold text-green-700">{fmtMoney(resumen.totalIngresos)}</p>
              </div>
              <div className="border border-gray-200 rounded-md p-2">
                <p className="text-xs text-gray-500">Total de egresos</p>
                <p className="font-semibold text-red-600">{fmtMoney(resumen.totalEgresos)}</p>
              </div>
              <div className="border border-gray-200 rounded-md p-2">
                <p className="text-xs text-gray-500">Costo de productos vendidos</p>
                <p className="font-semibold text-gray-900">{fmtMoney(resumen.costoVendido)}</p>
              </div>
              <div className="border border-gray-200 rounded-md p-2">
                <p className="text-xs text-gray-500">Caja</p>
                <p className="font-semibold text-gray-900">
                  {resumen.cajaAbierta ? "Abierta" : "Cerrada / sin caja"}
                </p>
              </div>
              <div className="border border-gray-200 rounded-md p-2">
                <p className="text-xs text-gray-500">Apertura (RF-068)</p>
                <p className="font-semibold text-gray-900">{resumen.usuarioApertura || "—"}</p>
              </div>
              <div className="border border-gray-200 rounded-md p-2">
                <p className="text-xs text-gray-500">Cierre (RF-068)</p>
                <p className="font-semibold text-gray-900">{resumen.usuarioCierre || "—"}</p>
              </div>
              <div className="border border-gray-200 rounded-md p-2">
                <p className="text-xs text-gray-500">Fecha de la jornada</p>
                <p className="font-semibold text-gray-900">{new Date(resumen.fecha).toLocaleDateString("es-AR")}</p>
              </div>
            </div>

            {resumen.ventasPorMetodoPago.length > 0 && (
              <div className="mt-3">
                <p className="text-sm font-medium text-gray-700 mb-1">Ventas por método de pago (RF-068)</p>
                <div className="flex flex-wrap gap-2">
                  {resumen.ventasPorMetodoPago.map((m) => (
                    <span
                      key={m.metodoPago}
                      className="bg-gray-100 text-gray-700 rounded-md px-2 py-1 text-xs"
                    >
                      {m.metodoPago}: {m.cantidad} · {fmtMoney(m.total)}
                    </span>
                  ))}
                </div>
              </div>
            )}
          </>
        )}
      </div>

      {/* ── Informes por período (RF-059, RF-060, RF-063, RF-066) ──────── */}
      <div className="mb-4 bg-white border border-gray-200 rounded-lg p-4">
        <h2 className="text-lg font-semibold text-gray-800 mb-3">Informes por período (RF-070)</h2>

        <div className="grid grid-cols-2 md:grid-cols-5 gap-3 mb-3">
          <div>
            <label htmlFor="inf-desde" className="block text-xs text-gray-600 mb-1">Desde</label>
            <input
              id="inf-desde"
              type="date"
              value={desde}
              onChange={(e) => setDesde(e.target.value)}
              className="w-full px-2 py-1.5 border border-gray-300 rounded-md text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div>
            <label htmlFor="inf-hasta" className="block text-xs text-gray-600 mb-1">Hasta</label>
            <input
              id="inf-hasta"
              type="date"
              value={hasta}
              onChange={(e) => setHasta(e.target.value)}
              className="w-full px-2 py-1.5 border border-gray-300 rounded-md text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div>
            <label htmlFor="inf-metodo" className="block text-xs text-gray-600 mb-1">Método de pago (RF-060)</label>
            <select
              id="inf-metodo"
              value={filtroMetodo}
              onChange={(e) => setFiltroMetodo(e.target.value)}
              className="w-full px-2 py-1.5 border border-gray-300 rounded-md text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Todos</option>
              {METODOS_PAGO.map((m) => (
                <option key={m} value={m}>{m}</option>
              ))}
            </select>
          </div>
          <div>
            <label htmlFor="inf-cliente" className="block text-xs text-gray-600 mb-1">Cliente (RF-060)</label>
            <select
              id="inf-cliente"
              value={filtroCliente}
              onChange={(e) => setFiltroCliente(e.target.value)}
              className="w-full px-2 py-1.5 border border-gray-300 rounded-md text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Todos</option>
              {clientes.map((c) => (
                <option key={c.id} value={c.id}>{c.nombre} {c.apellido}</option>
              ))}
            </select>
          </div>
          <div>
            <label htmlFor="inf-usuario" className="block text-xs text-gray-600 mb-1">Usuario (RF-060)</label>
            <select
              id="inf-usuario"
              value={filtroUsuario}
              onChange={(e) => setFiltroUsuario(e.target.value)}
              disabled={usuarios.length === 0}
              className="w-full px-2 py-1.5 border border-gray-300 rounded-md text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50"
            >
              <option value="">{usuarios.length === 0 ? "No disponible" : "Todos"}</option>
              {usuarios.map((u) => (
                <option key={u.id} value={u.id}>{u.nombre} {u.apellido}</option>
              ))}
            </select>
          </div>
        </div>

        <div className="flex flex-wrap gap-2 mb-4">
          <button
            onClick={buscarPeriodo}
            disabled={loading}
            className="bg-blue-600 text-white px-4 py-1.5 rounded-md hover:bg-blue-700 transition-colors text-sm disabled:opacity-50"
          >
            {loading ? "Cargando..." : "Generar informes"}
          </button>
          <div className="flex items-center gap-1">
            <label htmlFor="inf-top" className="text-xs text-gray-600">Top:</label>
            <input
              id="inf-top"
              type="number"
              min={1}
              value={top}
              onChange={(e) => setTop(e.target.value)}
              className="w-16 px-2 py-1 border border-gray-300 rounded-md text-sm text-gray-900"
            />
          </div>
          <button
            onClick={stockBajoSection}
            className="bg-red-50 text-red-700 border border-red-200 px-3 py-1.5 rounded-md hover:bg-red-100 transition-colors text-sm"
          >
            Stock bajo (RF-061)
          </button>
        </div>

        {/* Totales del período: RF-059 + ganancia RF-066 */}
        {informeVentas && (
          <div className="grid grid-cols-2 md:grid-cols-4 gap-3 mb-4 text-sm">
            <div className="bg-gray-50 rounded-md p-2">
              <p className="text-xs text-gray-500">Ventas (RF-059)</p>
              <p className="font-semibold text-gray-900">{informeVentas.cantidad}</p>
            </div>
            <div className="bg-gray-50 rounded-md p-2">
              <p className="text-xs text-gray-500">Total vendido</p>
              <p className="font-semibold text-gray-900">{fmtMoney(informeVentas.totalVendido)}</p>
            </div>
            <div className="bg-gray-50 rounded-md p-2">
              <p className="text-xs text-gray-500">Costo vendido</p>
              <p className="font-semibold text-gray-900">{fmtMoney(informeVentas.costoVendido)}</p>
            </div>
            <div className="bg-green-50 rounded-md p-2">
              <p className="text-xs text-gray-600">Ganancia (RF-066)</p>
              <p className="font-semibold text-green-700">
                {fmtMoney(ganancia?.ganancia ?? informeVentas.ganancia)}
              </p>
            </div>
          </div>
        )}

        {/* RF-063: productos más vendidos */}
        {masVendidos.length > 0 && (
          <div className="mb-4">
            <h3 className="text-sm font-semibold text-gray-700 mb-2">Productos más vendidos (RF-063)</h3>
            <table className="min-w-full divide-y divide-gray-200 text-sm">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-3 py-2 text-left font-semibold text-gray-700">#</th>
                  <th className="px-3 py-2 text-left font-semibold text-gray-700">Producto</th>
                  <th className="px-3 py-2 text-left font-semibold text-gray-700">Código</th>
                  <th className="px-3 py-2 text-right font-semibold text-gray-700">Unidades</th>
                  <th className="px-3 py-2 text-right font-semibold text-gray-700">Total vendido</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {masVendidos.map((p, i) => (
                  <tr key={p.productoId} className="hover:bg-gray-50">
                    <td className="px-3 py-2 text-gray-600">{i + 1}</td>
                    <td className="px-3 py-2 text-gray-900">{p.productoNombre}</td>
                    <td className="px-3 py-2 font-mono text-gray-600">{p.productoCodigo}</td>
                    <td className="px-3 py-2 text-right text-gray-900">{p.unidadesVendidas}</td>
                    <td className="px-3 py-2 text-right text-gray-900">{fmtMoney(p.totalVendido)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {/* RF-059/060: detalle de ventas del período */}
        {informeVentas && informeVentas.ventas.length > 0 && (
          <div className="mb-4">
            <h3 className="text-sm font-semibold text-gray-700 mb-2">Ventas del período (RF-059)</h3>
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200 text-sm">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-3 py-2 text-left font-semibold text-gray-700">#</th>
                    <th className="px-3 py-2 text-left font-semibold text-gray-700">Fecha</th>
                    <th className="px-3 py-2 text-left font-semibold text-gray-700">Cliente</th>
                    <th className="px-3 py-2 text-left font-semibold text-gray-700">Vendedor</th>
                    <th className="px-3 py-2 text-left font-semibold text-gray-700">Pago</th>
                    <th className="px-3 py-2 text-right font-semibold text-gray-700">Total</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {informeVentas.ventas.map((v) => (
                    <tr key={v.id} className="hover:bg-gray-50">
                      <td className="px-3 py-2 font-mono text-gray-600">{v.id}</td>
                      <td className="px-3 py-2 text-gray-900">{fmtFecha(v.fecha)}</td>
                      <td className="px-3 py-2 text-gray-600">{v.clienteNombre || "—"}</td>
                      <td className="px-3 py-2 text-gray-600">{v.usuarioNombre}</td>
                      <td className="px-3 py-2 text-gray-600">{v.metodoPago}</td>
                      <td className="px-3 py-2 text-right text-gray-900">{fmtMoney(v.total)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}

        {/* RF-062 + RF-069: caja del período con sus cierres */}
        {cajasPeriodo.length > 0 && (
          <div className="mb-4">
            <h3 className="text-sm font-semibold text-gray-700 mb-2">Caja del período (RF-062) e historial de cierres (RF-069)</h3>
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200 text-sm">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-3 py-2 text-left font-semibold text-gray-700">Apertura</th>
                    <th className="px-3 py-2 text-left font-semibold text-gray-700">Cierre</th>
                    <th className="px-3 py-2 text-right font-semibold text-gray-700">Inicial</th>
                    <th className="px-3 py-2 text-right font-semibold text-gray-700">Ingresos</th>
                    <th className="px-3 py-2 text-right font-semibold text-gray-700">Egresos</th>
                    <th className="px-3 py-2 text-right font-semibold text-gray-700">Saldo</th>
                    <th className="px-3 py-2 text-right font-semibold text-gray-700">Diferencia</th>
                    <th className="px-3 py-2 text-center font-semibold text-gray-700">Estado</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {cajasPeriodo.map((c) => (
                    <tr key={c.id} className="hover:bg-gray-50">
                      <td className="px-3 py-2 text-gray-900">{fmtFecha(c.fechaApertura)}</td>
                      <td className="px-3 py-2 text-gray-600">{c.fechaCierre ? fmtFecha(c.fechaCierre) : "—"}</td>
                      <td className="px-3 py-2 text-right text-gray-900">{fmtMoney(c.montoInicial)}</td>
                      <td className="px-3 py-2 text-right text-green-700">{fmtMoney(c.totalIngresos)}</td>
                      <td className="px-3 py-2 text-right text-red-600">{fmtMoney(c.totalEgresos)}</td>
                      <td className="px-3 py-2 text-right text-gray-900">{fmtMoney(c.saldoCalculado)}</td>
                      <td className="px-3 py-2 text-right text-gray-900">
                        {c.diferencia != null ? fmtMoney(c.diferencia) : "—"}
                      </td>
                      <td className="px-3 py-2 text-center">
                        <span
                          className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${
                            c.abierta ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-500"
                          }`}
                        >
                          {c.abierta ? "Abierta" : "Cerrada"}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}

        {/* RF-061: stock bajo */}
        {stockBajo != null && (
          <div className="mb-4">
            <h3 className="text-sm font-semibold text-gray-700 mb-2">Stock bajo (RF-061)</h3>
            {stockBajo.length === 0 ? (
              <p className="text-sm text-green-700">Sin productos con stock bajo. ✓</p>
            ) : (
              <ul className="text-sm text-gray-700 list-disc pl-5">
                {stockBajo.map((s) => (
                  <li key={s.id}>
                    {s.productoNombre} ({s.productoCodigo}) — {s.cantidad} / mín {s.stockMinimo}
                  </li>
                ))}
              </ul>
            )}
          </div>
        )}
      </div>

      {/* ── RF-072: exportación a Excel ─────────────────────────────────── */}
      <div className="bg-white border border-gray-200 rounded-lg p-4">
        <h2 className="text-lg font-semibold text-gray-800 mb-1">Exportación a Excel (RF-072)</h2>
        <p className="text-xs text-gray-500 mb-3">
          Generado en el backend con ClosedXML (RNF-057/RNF-058) para el período seleccionado.
        </p>
        <div className="flex flex-wrap gap-2">
          {EXPORTACIONES.map((e) => (
            <button
              key={e.tipo}
              onClick={() => exportar(e.tipo)}
              disabled={exportando !== null}
              className="bg-green-600 text-white px-3 py-1.5 rounded-md hover:bg-green-700 transition-colors text-sm disabled:opacity-50"
            >
              {exportando === e.tipo ? "Generando..." : `↓ ${e.label}`}
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}
