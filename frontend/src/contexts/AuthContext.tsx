import { createContext, useCallback, useContext, useState, type ReactNode } from 'react'
import type { User } from '../types'
import { api } from '../services/api'

interface AuthCtx {
  user: User | null
  login: (email: string, password: string) => Promise<void>
  register: (name: string, email: string, password: string) => Promise<void>
  logout: () => void
}

export const AuthContext = createContext<AuthCtx | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(() => {
    try {
      const stored = localStorage.getItem('user')
      return stored ? JSON.parse(stored) : null
    } catch {
      return null
    }
  })

  const save = (u: User) => {
    localStorage.setItem('token', u.token)
    localStorage.setItem('user', JSON.stringify(u))
    setUser(u)
  }

  const login = useCallback(async (email: string, password: string) => {
    const u = await api.auth.login(email, password)
    save(u)
  }, [])

  const register = useCallback(async (name: string, email: string, password: string) => {
    const u = await api.auth.register(name, email, password)
    save(u)
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    setUser(null)
  }, [])

  return <AuthContext.Provider value={{ user, login, register, logout }}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
