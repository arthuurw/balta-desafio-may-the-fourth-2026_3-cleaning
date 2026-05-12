import { describe, it, expect, vi, beforeEach } from 'vitest'
import { api } from '../services/api'

const mockFetch = vi.fn()
global.fetch = mockFetch

function mockResponse(data: unknown, ok = true, status = 200) {
  return Promise.resolve({
    ok,
    status,
    json: () => Promise.resolve(data),
  } as Response)
}

describe('api service', () => {
  beforeEach(() => {
    mockFetch.mockReset()
    localStorage.clear()
  })

  it('auth.login calls POST /api/auth/login', async () => {
    mockFetch.mockReturnValue(mockResponse({ id: '1', name: 'Test', email: 'a@b.com', token: 'tok' }))
    const user = await api.auth.login('a@b.com', 'pass')
    expect(mockFetch).toHaveBeenCalledWith('/api/auth/login', expect.objectContaining({ method: 'POST' }))
    expect(user.token).toBe('tok')
  })

  it('auth.register calls POST /api/auth/register', async () => {
    mockFetch.mockReturnValue(mockResponse({ id: '2', name: 'João', email: 'j@b.com', token: 'tok2' }))
    await api.auth.register('João', 'j@b.com', 'pass')
    expect(mockFetch).toHaveBeenCalledWith('/api/auth/register', expect.objectContaining({ method: 'POST' }))
  })

  it('throws on non-ok response', async () => {
    mockFetch.mockReturnValue(mockResponse({ error: 'Não encontrado' }, false, 404))
    await expect(api.homes.getMine()).rejects.toThrow('Não encontrado')
  })

  it('homes.getMine calls GET /api/homes/mine', async () => {
    mockFetch.mockReturnValue(mockResponse({ id: 'h1', type: 'apartment', hasGarden: false, equipment: [], upcomingTasks: [] }))
    const home = await api.homes.getMine()
    expect(mockFetch).toHaveBeenCalledWith('/api/homes/mine', expect.any(Object))
    expect(home.id).toBe('h1')
  })

  it('tasks.list includes status query param', async () => {
    mockFetch.mockReturnValue(mockResponse([]))
    await api.tasks.list('h1', 'pending')
    const url = mockFetch.mock.calls[0][0] as string
    expect(url).toContain('status=pending')
  })
})
