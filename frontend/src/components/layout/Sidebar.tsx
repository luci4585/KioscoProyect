"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

const menuItems = [
  { label: "Inicio", href: "/" },
  { label: "Rubros", href: "/rubros" },
  { label: "Productos", href: "/productos" },
  { label: "Clientes", href: "/clientes" },
  { label: "Proveedores", href: "/proveedores" },
  { label: "Stock", href: "/stock" },
  { label: "Ventas", href: "/ventas" },
  { label: "Caja", href: "/caja" },
  { label: "Informes", href: "/informes" },
];

export function Sidebar() {
  const pathname = usePathname();

  return (
    <aside className="w-64 bg-gray-900 text-white flex flex-col">
      <div className="p-4 border-b border-gray-700">
        <h1 className="text-xl font-bold">Kiosco</h1>
        <p className="text-sm text-gray-400">Sistema de Gestión</p>
      </div>
      <nav className="flex-1 p-4">
        <ul className="space-y-2">
          {menuItems.map((item) => (
            <li key={item.href}>
              <Link
                href={item.href}
                className={`block px-3 py-2 rounded-md transition-colors ${
                  pathname === item.href
                    ? "bg-blue-600 text-white"
                    : "text-gray-300 hover:bg-gray-800 hover:text-white"
                }`}
              >
                {item.label}
              </Link>
            </li>
          ))}
        </ul>
      </nav>
      <div className="p-4 border-t border-gray-700">
        <p className="text-xs text-gray-500">v0.1.0</p>
      </div>
    </aside>
  );
}
