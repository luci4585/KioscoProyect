"use client";

import { useEffect, useState } from "react";
import { onAuthStateChange, signOut } from "@/lib/auth";
import { User } from "firebase/auth";

export function Header() {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const unsubscribe = onAuthStateChange((user) => {
      setUser(user);
      setLoading(false);
    });
    return () => unsubscribe();
  }, []);

  const handleSignOut = async () => {
    await signOut();
  };

  return (
    <header className="bg-white border-b border-gray-200 px-6 py-4">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-lg font-semibold text-gray-800">
            Panel de Control
          </h2>
        </div>
        <div className="flex items-center gap-4">
          {loading ? (
            <span className="text-sm text-gray-500">Cargando...</span>
          ) : user ? (
            <>
              <span className="text-sm text-gray-600">{user.email}</span>
              <button
                onClick={handleSignOut}
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
