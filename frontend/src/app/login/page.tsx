"use client";

import { useState, FormEvent } from "react";
import { useRouter } from "next/navigation";
import { signIn, signUp, resetPassword } from "@/lib/auth";

export default function LoginPage() {
  const router = useRouter();
  const [mode, setMode] = useState<"login" | "register" | "reset">("login");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setInfo(null);
    setLoading(true);

    try {
      if (mode === "reset") {
        await resetPassword(email);
        setInfo(
          "Te enviamos un email con el enlace para restablecer tu contraseña. Revisá tu bandeja de entrada."
        );
        setLoading(false);
        setTimeout(() => {
          setMode("login");
          setInfo(null);
        }, 3000);
        return;
      }

      if (mode === "register") {
        if (password.length < 6) {
          setError("La contraseña debe tener al menos 6 caracteres.");
          setLoading(false);
          return;
        }
        if (password !== confirmPassword) {
          setError("Las contraseñas no coinciden.");
          setLoading(false);
          return;
        }
        await signUp(email, password);
        setInfo(
          "Cuenta creada correctamente. Te enviamos un email de verificación. Revisá tu bandeja de entrada para verificar tu correo."
        );
        setLoading(false);
        setTimeout(() => {
          setMode("login");
          setEmail("");
          setPassword("");
          setConfirmPassword("");
          setInfo(null);
        }, 2500);
      } else {
        await signIn(email, password);
        router.push("/");
      }
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Error en la operación";

      if (mode === "reset") {
        if (message.includes("auth/invalid-email")) {
          setError("El formato del email no es válido.");
        } else if (message.includes("auth/user-not-found")) {
          setError("No existe una cuenta con ese email.");
        } else if (message.includes("auth/too-many-requests")) {
          setError("Demasiados intentos. Espere unos minutos.");
        } else if (message.includes("auth/network-request-failed")) {
          setError("Error de conexión. Verifique su red.");
        } else {
          setError(message);
        }
      } else if (mode === "register") {
        if (message.includes("auth/email-already-in-use")) {
          setError("Ya existe una cuenta con ese email.");
        } else if (message.includes("auth/weak-password")) {
          setError("La contraseña es demasiado débil.");
        } else if (message.includes("auth/invalid-email")) {
          setError("El formato del email no es válido.");
        } else {
          setError(message);
        }
      } else {
        if (
          message.includes("auth/invalid-credential") ||
          message.includes("auth/wrong-password") ||
          message.includes("auth/user-not-found")
        ) {
          setError("Email o contraseña incorrectos.");
        } else if (message.includes("auth/too-many-requests")) {
          setError("Demasiados intentos. Espere unos minutos.");
        } else if (message.includes("auth/network-request-failed")) {
          setError("Error de conexión. Verifique su red.");
        } else if (message.includes("auth/email-not-verified")) {
          setError("Verificá tu email antes de iniciar sesión.");
        } else {
          setError(message);
        }
      }
    }
  };

  const toggleMode = () => {
    setMode(mode === "login" ? "register" : "login");
    setError(null);
    setInfo(null);
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100">
      <div className="w-full max-w-md bg-white rounded-lg shadow-md p-8">
        <h1 className="text-2xl font-bold text-center text-gray-800 mb-2">
          Sistema de Gestión de Kiosco
        </h1>
        <p className="text-center text-gray-500 mb-6">
          {mode === "login"
            ? "Iniciá sesión para continuar"
            : mode === "reset"
              ? "Ingresá tu email para restablecer tu contraseña"
              : "Creá tu cuenta para comenzar"}
        </p>

        {error && (
          <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-md text-sm text-red-600">
            {error}
          </div>
        )}

        {info && (
          <div className="mb-4 p-3 bg-green-50 border border-green-200 rounded-md text-sm text-green-700">
            {info}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label
              htmlFor="email"
              className="block text-sm font-medium text-gray-700 mb-1"
            >
              Email
            </label>
            <input
              id="email"
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 text-gray-900"
              placeholder="ejemplo@mail.com"
            />
          </div>

          {mode !== "reset" && (
            <div>
              <label
                htmlFor="password"
                className="block text-sm font-medium text-gray-700 mb-1"
              >
                Contraseña
              </label>
              <input
                id="password"
                type="password"
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 text-gray-900"
                placeholder="••••••••"
              />
            </div>
          )}

          {mode === "register" && (
            <div>
              <label
                htmlFor="confirmPassword"
                className="block text-sm font-medium text-gray-700 mb-1"
              >
                Confirmar contraseña
              </label>
              <input
                id="confirmPassword"
                type="password"
                required
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 text-gray-900"
                placeholder="••••••••"
              />
            </div>
          )}

          <button
            type="submit"
            disabled={loading}
            className="w-full bg-blue-600 text-white py-2 rounded-md hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {loading
              ? mode === "login"
                ? "Ingresando..."
                : mode === "reset"
                  ? "Enviando..."
                  : "Creando cuenta..."
              : mode === "login"
                ? "Ingresar"
                : mode === "reset"
                  ? "Enviar email de recuperación"
                  : "Registrarme"}
          </button>
        </form>

        <div className="mt-4 text-center space-y-2">
          {mode === "login" && (
            <div>
              <button
                onClick={() => {
                  setMode("reset");
                  setError(null);
                  setInfo(null);
                }}
                className="text-sm text-blue-600 hover:text-blue-800"
              >
                ¿Olvidaste tu contraseña?
              </button>
            </div>
          )}

          <div>
            <button
              onClick={
                mode === "reset"
                  ? () => {
                      setMode("login");
                      setError(null);
                      setInfo(null);
                    }
                  : toggleMode
              }
              className="text-sm text-blue-600 hover:text-blue-800"
            >
              {mode === "reset"
                ? "Volver a iniciar sesión"
                : mode === "login"
                  ? "¿No tenés cuenta? Registrate"
                  : "¿Ya tenés cuenta? Iniciá sesión"}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
