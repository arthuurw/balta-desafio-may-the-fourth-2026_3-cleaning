import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { ChatAgent } from '../components/ChatAgent'

vi.mock('../services/api', () => ({
  api: {
    chat: {
      send: vi.fn(),
    },
  },
}))

import { api } from '../services/api'

describe('ChatAgent', () => {
  beforeEach(() => {
    vi.mocked(api.chat.send).mockReset()
  })

  it('renders initial agent message', () => {
    render(<ChatAgent />)
    expect(screen.getByText(/assistente de manutenção/i)).toBeDefined()
  })

  it('send button disabled when input empty', () => {
    render(<ChatAgent />)
    const btn = screen.getByRole('button', { name: /enviar/i }) as HTMLButtonElement
    expect(btn.disabled).toBe(true)
  })

  it('send button enabled when input has text', () => {
    render(<ChatAgent />)
    fireEvent.change(screen.getByPlaceholderText(/pergunte/i), { target: { value: 'Olá' } })
    const btn = screen.getByRole('button', { name: /enviar/i }) as HTMLButtonElement
    expect(btn.disabled).toBe(false)
  })

  it('shows agent reply after send', async () => {
    vi.mocked(api.chat.send).mockResolvedValue({ action: 'answer_maintenance', reply: 'Troque o filtro a cada 6 meses.', tips: [] })
    render(<ChatAgent />)
    fireEvent.change(screen.getByPlaceholderText(/pergunte/i), { target: { value: 'Filtro de água?' } })
    fireEvent.click(screen.getByRole('button', { name: /enviar/i }))
    await waitFor(() => expect(screen.getByText('Troque o filtro a cada 6 meses.')).toBeDefined())
  })

  it('shows error message on API failure', async () => {
    vi.mocked(api.chat.send).mockRejectedValue(new Error('network'))
    render(<ChatAgent />)
    fireEvent.change(screen.getByPlaceholderText(/pergunte/i), { target: { value: 'teste' } })
    fireEvent.click(screen.getByRole('button', { name: /enviar/i }))
    await waitFor(() => expect(screen.getByText(/não consegui processar/i)).toBeDefined())
  })
})
