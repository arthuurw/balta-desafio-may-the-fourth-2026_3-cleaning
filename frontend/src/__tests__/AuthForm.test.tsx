import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { AuthForm } from '../components/AuthForm'
import { AuthContext } from '../contexts/AuthContext'

const mockLogin = vi.fn()
const mockRegister = vi.fn()

function renderWithAuth() {
  return render(
    <AuthContext.Provider value={{ user: null, login: mockLogin, register: mockRegister, logout: vi.fn() }}>
      <AuthForm />
    </AuthContext.Provider>
  )
}

describe('AuthForm', () => {
  beforeEach(() => {
    mockLogin.mockReset()
    mockRegister.mockReset()
  })

  it('renders login mode by default', () => {
    renderWithAuth()
    expect(screen.getByPlaceholderText('seu@email.com')).toBeDefined()
    expect(screen.queryByPlaceholderText('Seu nome')).toBeNull()
  })

  it('toggles to register mode', () => {
    renderWithAuth()
    fireEvent.click(screen.getByText('Cadastrar'))
    expect(screen.getByPlaceholderText('Seu nome')).toBeDefined()
  })

  it('calls login with email and password', async () => {
    mockLogin.mockResolvedValue(undefined)
    renderWithAuth()
    fireEvent.change(screen.getByPlaceholderText('seu@email.com'), { target: { value: 'a@b.com' } })
    fireEvent.change(screen.getByPlaceholderText('••••••'), { target: { value: '123456' } })
    fireEvent.submit(screen.getByPlaceholderText('seu@email.com').closest('form')!)
    await waitFor(() => expect(mockLogin).toHaveBeenCalledWith('a@b.com', '123456'))
  })

  it('shows error on login failure', async () => {
    mockLogin.mockRejectedValue(new Error('Credenciais inválidas'))
    renderWithAuth()
    fireEvent.change(screen.getByPlaceholderText('seu@email.com'), { target: { value: 'a@b.com' } })
    fireEvent.change(screen.getByPlaceholderText('••••••'), { target: { value: 'errado' } })
    fireEvent.submit(screen.getByPlaceholderText('seu@email.com').closest('form')!)
    await waitFor(() => expect(screen.getByText('Credenciais inválidas')).toBeDefined())
  })

  it('calls register in cadastrar mode', async () => {
    mockRegister.mockResolvedValue(undefined)
    renderWithAuth()
    fireEvent.click(screen.getByText('Cadastrar'))
    fireEvent.change(screen.getByPlaceholderText('Seu nome'), { target: { value: 'João' } })
    fireEvent.change(screen.getByPlaceholderText('seu@email.com'), { target: { value: 'j@t.com' } })
    fireEvent.change(screen.getByPlaceholderText('••••••'), { target: { value: 'senha123' } })
    fireEvent.submit(screen.getByPlaceholderText('Seu nome').closest('form')!)
    await waitFor(() => expect(mockRegister).toHaveBeenCalledWith('João', 'j@t.com', 'senha123'))
  })
})
