import { useState } from 'react'
import { api } from '../services/api'

interface Message {
  id: number
  role: 'user' | 'agent'
  text: string
  tips?: { type: string; tip: string }[]
}

let nextId = 0

export function ChatAgent() {
  const [messages, setMessages] = useState<Message[]>([
    { id: nextId++, role: 'agent', text: 'Olá! Sou seu assistente de manutenção residencial. Como posso ajudar?' }
  ])
  const [input, setInput] = useState('')
  const [loading, setLoading] = useState(false)

  const send = async () => {
    const text = input.trim()
    if (!text || loading) return
    setInput('')
    setMessages((prev) => [...prev, { id: nextId++, role: 'user', text }])
    setLoading(true)
    try {
      const res = await api.chat.send(text)
      setMessages((prev) => [
        ...prev,
        { id: nextId++, role: 'agent', text: res.reply, tips: res.tips }
      ])
    } catch {
      setMessages((prev) => [
        ...prev,
        { id: nextId++, role: 'agent', text: 'Desculpe, não consegui processar sua mensagem. Tente novamente.' }
      ])
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="flex flex-col h-[500px]">
      <div className="flex-1 overflow-y-auto space-y-3 pb-3">
        {messages.map((msg) => (
          <div key={msg.id} className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}>
            <div
              className={`max-w-[80%] px-4 py-2.5 rounded-2xl text-sm ${
                msg.role === 'user'
                  ? 'bg-blue-600 text-white rounded-br-sm'
                  : 'bg-white border border-gray-200 text-gray-800 rounded-bl-sm'
              }`}
            >
              <p>{msg.text}</p>
              {msg.tips && msg.tips.length > 0 && (
                <div className="mt-2 space-y-1">
                  {msg.tips.map((tip, j) => (
                    <p key={j} className="text-xs bg-green-50 text-green-800 px-2 py-1 rounded-lg">
                      💡 {tip.tip}
                    </p>
                  ))}
                </div>
              )}
            </div>
          </div>
        ))}
        {loading && (
          <div className="flex justify-start">
            <div className="bg-white border border-gray-200 px-4 py-2.5 rounded-2xl rounded-bl-sm">
              <span className="text-gray-400 text-sm">Pensando...</span>
            </div>
          </div>
        )}
      </div>

      <div className="flex gap-2 pt-3 border-t border-gray-100">
        <input
          type="text"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && send()}
          placeholder="Pergunte sobre manutenção..."
          maxLength={2000}
          className="flex-1 border border-gray-300 rounded-xl px-4 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          disabled={loading}
        />
        <button
          onClick={send}
          disabled={loading || !input.trim()}
          className="px-4 py-2 bg-blue-600 text-white rounded-xl text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
        >
          Enviar
        </button>
      </div>
    </div>
  )
}
