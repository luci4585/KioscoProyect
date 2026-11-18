import { getIdToken } from "@/lib/auth";

export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5009";

async function request<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
  const token = await getIdToken();

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(options.headers as Record<string, string>),
  };

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    throw new Error(await extractErrorMessage(response));
  }

  return response.json();
}

/**
 * Extrae un mensaje legible de una respuesta de error del backend:
 * - ValidationProblemDetails (RFC 7807): { errors: { Campo: [msg] } }
 * - Middleware de excepciones: { error: "..." }
 * - Fallback: mensaje genérico con el código HTTP.
 */
async function extractErrorMessage(response: Response): Promise<string> {
  const fallback = `HTTP error ${response.status}`;
  const data = await response.json().catch(() => null);

  if (!data || typeof data !== "object") return fallback;

  if (typeof data.message === "string" && data.message) return data.message;
  if (typeof data.error === "string" && data.error) return data.error;

  if (data.errors && typeof data.errors === "object") {
    const messages = Object.values(data.errors)
      .flat()
      .filter((m): m is string => typeof m === "string" && m.length > 0);
    if (messages.length > 0) return messages.join(" ");
  }

  return fallback;
}

export const api = {
  get: <T>(endpoint: string) => request<T>(endpoint),

  post: <T>(endpoint: string, body: unknown) =>
    request<T>(endpoint, { method: "POST", body: JSON.stringify(body) }),

  put: <T>(endpoint: string, body: unknown) =>
    request<T>(endpoint, { method: "PUT", body: JSON.stringify(body) }),

  delete: <T>(endpoint: string) =>
    request<T>(endpoint, { method: "DELETE" }),
};
