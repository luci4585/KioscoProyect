"use client";

import { useEffect, useState } from "react";
import { api } from "@/services/api";
import { Rubro } from "@/types";

export default function RubrosPage() {
  const [rubros, setRubros] = useState<Rubro[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api
      .get<Rubro[]>("/api/Rubros")
      .then(setRubros)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, []);

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-800 mb-4">Rubros</h1>

      {loading && <p className="text-gray-600">Cargando rubros...</p>}

      {error && (
        <p className="text-red-600 bg-red-50 border border-red-200 rounded-md p-3">
          Error al cargar los rubros: {error}
        </p>
      )}

      {!loading && !error && rubros.length === 0 && (
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
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
