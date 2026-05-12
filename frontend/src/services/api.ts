import type { Alert, ChatResponse, Equipment, Home, Task, User } from '../types'

const BASE = (import.meta.env.VITE_API_URL ?? '') + '/api'

function getToken(): string {
  return localStorage.getItem('token') ?? ''
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${getToken()}`,
      ...options.headers,
    },
  })
  if (!res.ok) {
    const body = await res.json().catch(() => ({}))
    throw new Error(body.error ?? `HTTP ${res.status}`)
  }
  return res.json() as Promise<T>
}

export const api = {
  auth: {
    register: (name: string, email: string, password: string) =>
      request<User>('/auth/register', {
        method: 'POST',
        body: JSON.stringify({ name, email, password }),
      }),
    login: (email: string, password: string) =>
      request<User>('/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email, password }),
      }),
  },

  homes: {
    create: (type: string, hasGarden: boolean) =>
      request<Home & { tasks: Task[] }>('/homes', {
        method: 'POST',
        body: JSON.stringify({ type, hasGarden }),
      }),
    getMine: () => request<Home>('/homes/mine'),
  },

  equipment: {
    add: (homeId: string, type: string, name: string, installedAt?: string) =>
      request<Equipment & { tasks: Task[] }>(`/homes/${homeId}/equipment`, {
        method: 'POST',
        body: JSON.stringify({ type, name, installedAt }),
      }),
    remove: (homeId: string, equipmentId: string) =>
      fetch(`${BASE}/homes/${homeId}/equipment/${equipmentId}`, {
        method: 'DELETE',
        headers: { Authorization: `Bearer ${getToken()}` },
      }),
  },

  tasks: {
    list: (homeId: string, status?: string, month?: number) => {
      const params = new URLSearchParams()
      if (status) params.set('status', status)
      if (month) params.set('month', String(month))
      return request<Task[]>(`/homes/${homeId}/tasks?${params}`)
    },
    complete: (homeId: string, taskId: string, notes?: string) =>
      request(`/homes/${homeId}/tasks/${taskId}/complete`, {
        method: 'POST',
        body: JSON.stringify({ notes }),
      }),
    skip: (homeId: string, taskId: string) =>
      request(`/homes/${homeId}/tasks/${taskId}/skip`, {
        method: 'POST',
        body: JSON.stringify({}),
      }),
  },

  alerts: {
    get: () => request<Alert[]>('/alerts'),
    markRead: (alertId: string) =>
      fetch(`${BASE}/alerts/${alertId}/read`, {
        method: 'PUT',
        headers: { Authorization: `Bearer ${getToken()}` },
      }),
  },

  chat: {
    send: (message: string) =>
      request<ChatResponse>('/chat', {
        method: 'POST',
        body: JSON.stringify({ message }),
      }),
  },
}
