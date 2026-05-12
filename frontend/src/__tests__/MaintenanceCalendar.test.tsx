import { render, screen } from '@testing-library/react'
import { describe, it, expect } from 'vitest'
import { MaintenanceCalendar } from '../components/MaintenanceCalendar'
import type { Task } from '../types'

const currentYear = new Date().getFullYear()
const currentMonth = new Date().getMonth() + 1
const pad = (n: number) => String(n).padStart(2, '0')

const makeTask = (month: number, overrides?: Partial<Task>): Task => ({
  id: `t-${month}`,
  type: 'ac_cleaning',
  scheduledDate: `${currentYear}-${pad(month)}-15`,
  priority: 'medium',
  reason: 'Limpeza periódica',
  status: 'pending',
  ...overrides,
})

describe('MaintenanceCalendar', () => {
  it('renders 12 month cells', () => {
    render(<MaintenanceCalendar tasks={[]} />)
    const months = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez']
    months.forEach((m) => expect(screen.getByText(m)).toBeDefined())
  })

  it('shows task label in correct month', () => {
    const task = makeTask(currentMonth)
    render(<MaintenanceCalendar tasks={[task]} />)
    expect(screen.getByText('Ar-Condicionado')).toBeDefined()
  })

  it('does not show completed tasks', () => {
    const task = makeTask(currentMonth, { status: 'completed' })
    render(<MaintenanceCalendar tasks={[task]} />)
    expect(screen.queryByText('Ar-Condicionado')).toBeNull()
  })

  it('shows current year in header', () => {
    render(<MaintenanceCalendar tasks={[]} />)
    expect(screen.getByText(new RegExp(String(currentYear)))).toBeDefined()
  })

  it('shows em dash for empty months', () => {
    render(<MaintenanceCalendar tasks={[]} />)
    const dashes = screen.getAllByText('—')
    expect(dashes.length).toBe(12)
  })
})
