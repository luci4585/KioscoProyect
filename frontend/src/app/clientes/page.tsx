"use client";

import { useCallback, useEffect, useState } from "react";
import { api } from "@/services/api";
import { Cliente } from "@/types";

interface ClienteFormData {
  nombre: string;
  apellido: string;
  documento: string;
  telefono: string;
  email: string;
  direccion: string;
}

interface Filtros {
  nombre: string;
  documento: string;
}

const emptyForm: ClienteFormData = {
  nombre: "",
  apellido: "",
  documento: "",
  telefono: "",
  email: "",
  direccion: "",
};

const emptyFiltros: Filtros = { nombre: "", documento: "" };

export default function ClientesPage() {
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);

  // Búsqueda (RF-029)
  const [filtros, setFiltros] = useState<Filtros>(emptyFiltros);

  // Alta (RF-027) / modificación (RF-030)
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<Cliente | null>(null);
  const [form, setForm] = useState<ClienteFormData>(emptyForm);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const loadClientes = useCallback(async () => {
    try {
      const data = await api.get<Cliente[]>("/api/Clientes");
      setClientes(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cargar los clientes");
    } finally {
      setLoading(false);
    }
  }, []);

  // Carga inicial (RF-028). Fetch encadenado en el efecto: no invocar
  // setState de forma directa dentro del mismo (react-hooks/set-state-in-effect).
  useEffect(() => {
    api
      .get<Cliente[]>("/api/Clientes")
      .then(setClientes)
      .catch((err) =>
        setError(err instanceof Error ? err.message : "Error al cargar los clientes")
      )
      .finally(() => setLoading(false));
  }, []);

  const handleSearch = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams();
      if (filtros.nombre.trim()) params.set("nombre", filtros.nombre.trim());
      if (filtros.documento.trim()) params.set("documento", filtros.documento.trim());
      const qs = params.toString();
      const data = await api.get<Cliente[]>(
        qs ? `/api/Clientes/buscar?${qs}` : "/api/Clientes"
      );
      setClientes(data);
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
      .get<Cliente[]>("/api/Clientes")
      .then(setClientes)
      .catch((err) =>
        setError(err instanceof Error ? err.message : "Error al cargar los clientes")
      )
      .finally(() => setLoading(false));
  }, []);

  const openCreate = () => {
    setEditing(null);
    setForm(emptyForm);
    setFormError(null);
    setShowForm(true);
  };

  const openEdit = (cliente: Cliente) => {
    setEditing(cliente);
    setForm({
      nombre: cliente.nombre,
      apellido: cliente.apellido,
      documento: cliente.documento ?? "",
      telefono: cliente.telefono ?? "",
      email: cliente.email ?? "",
      direccion: cliente.direccion ?? "",
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
    if (!form.apellido.trim()) return setFormError("El apellido es requerido.");
    if (form.nombre.length > 100)
      return setFormError("El nombre no puede exceder 100 caracteres.");
    if (form.apellido.length > 100)
      return setFormError("El apellido no puede exceder 100 caracteres.");
    if (form.email.trim() && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email.trim()))
      return setFormError("El email no es válido.");

    setSaving(true);
    try {
      const payload = {
        nombre: form.nombre.trim(),
        apellido: form.apellido.trim(),
        documento: form.documento.trim() || null,
        telefono: form.telefono.trim() || null,
        email: form.email.trim() || null,
        direccion: form.direccion.trim() || null,
      };

      if (editing) {
        await api.put(`/api/Clientes/${editing.id}`, payload);
        setInfo("Cliente modificado correctamente.");
      } else {
        await api.post("/api/Clientes", payload);
        setInfo("Cliente creado correctamente.");
      }

      closeForm();
      await loadClientes();
      setTimeout(() => setInfo(null), 3000);
    } catch (err) {
      setFormError(err instanceof Error ? err.message : "Error al guardar el cliente");
    } finally {
      setSaving(false);
    }
  };

  // Baja lógica (RF-031): el backend nunca elimina físicamente.
  const handleDelete = async (cliente: Cliente) => {
    if (!confirm(`¿Dar de baja el cliente "${cliente.nombre} ${cliente.apellido}"? Se conservará su información histórica.`)) {
      return;
    }

    setError(null);
    try {
      await api.delete(`/api/Clientes/${cliente.id}`);
      setInfo("Cliente dado de baja correctamente.");
      await loadClientes();
      setTimeout(() => setInfo(null), 3000);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al dar de baja el cliente");
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold text-gray-800">Clientes</h1>
        <button
          onClick={openCreate}
          className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition-colors text-sm"
        >
          Nuevo cliente
        </button>
      </div>

      {loading && <p className="text-gray-600">Cargando clientes...</p>}

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

      {/* Búsqueda (RF-029) */}
      <div className="mb-6 bg-white border border-gray-200 rounded-lg p-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
          <div>
            <label htmlFor="f-nombre" className="block text-xs font-medium text-gray-600 mb-1">
              Nombre / apellido
            </label>
            <input
              id="f-nombre"
              type="text"
              value={filtros.nombre}
              onChange={(e) => setFiltros({ ...filtros, nombre: e.target.value })}
              onKeyDown={(e) => e.key === "Enter" && handleSearch()}
              className="w-full px-2 py-1.5 border border-gray-300 rounded-md text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Buscar por nombre o apellido"
            />
          </div>
          <div>
            <label htmlFor="f-documento" className="block text-xs font-medium text-gray-600 mb-1">
              Documento
            </label>
            <input
              id="f-documento"
              type="text"
              value={filtros.documento}
              onChange={(e) => setFiltros({ ...filtros, documento: e.target.value })}
              onKeyDown={(e) => e.key === "Enter" && handleSearch()}
              className="w-full px-2 py-1.5 border border-gray-300 rounded-md text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Buscar por documento"
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

      {/* Formulario alta/modificación (RF-027, RF-030) */}
      {showForm && (
        <div className="mb-6 bg-white border border-gray-200 rounded-lg p-4">
          <h2 className="text-lg font-semibold text-gray-800 mb-3">
            {editing ? "Modificar cliente" : "Nuevo cliente"}
          </h2>

          {formError && (
            <p className="text-red-600 bg-red-50 border border-red-200 rounded-md p-3 mb-3 text-sm">
              {formError}
            </p>
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
            <div>
              <label htmlFor="c-nombre" className="block text-sm font-medium text-gray-700 mb-1">
                Nombre *
              </label>
              <input
                id="c-nombre"
                type="text"
                maxLength={100}
                value={form.nombre}
                onChange={(e) => setForm({ ...form, nombre: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label htmlFor="c-apellido" className="block text-sm font-medium text-gray-700 mb-1">
                Apellido *
              </label>
              <input
                id="c-apellido"
                type="text"
                maxLength={100}
                value={form.apellido}
                onChange={(e) => setForm({ ...form, apellido: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label htmlFor="c-documento" className="block text-sm font-medium text-gray-700 mb-1">
                Documento
              </label>
              <input
                id="c-documento"
                type="text"
                maxLength={20}
                value={form.documento}
                onChange={(e) => setForm({ ...form, documento: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label htmlFor="c-telefono" className="block text-sm font-medium text-gray-700 mb-1">
                Teléfono
              </label>
              <input
                id="c-telefono"
                type="text"
                maxLength={20}
                value={form.telefono}
                onChange={(e) => setForm({ ...form, telefono: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label htmlFor="c-email" className="block text-sm font-medium text-gray-700 mb-1">
                Email
              </label>
              <input
                id="c-email"
                type="email"
                maxLength={150}
                value={form.email}
                onChange={(e) => setForm({ ...form, email: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label htmlFor="c-direccion" className="block text-sm font-medium text-gray-700 mb-1">
                Dirección
              </label>
              <input
                id="c-direccion"
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
              {saving ? "Guardando..." : editing ? "Guardar cambios" : "Crear cliente"}
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

      {!loading && !error && clientes.length === 0 && (
        <p className="text-gray-600">No hay clientes registrados.</p>
      )}

      {!loading && clientes.length > 0 && (
        <div className="overflow-x-auto rounded-lg border border-gray-200 bg-white">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Nombre</th>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Apellido</th>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Documento</th>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Teléfono</th>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Email</th>
                <th className="px-3 py-3 text-left text-sm font-semibold text-gray-700">Dirección</th>
                <th className="px-3 py-3 text-center text-sm font-semibold text-gray-700">Estado</th>
                <th className="px-3 py-3 text-right text-sm font-semibold text-gray-700">Acciones</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {clientes.map((cliente) => (
                <tr key={cliente.id} className="hover:bg-gray-50">
                  <td className="px-3 py-2 text-sm text-gray-900">{cliente.nombre}</td>
                  <td className="px-3 py-2 text-sm text-gray-900">{cliente.apellido}</td>
                  <td className="px-3 py-2 text-sm text-gray-600 font-mono">{cliente.documento || "—"}</td>
                  <td className="px-3 py-2 text-sm text-gray-600">{cliente.telefono || "—"}</td>
                  <td className="px-3 py-2 text-sm text-gray-600">{cliente.email || "—"}</td>
                  <td className="px-3 py-2 text-sm text-gray-600">{cliente.direccion || "—"}</td>
                  <td className="px-3 py-2 text-center text-sm">
                    <span
                      className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${
                        cliente.activo
                          ? "bg-green-100 text-green-700"
                          : "bg-gray-100 text-gray-500"
                      }`}
                    >
                      {cliente.activo ? "Activo" : "Baja"}
                    </span>
                  </td>
                  <td className="px-3 py-2 text-sm text-right whitespace-nowrap">
                    <button
                      onClick={() => openEdit(cliente)}
                      className="text-blue-600 hover:text-blue-800 mr-3"
                    >
                      Editar
                    </button>
                    {cliente.activo && (
                      <button
                        onClick={() => handleDelete(cliente)}
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
