"use client";

import { useCallback, useEffect, useState } from "react";
import { api } from "@/services/api";
import { Rubro } from "@/types";

interface RubroFormData {
  nombre: string;
  descripcion: string;
}

const emptyForm: RubroFormData = { nombre: "", descripcion: "" };

export default function RubrosPage() {
  const [rubros, setRubros] = useState<Rubro[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);

  // Formulario de alta (RF-009) / modificación (RF-011)
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<Rubro | null>(null);
  const [form, setForm] = useState<RubroFormData>(emptyForm);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const loadRubros = useCallback(async () => {
    try {
      const data = await api.get<Rubro[]>("/api/Rubros");
      setRubros(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cargar los rubros");
    } finally {
      setLoading(false);
    }
  }, []);

  // Carga inicial (RF-010). El fetch va encadenado en el efecto para no
  // invocar setState de forma directa dentro del mismo (react-hooks/set-state-in-effect).
  useEffect(() => {
    api
      .get<Rubro[]>("/api/Rubros")
      .then(setRubros)
      .catch((err) =>
        setError(err instanceof Error ? err.message : "Error al cargar los rubros")
      )
      .finally(() => setLoading(false));
  }, []);

  const openCreate = () => {
    setEditing(null);
    setForm(emptyForm);
    setFormError(null);
    setShowForm(true);
  };

  const openEdit = (rubro: Rubro) => {
    setEditing(rubro);
    setForm({ nombre: rubro.nombre, descripcion: rubro.descripcion ?? "" });
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

    if (!form.nombre.trim()) {
      setFormError("El nombre es requerido.");
      return;
    }
    if (form.nombre.length > 50) {
      setFormError("El nombre no puede exceder 50 caracteres.");
      return;
    }
    if (form.descripcion.length > 200) {
      setFormError("La descripción no puede exceder 200 caracteres.");
      return;
    }

    setSaving(true);
    try {
      const payload = {
        nombre: form.nombre.trim(),
        descripcion: form.descripcion.trim(),
      };

      if (editing) {
        await api.put(`/api/Rubros/${editing.id}`, payload);
        setInfo("Rubro modificado correctamente.");
      } else {
        await api.post("/api/Rubros", payload);
        setInfo("Rubro creado correctamente.");
      }

      closeForm();
      await loadRubros();
      setTimeout(() => setInfo(null), 3000);
    } catch (err) {
      setFormError(err instanceof Error ? err.message : "Error al guardar el rubro");
    } finally {
      setSaving(false);
    }
  };

  // Baja lógica (RF-012): el backend nunca elimina físicamente.
  const handleDelete = async (rubro: Rubro) => {
    if (!confirm(`¿Dar de baja el rubro "${rubro.nombre}"? Se conservará su información histórica.`)) {
      return;
    }

    setError(null);
    try {
      await api.delete(`/api/Rubros/${rubro.id}`);
      setInfo("Rubro dado de baja correctamente.");
      await loadRubros();
      setTimeout(() => setInfo(null), 3000);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al dar de baja el rubro");
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold text-gray-800">Rubros</h1>
        <button
          onClick={openCreate}
          className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition-colors text-sm"
        >
          Nuevo rubro
        </button>
      </div>

      {loading && <p className="text-gray-600">Cargando rubros...</p>}

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

      {showForm && (
        <div className="mb-6 bg-white border border-gray-200 rounded-lg p-4">
          <h2 className="text-lg font-semibold text-gray-800 mb-3">
            {editing ? "Modificar rubro" : "Nuevo rubro"}
          </h2>

          {formError && (
            <p className="text-red-600 bg-red-50 border border-red-200 rounded-md p-3 mb-3 text-sm">
              {formError}
            </p>
          )}

          <div className="space-y-3">
            <div>
              <label htmlFor="rubro-nombre" className="block text-sm font-medium text-gray-700 mb-1">
                Nombre *
              </label>
              <input
                id="rubro-nombre"
                type="text"
                required
                maxLength={50}
                value={form.nombre}
                onChange={(e) => setForm({ ...form, nombre: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 text-gray-900"
                placeholder="Ej: Golosinas"
              />
            </div>

            <div>
              <label htmlFor="rubro-descripcion" className="block text-sm font-medium text-gray-700 mb-1">
                Descripción
              </label>
              <input
                id="rubro-descripcion"
                type="text"
                maxLength={200}
                value={form.descripcion}
                onChange={(e) => setForm({ ...form, descripcion: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 text-gray-900"
                placeholder="Ej: Bombones, chocolates, caramelos"
              />
            </div>

            <div className="flex gap-2">
              <button
                onClick={handleSubmit}
                disabled={saving}
                className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition-colors text-sm disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {saving ? "Guardando..." : editing ? "Guardar cambios" : "Crear rubro"}
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
        </div>
      )}

      {!loading && !error && !showForm && rubros.length === 0 && (
        <p className="text-gray-600">No hay rubros registrados.</p>
      )}

      {!loading && !error && rubros.length > 0 && (
        <div className="overflow-x-auto rounded-lg border border-gray-200 bg-white">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">
                  Nombre
                </th>
                <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">
                  Descripción
                </th>
                <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">
                  Activo
                </th>
                <th className="px-4 py-3 text-right text-sm font-semibold text-gray-700">
                  Acciones
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {rubros.map((rubro) => (
                <tr key={rubro.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 text-sm text-gray-900">
                    {rubro.nombre}
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-600">
                    {rubro.descripcion || "—"}
                  </td>
                  <td className="px-4 py-3 text-sm">
                    <span
                      className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${
                        rubro.activo
                          ? "bg-green-100 text-green-700"
                          : "bg-gray-100 text-gray-500"
                      }`}
                    >
                      {rubro.activo ? "Sí" : "No"}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-sm text-right whitespace-nowrap">
                    <button
                      onClick={() => openEdit(rubro)}
                      className="text-blue-600 hover:text-blue-800 mr-3"
                    >
                      Editar
                    </button>
                    {rubro.activo && (
                      <button
                        onClick={() => handleDelete(rubro)}
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
