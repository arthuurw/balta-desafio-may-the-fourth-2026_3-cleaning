import { useState } from 'react'
import { api } from '../services/api'
import type { Home } from '../types'

interface Props {
  onCreated: (home: Home) => void
}

const EQUIPMENT_TYPES = [
  { value: 'ac_cleaning', label: 'Ar-Condicionado' },
  { value: 'water_filter_change', label: 'Filtro de Água' },
  { value: 'water_tank_cleaning', label: 'Caixa d\'Água' },
  { value: 'gas_check', label: 'Instalação de Gás' },
  { value: 'fire_extinguisher_check', label: 'Extintor' },
]

export function HomeSetup({ onCreated }: Props) {
  const [type, setType] = useState<'apartment' | 'house'>('apartment')
  const [hasGarden, setHasGarden] = useState(false)
  const [eqType, setEqType] = useState('ac_cleaning')
  const [eqName, setEqName] = useState('')
  const [eqList, setEqList] = useState<{ type: string; name: string }[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [step, setStep] = useState<'home' | 'equipment'>('home')

  const addEquipment = () => {
    if (!eqName.trim()) return
    setEqList([...eqList, { type: eqType, name: eqName.trim() }])
    setEqName('')
  }

  const submit = async () => {
    setLoading(true)
    setError('')
    try {
      const home = await api.homes.create(type, hasGarden)
      for (const eq of eqList) {
        await api.equipment.add(home.id, eq.type, eq.name)
      }
      const updated = await api.homes.getMine()
      onCreated(updated)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao criar residência')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 p-4">
      <div className="bg-white p-8 rounded-2xl shadow-md w-full max-w-md">
        <h2 className="text-xl font-bold mb-1">Configure sua residência</h2>
        <p className="text-gray-500 text-sm mb-6">A IA irá gerar um cronograma personalizado</p>

        {step === 'home' ? (
          <div className="space-y-5">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Tipo de residência</label>
              <div className="flex gap-3">
                {(['apartment', 'house'] as const).map((t) => (
                  <button
                    key={t}
                    onClick={() => setType(t)}
                    className={`flex-1 py-3 rounded-xl text-sm font-medium border-2 transition-all ${
                      type === t ? 'border-blue-500 bg-blue-50 text-blue-700' : 'border-gray-200 text-gray-600'
                    }`}
                  >
                    {t === 'apartment' ? '🏢 Apartamento' : '🏡 Casa'}
                  </button>
                ))}
              </div>
            </div>

            <label className="flex items-center gap-3 cursor-pointer">
              <input
                type="checkbox"
                checked={hasGarden}
                onChange={(e) => setHasGarden(e.target.checked)}
                className="w-4 h-4 text-blue-600"
              />
              <span className="text-sm text-gray-700">Tem jardim</span>
            </label>

            <button
              onClick={() => setStep('equipment')}
              className="w-full bg-blue-600 text-white py-2.5 rounded-xl text-sm font-medium hover:bg-blue-700"
            >
              Continuar →
            </button>
          </div>
        ) : (
          <div className="space-y-4">
            <div className="flex gap-2">
              <select
                value={eqType}
                onChange={(e) => setEqType(e.target.value)}
                className="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm"
              >
                {EQUIPMENT_TYPES.map((e) => (
                  <option key={e.value} value={e.value}>{e.label}</option>
                ))}
              </select>
              <input
                type="text"
                value={eqName}
                onChange={(e) => setEqName(e.target.value)}
                placeholder="Ex: AC da Sala"
                className="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm"
                onKeyDown={(e) => e.key === 'Enter' && addEquipment()}
              />
              <button
                onClick={addEquipment}
                className="px-3 py-2 bg-gray-100 rounded-lg text-sm hover:bg-gray-200"
              >
                +
              </button>
            </div>

            {eqList.length > 0 && (
              <ul className="space-y-1">
                {eqList.map((eq, i) => (
                  <li key={i} className="flex justify-between items-center text-sm bg-gray-50 px-3 py-2 rounded-lg">
                    <span>{eq.name}</span>
                    <button onClick={() => setEqList(eqList.filter((_, j) => j !== i))} className="text-red-400 hover:text-red-600">✕</button>
                  </li>
                ))}
              </ul>
            )}

            {error && <p className="text-red-500 text-sm">{error}</p>}

            <div className="flex gap-2">
              <button onClick={() => setStep('home')} className="flex-1 border border-gray-300 py-2.5 rounded-xl text-sm">← Voltar</button>
              <button
                onClick={submit}
                disabled={loading}
                className="flex-1 bg-blue-600 text-white py-2.5 rounded-xl text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
              >
                {loading ? 'Criando...' : 'Criar residência'}
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
