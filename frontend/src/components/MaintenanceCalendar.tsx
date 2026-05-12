import type { Task } from '../types'
import { PRIORITY_COLORS, TASK_LABELS } from '../types'

interface Props {
  tasks: Task[]
}

const MONTHS = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez']

export function MaintenanceCalendar({ tasks }: Props) {
  const pending = tasks.filter((t) => t.status === 'pending')
  const now = new Date()
  const currentYear = now.getFullYear()

  const byMonth = Array.from({ length: 12 }, (_, i) => ({
    month: i,
    tasks: pending.filter((t) => {
      const d = new Date(t.scheduledDate)
      return d.getMonth() === i && d.getFullYear() === currentYear
    })
  }))

  return (
    <div>
      <p className="text-sm text-gray-500 mb-4">Cronograma {currentYear} — tarefas pendentes</p>
      <div className="grid grid-cols-3 sm:grid-cols-4 gap-3">
        {byMonth.map(({ month, tasks: mTasks }) => {
          const isPast = month < now.getMonth()
          const isCurrent = month === now.getMonth()
          return (
            <div
              key={month}
              className={`rounded-xl p-3 border ${
                isCurrent ? 'border-blue-300 bg-blue-50' : isPast ? 'border-gray-100 bg-gray-50 opacity-60' : 'border-gray-200 bg-white'
              }`}
            >
              <p className={`text-xs font-semibold mb-2 ${isCurrent ? 'text-blue-600' : 'text-gray-500'}`}>
                {MONTHS[month]}
              </p>
              {mTasks.length === 0 ? (
                <p className="text-xs text-gray-300">—</p>
              ) : (
                <div className="space-y-1">
                  {mTasks.map((task) => (
                    <div key={task.id} className={`text-xs px-1.5 py-0.5 rounded font-medium truncate ${PRIORITY_COLORS[task.priority]}`}>
                      {TASK_LABELS[task.type] ?? task.type}
                    </div>
                  ))}
                </div>
              )}
            </div>
          )
        })}
      </div>
    </div>
  )
}
