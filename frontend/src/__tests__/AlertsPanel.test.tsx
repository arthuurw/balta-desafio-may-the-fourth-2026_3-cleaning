import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { AlertsPanel } from '../components/AlertsPanel'
import type { Alert } from '../types'

vi.mock('../services/api', () => ({
  api: {
    alerts: {
      get: vi.fn(),
      markRead: vi.fn(),
    },
  },
}))

import { api } from '../services/api'

const mockAlerts: Alert[] = [
  {
    id: 'a1',
    type: 'overdue',
    message: 'Filtro de água vencido há 30 dias',
    task: { id: 't1', type: 'water_filter_change', scheduledDate: '2026-03-01' },
    createdAt: '2026-04-01',
  },
]

describe('AlertsPanel', () => {
  beforeEach(() => {
    vi.mocked(api.alerts.get).mockReset()
    vi.mocked(api.alerts.markRead).mockReset()
  })

  it('shows empty state when no alerts', async () => {
    vi.mocked(api.alerts.get).mockResolvedValue([])
    render(<AlertsPanel />)
    await waitFor(() => expect(screen.getByText(/nenhum alerta/i)).toBeDefined())
  })

  it('shows alert message', async () => {
    vi.mocked(api.alerts.get).mockResolvedValue(mockAlerts)
    render(<AlertsPanel />)
    await waitFor(() => expect(screen.getByText('Filtro de água vencido há 30 dias')).toBeDefined())
  })

  it('shows alert count', async () => {
    vi.mocked(api.alerts.get).mockResolvedValue(mockAlerts)
    render(<AlertsPanel />)
    await waitFor(() => expect(screen.getByText(/1 alerta/i)).toBeDefined())
  })

  it('removes alert after mark read', async () => {
    vi.mocked(api.alerts.get).mockResolvedValue(mockAlerts)
    vi.mocked(api.alerts.markRead).mockResolvedValue(undefined)
    render(<AlertsPanel />)
    await waitFor(() => screen.getByText('Filtro de água vencido há 30 dias'))
    fireEvent.click(screen.getByRole('button', { name: '✕' }))
    await waitFor(() => expect(screen.queryByText('Filtro de água vencido há 30 dias')).toBeNull())
  })

  it('shows loading state initially', () => {
    vi.mocked(api.alerts.get).mockReturnValue(new Promise(() => {}))
    render(<AlertsPanel />)
    expect(screen.getByText(/verificando alertas/i)).toBeDefined()
  })
})
