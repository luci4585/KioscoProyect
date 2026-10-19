"use client";

import { AuthUser } from "@/lib/AuthProvider";
import { useAuth } from "@/lib/AuthProvider";

export function Header({ user }: { user: AuthUser | null }) {
  const { signOut } = useAuth();

  return (
    <header className="bg-white border-b border-gray-200 px-6 py-4">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-lg font-semibold text-gray-800">
            Panel de Control
          </h2>
        </div>
        <div className="flex items-center gap-4">
          {user ? (
            <>
              <div className="text-right">
                <p className="text-sm font-medium text-gray-800">
                  {user.nombre} {user.apellido}
                </p>
                <p className="text-xs text-gray-500">{user.rolNombre}</p>
              </div>
              <button
                onClick={signOut}
                className="text-sm text-red-600 hover:text-red-800"
              >
                Cerrar sesión
              </button>
            </>
          ) : (
            <span className="text-sm text-gray-500">No autenticado</span>
          )}
        </div>
      </div>
    </header>
  );
}
