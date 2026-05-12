export interface User {
  id: string
  name: string
  email: string
  token: string
}

export interface Equipment {
  id: string
  type: string
  name: string
  installedAt?: string
}

export interface Task {
  id: string
  type: string
  scheduledDate: string
  priority: 'low' | 'medium' | 'high' | 'critical'
  reason: string
  status: 'pending' | 'completed' | 'skipped'
  equipment?: Equipment
}

export interface Home {
  id: string
  type: 'house' | 'apartment'
  hasGarden: boolean
  equipment: Equipment[]
  upcomingTasks: Task[]
}

export interface Alert {
  id: string
  type: 'approaching' | 'overdue' | 'seasonal'
  message: string
  task: { id: string; type: string; scheduledDate: string }
  createdAt: string
}

export interface ChatResponse {
  action: string
  reply: string
  tips?: { type: string; tip: string }[]
}

export const TASK_LABELS: Record<string, string> = {
  water_filter_change: 'Filtro de Água',
  ac_cleaning: 'Ar-Condicionado',
  drain_cleaning: 'Ralos',
  water_tank_cleaning: 'Caixa d\'Água',
  electrical_check: 'Elétrica',
  gas_check: 'Gás',
  pest_control: 'Dedetização',
  fire_extinguisher_check: 'Extintor',
  roof_check: 'Telhado',
  gutter_cleaning: 'Calhas',
  garden_maintenance: 'Jardim',
  external_painting: 'Pintura Externa',
}

export const PRIORITY_COLORS: Record<string, string> = {
  low: 'bg-gray-100 text-gray-700',
  medium: 'bg-amber-100 text-amber-700',
  high: 'bg-orange-100 text-orange-700',
  critical: 'bg-red-100 text-red-700',
}

export const PRIORITY_LABELS: Record<string, string> = {
  low: 'Baixa',
  medium: 'Média',
  high: 'Alta',
  critical: 'Crítica',
}
