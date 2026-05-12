import { useEffect, useState } from 'react'
import { api } from '../services/api'
import type { Alert } from '../types'
import { TASK_LABELS } from '../types'

const ALERT_ICONS: Record<string, string> = {
  approaching: '⏰',
  overdue: '🚨',
  seasonal: '🌱',
}

const ALERT_COLORS: Record<string, string> = {
  approaching: 'border-orange-200 bg-orange-50',
  overdue: 'border-red-200 bg-red-50',
  seasonal: 'border-purple-200 bg-purple-50',
}

export function AlertsPanel() {
  const [alerts, setAlerts] = useState<Alert[]>([])
  const [loading, setLoading] = useState(true)

  const load = async () => {
    setLoading(true)
    try {
      const data = await api.alerts.get()
      setAlerts(data)
    } catch {
      setAlerts([])
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  const markRead = async (alertId: string) => {
    await api.alerts.markRead(alertId)
    setAlerts(alerts.filter((a) => a.id !== alertId))
  }

  if (loading) return <div className="text-center text-gray-400 py-8">Verificando alertas...</div>

  if (alerts.length === 0) {
    return (
      <div className="text-center py-12">
        <div className="text-4xl mb-3">✅</div>
        <p className="text-gray-600 font-medium">Nenhum alerta pendente</p>
        <p className="text-gray-400 text-sm mt-1">Sua casa está em dia!</p>
      </div>
    )
  }

  return (
    <div className="space-y-3">
      <p className="text-sm text-gray-500">{alerts.length} alerta{alerts.length !== 1 ? 's' : ''} não lido{alerts.length !== 1 ? 's' : ''}</p>
      {alerts.map((alert) => (
        <div key={alert.id} className={`rounded-xl p-4 border ${ALERT_COLORS[alert.type] ?? 'border-gray-200 bg-gray-50'}`}>
          <div className="flex justify-between items-start">
            <div className="flex gap-3">
              <span className="text-2xl">{ALERT_ICONS[alert.type] ?? '📌'}</span>
              <div>
                <p className="font-medium text-sm">{TASK_LABELS[alert.task.type] ?? alert.task.type}</p>
                <p className="text-sm text-gray-600 mt-1">{alert.message}</p>
                <p className="text-xs text-gray-400 mt-1">
                  {new Date(alert.task.scheduledDate).toLocaleDateString('pt-BR')}
                </p>
              </div>
            </div>
            <button
              onClick={() => markRead(alert.id)}
              className="text-gray-400 hover:text-gray-600 text-sm ml-2 shrink-0"
            >
              ✕
            </button>
          </div>
        </div>
      ))}
    </div>
  )
}
