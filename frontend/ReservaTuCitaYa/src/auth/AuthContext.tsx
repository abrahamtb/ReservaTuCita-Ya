import { createContext, useCallback, useEffect, useMemo, useState, type PropsWithChildren } from 'react'
import * as authApi from '../api/authApi'
import { ApiError } from '../api/apiClient'
import type { AuthUser } from '../types'

interface AuthContextValue {
  user: AuthUser | null; loading: boolean
  login: (email: string, password: string, remember: boolean) => Promise<void>
  logout: () => Promise<void>
}
export const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export function AuthProvider({ children }: PropsWithChildren) {
  const [user, setUser] = useState<AuthUser | null>(null)
  const [loading, setLoading] = useState(true)
  useEffect(() => {
    const clearSession = () => setUser(null)
    window.addEventListener('auth:unauthorized', clearSession)
    return () => window.removeEventListener('auth:unauthorized', clearSession)
  }, [])
  useEffect(() => {
    authApi.me().then(setUser).catch((error: unknown) => {
      if (!(error instanceof ApiError) || error.status !== 401) console.error(error)
      setUser(null)
    }).finally(() => setLoading(false))
  }, [])
  const signIn = useCallback(async (email: string, password: string, remember: boolean) => {
    setUser(await authApi.login(email, password, remember))
  }, [])
  const signOut = useCallback(async () => { await authApi.logout(); setUser(null) }, [])
  const value = useMemo(() => ({ user, loading, login: signIn, logout: signOut }), [user, loading, signIn, signOut])
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
