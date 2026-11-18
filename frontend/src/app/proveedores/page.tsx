"use client";

import { useCallback, useEffect, useState } from "react";
import { api } from "@/services/api";
import { Proveedor } from "@/types";

interface ProveedorFormData {
  razonSocial: string;
  cuit: string;
  telefono: string;
  email: string;
  direccion: string;
}

interface Filtros {
  razonSocial: string;
  cuit: string;
}

const emptyForm: ProveedorFormData = {
  razonSocial: "",
  cuit: "",
  telefono: "",
  email: "",
  direccion: "",
};

const emptyFiltros: Filtros = { razonSocial: "", cuit: "" };

export default function ProveedoresPage() {
  const [proveedores, setProveedores] = useState<Proveedor[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);

  // Búsqueda (RoadMap §9)
  const [filtros, setFiltros] = useState<Filtros>(emptyFiltros);

  // Alta (RF-032) / modificación (RF-034)
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<Proveedor | null>(null);
  const [form, setForm] = useState<ProveedorFormData>(emptyForm);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const loadProveedores = useCallback(async () => {
    try {
      const data = await api.get<Proveedor[]>("/api/Proveedores");
      setProveedores(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cargar los proveedores");
    } finally {
      setLoading(false);
    }
  }, []);

  // Carga inicial (RF-033). Fetch encadenado en el efecto: no invocar
  // setState de forma directa dentro del mismo (react-hooks/set-state-in-effect).
  useEffect(() => {
    api
      .get<Proveedor[]>("/api/Proveedores")
      .then(setProveedores)
      .catch((err) =>
        setError(err instanceof Error ? err.message : "Error al cargar los proveedores")
      )
      .finally(() => setLoading(false));
  }, []);

  const handleSearch = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams();
      if (filtros.razonSocial.trim()) params.set("razonSocial", filtros.razonSocial.trim());
      if (filtros.cuit.trim()) params.set("cuit", filtros.cuit.trim());
      const qs = params.toString();
      const data = await api.get<Proveedor[]>(
        qs ? `/api/Proveedores/buscar?${qs}` : "/api/Proveedores"
      );
      setProveedores(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error en la búsqueda");
    } finally {
      setLoading(false);
    }
  }, [filtros]);

  const handleClearFilters = useCallback(() => {
    setFiltros(emptyFiltros);
    setLoading(true);
    api
      .get<Proveedor[]>("/api/Proveedores")
      .then(setProveedores)
      .catch((err) =>
        setError(err instanceof Error ? err.message : "Error al cargar los proveedores")
      )
      .finally(() => setLoading(false));
  }, []);

  const openCreate = () => {
    setEditing(null);
    setForm(emptyForm);
    setFormError(null);
    setShowForm(true);
  };

  const openEdit = (proveedor: Proveedor) => {
    setEditing(proveedor);
    setForm({
      razonSocial: proveedor.razonSocial,
      cuit: proveedor.cuit ?? "",
      telefono: proveedor.telefono ?? "",
      email: proveedor.email ?? "",
      direccion: proveedor.direccion ?? "",
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
    if (!form.razonSocial.trim()) return setFormError("La razón social es requerida.");
    if (form.razonSocial.length > 150)
      return setFormError("La razón social no puede exceder 150 caracteres.");
    if (form.cuit.length > 11)
      return setFormError("El CUIT no puede exceder 11 caracteres.");
    if (form.email.trim() && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email.trim()))
      return setFormError("El email no es válido.");

    setSaving(true);
    try {
      const payload = {
        razonSocial: form.razonSocial.trim(),
        cuit: form.cuit.trim() || null,
        telefono: form.telefono.trim() || null,
        email: form.email.trim() || null,
        direccion: form.direccion.trim() || null,
      };

      if (editing) {
        await api.put(`/api/Proveedores/${editing.id}`, payload);
        setInfo("Proveedor modificado correctamente.");
      } else {
        await api.post("/api/Proveedores", payload);
        setInfo("Proveedor creado correctamente.");
      }

      closeForm();
      await loadProveedores();
      setTimeout(() => setInfo(null), 3000);
    } catch (err) {
      setFormError(err instanceof Error ? err.message : "Error al guardar el proveedor");
    } finally {
      setSaving(false);
    }
  };

  // Baja lógica (RF-035): el backend nunca elimina físicamente.
  const handleDelete = async (proveedor: Proveedor) => {
    if (!confirm(`¿Dar de baja el proveedor "${proveedor.razonSocial}"? Se conservará su información histórica.`)) {
      return;
    }

    setError(null);
    try {
      await api.delete(`/api/Proveedores/${proveedor.id}`);
      setInfo("Proveedor dado de baja correctamente.");
      await loadProveedores();
      setTimeout(() => setInfo(null), 3000);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al dar de baja el proveedor");
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold text-gray-800">Proveedores</h1>
        <button
          onClick={openCreate}
          className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition-colors text-sm"
        >
          Nuevo proveedor
        </button>
      </div>

      {loading && <p className="text-gray-600">Cargando proveedores...</p>}

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

      {/* Búsqueda (RoadMap §9) */}
      <div className="mb-6 bg-white border border-gray-200 rounded-lg p-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
          <div>
            <label htmlFor="f-razonsocial" className="block text-xs font-medium text-gray-600 mb-1">
              Razón social
            </label>
            <input
              id="f-razonsocial"
              type="text"
              value={filtros.razonSocial}
              onChange={(e) => setFiltros({ ...filtros, razonSocial: e.target.value })}
              onKeyDown={(e) => e.key === "Enter" && handleSearch()}
              className="w-full px-2 py-1.5 border border-gray-300 rounded-md text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Buscar por razón social"
            />
          </div>
          <div>
            <label htmlFor="f-cuit" className="block text-xs font-medium text-gray-600 mb-1">
              CUIT
            </label>
            <input
              id="f-cuit"
              type="text"
              value={filtros.cuit}
              onChange={(e) => setFiltros({ ...filtros, cuit: e.target.value })}
              onKeyDown={(e) => e.key === "Enter" && handleSearch()}
              className="w-full px-2 py-1.5 border border-gray-300 rounded-md text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Buscar por CUIT"
            />
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

      {/* Formulario alta/modificación (RF-032, RF-034) */}
      {showForm && (
        <div className="mb-6 bg-white border border-gray-200 rounded-lg p-4">
          <h2 className="text-lg font-semibold text-gray-800 mb-3">
            {editing ? "Modificar proveedor" : "Nuevo proveedor"}
          </h2>

          {formError && (
            <p className="text-red-600 bg-red-50 border border-red-200 rounded-md p-3 mb-3 text-sm">
              {formError}
            </p>
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
            <div>
              <label htmlFor="p-razonsocial" className="block text-sm font-medium text-gray-700 mb-1">
                Razón social *
              </label>
              <input
                id="p-razonsocial"
                type="text"
                maxLength={150}
                value={form.razonSocial}
                onChange={(e) => setForm({ ...form, razonSocial: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label htmlFor="p-cuit" className="block text-sm font-medium text-gray-700 mb-1">
                CUIT
              </label>
              <input
                id="p-cuit"
                type="text"
                maxLength={11}
                value={form.cuit}
                onChange={(e) => setForm({ ...form, cuit: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label htmlFor="p-telefono" className="block text-sm font-medium text-gray-700 mb-1">
                Teléfono
              </label>
              <input
                id="p-telefono"
                type="text"
                maxLength={20}
                value={form.telefono}
                onChange={(e) => setForm({ ...form, telefono: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label htmlFor="p-email" className="block text-sm font-medium text-gray-700 mb-1">
                Email
              </label>
              <input
                id="p-email"
                type="email"
                maxLength={150}
                value={form.email}
                onChange={(e) => setForm({ ...form, email: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label htmlFor="p-direccion" className="block text-sm font-medium text-gray-700 mb-1">
                Dirección
              </label>
              <input
                id="p-direccion"
                type="text"
                maxLength={200}
                value={form.direccion}
                onChange={(e) => setForm({ ...form, direccion: e.target.value })}
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
              {saving ? "Guardando..." : editing ? "Guardar cambios" : "Crear proveedor"}
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

      {!loading && !error && proveedores.length === 0 && (
        <p className="text-gray-600">No hay proveedores registrados.</p>
      )}

      {!loading && proveedores.length > 0 && (
        <div className="overflow-x-auto rounded-lg border border-gray-200 bg-white">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Razón social</th>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">CUIT</th>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Teléfono</th>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Email</th>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Dirección</th>
                <th className="px-3 py-3 text-center text-sm font-semibold text-gray-700">Estado</th>
                <th className="px-3 py-3 text-right text-sm font-semibold text-gray-700">Acciones</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {proveedores.map((proveedor) => (
                <tr key={proveedor.id} className="hover:bg-gray-50">
                  <td className="px-3 py-2 text-sm text-gray-900">{proveedor.razonSocial}</td>
                  <td className="px-3 py-2 text-sm text-gray-600 font-mono">{proveedor.cuit || "—"}</td>
                  <td className="px-3 py-2 text-sm text-gray-600">{proveedor.telefono || "—"}</td>
                  <td className="px-3 py-2 text-sm text-gray-600">{proveedor.email || "—"}</td>
                  <td className="px-3 py-2 text-sm text-gray-600">{proveedor.direccion || "—"}</td>
                  <td className="px-3 py-2 text-center text-sm">
                    <span
                      className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${
                        proveedor.activo
                          ? "bg-green-100 text-green-700"
                          : "bg-gray-100 text-gray-500"
                      }`}
                    >
                      {proveedor.activo ? "Activo" : "Baja"}
                    </span>
                  </td>
                  <td className="px-3 py-2 text-sm text-right whitespace-nowrap">
                    <button
                      onClick={() => openEdit(proveedor)}
                      className="text-blue-600 hover:text-blue-800 mr-3"
                    >
                      Editar
                    </button>
                    {proveedor.activo && (
                      <button
                        onClick={() => handleDelete(proveedor)}
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
