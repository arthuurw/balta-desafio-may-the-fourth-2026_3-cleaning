import type { Task } from '../types'
import { PRIORITY_COLORS, TASK_LABELS } from '../types'

interface Props {
  tasks: Task[]
}

const MONTHS = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez']

export function MaintenanceCalendar({ tasks }: Props) {
  const pending = tasks.filter((t) => t.status === 'pending')
  const now = new Date()

  const slots = Array.from({ length: 12 }, (_, i) => {
    const d = new Date(now.getFullYear(), now.getMonth() + i, 1)
    return { year: d.getFullYear(), month: d.getMonth(), isFirst: i === 0 }
  })

  const bySlot = slots.map((slot) => ({
    ...slot,
    tasks: pending.filter((t) => {
      const d = new Date(t.scheduledDate)
      return d.getMonth() === slot.month && d.getFullYear() === slot.year
    }),
  }))

  return (
    <div>
      <p className="text-sm text-gray-500 mb-4">Próximos 12 meses — tarefas pendentes</p>
      <div className="grid grid-cols-3 sm:grid-cols-4 gap-3">
        {bySlot.map(({ year, month, isFirst, tasks: mTasks }) => (
          <div
            key={`${year}-${month}`}
            className={`rounded-xl p-3 border ${
              isFirst ? 'border-blue-300 bg-blue-50' : 'border-gray-200 bg-white'
            }`}
          >
            <p className={`text-xs font-semibold mb-2 ${isFirst ? 'text-blue-600' : 'text-gray-500'}`}>
              {MONTHS[month]}{year !== now.getFullYear() ? ` ${year}` : ''}
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
        ))}
      </div>
    </div>
  )
}
