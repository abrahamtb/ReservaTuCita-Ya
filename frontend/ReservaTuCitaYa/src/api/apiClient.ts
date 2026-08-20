import type { ProblemDetails } from '../types'

const apiUrl = (import.meta.env.VITE_API_URL as string | undefined)?.replace(/\/$/, '')
if (!apiUrl) throw new Error('Falta VITE_API_URL. Copia .env.example como .env.')

let antiforgeryToken: string | null = null

export class ApiError extends Error {
  constructor(public status: number, public problem?: ProblemDetails) {
    super(problem?.detail ?? problem?.title ?? `Error HTTP ${status}`)
  }
}

async function getAntiforgeryToken(signal?: AbortSignal) {
  const response = await fetch(`${apiUrl}/api/antiforgery/token`, {
    credentials: 'include', signal,
  })
  if (!response.ok) {
    if (response.status === 401) window.dispatchEvent(new Event('auth:unauthorized'))
    throw await toApiError(response)
  }
  const data = await response.json() as { requestToken: string }
  antiforgeryToken = data.requestToken
}

async function toApiError(response: Response) {
  let problem: ProblemDetails | undefined
  try { problem = await response.json() as ProblemDetails } catch { problem = undefined }
  return new ApiError(response.status, problem)
}

export interface ApiDownload {
  blob: Blob
  filename?: string
}

export async function apiDownload(path: string, signal?: AbortSignal): Promise<ApiDownload> {
  const response = await fetch(`${apiUrl}${path}`, { credentials: 'include', signal })
  if (response.status === 401) window.dispatchEvent(new Event('auth:unauthorized'))
  if (!response.ok) throw await toApiError(response)

  const disposition = response.headers.get('Content-Disposition')
  const encoded = disposition?.match(/filename\*=UTF-8''([^;]+)/i)?.[1]
  const plain = disposition?.match(/filename="?([^";]+)"?/i)?.[1]
  return {
    blob: await response.blob(),
    filename: encoded ? decodeURIComponent(encoded) : plain,
  }
}

export async function apiRequest<T>(
  path: string,
  init: RequestInit = {},
  retryAntiforgery = true,
): Promise<T> {
  const method = (init.method ?? 'GET').toUpperCase()
  const mutates = ['POST', 'PUT', 'PATCH', 'DELETE'].includes(method)
  if (mutates && !antiforgeryToken) await getAntiforgeryToken(init.signal ?? undefined)

  const headers = new Headers(init.headers)
  if (init.body && !headers.has('Content-Type')) headers.set('Content-Type', 'application/json')
  if (mutates && antiforgeryToken) headers.set('X-XSRF-TOKEN', antiforgeryToken)

  const response = await fetch(`${apiUrl}${path}`, { ...init, headers, credentials: 'include' })
  if (response.status === 400 && mutates && retryAntiforgery) {
    const body = await response.clone().text()
    if (body.includes('antiforgery')) {
      await getAntiforgeryToken(init.signal ?? undefined)
      return apiRequest<T>(path, init, false)
    }
  }
  if (!response.ok) throw await toApiError(response)
  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}

export function refreshAntiforgeryToken(signal?: AbortSignal) {
  antiforgeryToken = null
  return getAntiforgeryToken(signal)
}

export function queryString(values: Record<string, string | number | boolean | undefined | null>) {
  const query = new URLSearchParams()
  Object.entries(values).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') query.set(key, String(value))
  })
  const result = query.toString()
  return result ? `?${result}` : ''
}
