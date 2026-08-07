import { apiRequest, refreshAntiforgeryToken } from './apiClient'
import type { AuthUser } from '../types'

export async function login(email: string, password: string, recordarme: boolean) {
  const user = await apiRequest<AuthUser>('/api/auth/login', {
    method: 'POST', body: JSON.stringify({ email, password, recordarme }),
  })
  await refreshAntiforgeryToken()
  return user
}
export const me = () => apiRequest<AuthUser>('/api/auth/me')
export const logout = () => apiRequest<void>('/api/auth/logout', { method: 'POST' })
