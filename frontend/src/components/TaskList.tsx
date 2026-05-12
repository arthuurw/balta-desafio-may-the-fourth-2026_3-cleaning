import { useEffect, useState } from 'react'
import { api } from '../services/api'
import type { Task } from '../types'
import { PRIORITY_COLORS, PRIORITY_LABELS, TASK_LABELS } from '../types'

interface Props {
  homeId: string
  onUpdate: () => void
}

export function TaskList({ homeId, onUpdate }: Props) {
  const [tasks, setTasks] = useState<Task[]>([])
  const [filter, setFilter] = useState('pending')
  const [loading, setLoading] = useState(true)
  const [actionId, setActionId] = useState<string | null>(null)

  const load = async () => {
    setLoading(true)
    try {
      const data = await api.tasks.list(homeId, filter)
      setTasks(data)
    } catch {
      setTasks([])
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [homeId, filter])

  const handleComplete = async (taskId: string) => {
    setActionId(taskId)
    try {
      await api.tasks.complete(homeId, taskId)
      await load()
      onUpdate()
    } finally {
      setActionId(null)
    }
  }

  const handleSkip = async (taskId: string) => {
    setActionId(taskId)
    try {
      await api.tasks.skip(homeId, taskId)
      await load()
      onUpdate()
    } finally {
      setActionId(null)
    }
  }

  const today = new Date()
  const getDaysUntil = (date: string) => {
    const d = new Date(date)
    return Math.round((d.getTime() - today.getTime()) / 86400000)
  }

  return (
    <div>
      <div className="flex gap-2 mb-4">
        {['pending', 'completed', 'skipped'].map((s) => (
          <button
            key={s}
            onClick={() => setFilter(s)}
            className={`px-3 py-1 rounded-full text-sm font-medium transition-all ${
              filter === s ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
            }`}
          >
            {s === 'pending' ? 'Pendentes' : s === 'completed' ? 'Concluídas' : 'Puladas'}
          </button>
        ))}
      </div>

      {loading ? (
        <div className="text-center text-gray-400 py-8">Carregando...</div>
      ) : tasks.length === 0 ? (
        <div className="text-center text-gray-400 py-8">Nenhuma tarefa encontrada</div>
      ) : (
        <div className="space-y-3">
          {tasks.map((task) => {
            const days = getDaysUntil(task.scheduledDate)
            const isOverdue = days < 0
            return (
              <div
                key={task.id}
                className={`bg-white rounded-xl p-4 border ${
                  isOverdue && filter === 'pending' ? 'border-red-200' : 'border-gray-100'
                } shadow-sm`}
              >
                <div className="flex justify-between items-start">
                  <div className="flex-1">
                    <div className="flex items-center gap-2 mb-1">
                      <span className="font-medium text-sm">
                        {TASK_LABELS[task.type] ?? task.type}
                      </span>
                      <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${PRIORITY_COLORS[task.priority]}`}>
                        {PRIORITY_LABELS[task.priority]}
                      </span>
                    </div>
                    {task.equipment && (
                      <p className="text-xs text-gray-500 mb-1">{task.equipment.name}</p>
                    )}
                    <p className="text-xs text-gray-500">{task.reason}</p>
                    <p className={`text-xs mt-1 font-medium ${isOverdue ? 'text-red-500' : days <= 7 ? 'text-orange-500' : 'text-gray-400'}`}>
                      {isOverdue
                        ? `Vencida há ${Math.abs(days)} dias`
                        : `${new Date(task.scheduledDate).toLocaleDateString('pt-BR')} · em ${days} dias`}
                    </p>
                  </div>
                  {filter === 'pending' && (
                    <div className="flex gap-2 ml-3 shrink-0">
                      <button
                        onClick={() => handleSkip(task.id)}
                        disabled={actionId === task.id}
                        className="px-2 py-1 text-xs text-gray-500 border border-gray-200 rounded-lg hover:bg-gray-50"
                      >
                        Pular
                      </button>
                      <button
                        onClick={() => handleComplete(task.id)}
                        disabled={actionId === task.id}
                        className="px-2 py-1 text-xs text-white bg-green-500 rounded-lg hover:bg-green-600"
                      >
                        ✓ Feito
                      </button>
                    </div>
                  )}
                </div>
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}
