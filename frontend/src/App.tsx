import { useCallback, useEffect, useState } from 'react'
import { AuthForm } from './components/AuthForm'
import { HomeSetup } from './components/HomeSetup'
import { TaskList } from './components/TaskList'
import { AlertsPanel } from './components/AlertsPanel'
import { ChatAgent } from './components/ChatAgent'
import { MaintenanceCalendar } from './components/MaintenanceCalendar'
import { useAuth } from './contexts/AuthContext'
import { api } from './services/api'
import type { Home, Task } from './types'

type Tab = 'calendar' | 'tasks' | 'alerts' | 'chat'

const TABS: { id: Tab; label: string }[] = [
  { id: 'calendar', label: 'Calendário' },
  { id: 'tasks', label: 'Tarefas' },
  { id: 'alerts', label: 'Alertas' },
  { id: 'chat', label: 'Chat IA' },
]

export default function App() {
  const { user, logout } = useAuth()
  const [home, setHome] = useState<Home | null | undefined>(undefined)
  const [allTasks, setAllTasks] = useState<Task[]>([])
  const [tab, setTab] = useState<Tab>('calendar')

  const loadHome = useCallback(async () => {
    if (!user) return
    try {
      const h = await api.homes.getMine()
      setHome(h)
    } catch {
      setHome(null)
    }
  }, [user])

  const loadAllTasks = useCallback(async (homeId: string) => {
    try {
      const tasks = await api.tasks.list(homeId, 'pending')
      setAllTasks(tasks)
    } catch {
      setAllTasks([])
    }
  }, [])

  useEffect(() => {
    loadHome()
  }, [loadHome])

  useEffect(() => {
    if (home) loadAllTasks(home.id)
  }, [home, loadAllTasks])

  if (!user) return <AuthForm />

  if (home === undefined) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <p className="text-gray-400">Carregando...</p>
      </div>
    )
  }

  if (home === null) {
    return <HomeSetup onCreated={(h) => setHome(h)} />
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white border-b border-gray-200 sticky top-0 z-10">
        <div className="max-w-2xl mx-auto px-4 py-3 flex justify-between items-center">
          <div>
            <h1 className="font-bold text-gray-900">CasaLog</h1>
            <p className="text-xs text-gray-500">
              {home.type === 'apartment' ? 'Apartamento' : 'Casa'}
              {home.hasGarden ? ' · com jardim' : ''}
            </p>
          </div>
          <button
            onClick={logout}
            className="text-sm text-gray-500 hover:text-gray-700"
          >
            Sair
          </button>
        </div>

        <div className="max-w-2xl mx-auto px-4 flex gap-1 pb-0">
          {TABS.map((t) => (
            <button
              key={t.id}
              onClick={() => setTab(t.id)}
              className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
                tab === t.id
                  ? 'border-blue-600 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700'
              }`}
            >
              {t.label}
            </button>
          ))}
        </div>
      </header>

      <main className="max-w-2xl mx-auto px-4 py-6">
        {tab === 'calendar' && <MaintenanceCalendar tasks={allTasks} />}
        {tab === 'tasks' && (
          <TaskList
            homeId={home.id}
            onUpdate={() => loadAllTasks(home.id)}
          />
        )}
        {tab === 'alerts' && <AlertsPanel />}
        {tab === 'chat' && <ChatAgent />}
      </main>
    </div>
  )
}
