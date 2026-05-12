interface Feature {
  icon: string
  title: string
  description: string
  details: string[]
  badge?: string
  badgeColor?: string
}

const FEATURES: Feature[] = [
  {
    icon: '🏠',
    title: 'Residência',
    badge: 'Configuração',
    badgeColor: 'bg-gray-100 text-gray-600',
    description: 'Cadastre sua residência para que a IA possa personalizar o cronograma de manutenção.',
    details: [
      'Escolha o tipo: Casa ou Apartamento',
      'Informe se possui jardim — tarefas mensais de jardinagem são incluídas automaticamente',
      'Ao criar a residência, a IA gera um cronograma completo de 12 meses',
      'A IA também sugere quais equipamentos você deve cadastrar com base no perfil da sua casa',
    ],
  },
  {
    icon: '🔧',
    title: 'Equipamentos',
    badge: 'Configuração',
    badgeColor: 'bg-gray-100 text-gray-600',
    description: 'Cadastre seus equipamentos para que tarefas específicas sejam criadas e vinculadas a eles.',
    details: [
      'A IA valida e normaliza o tipo de equipamento automaticamente — escreva "Ar condicionado split" e ela entende',
      'Tipos suportados: ar-condicionado, filtro de água, caixa d\'água, extintor, gás, elétrica, telhado, calhas, jardim, piscina, painel solar, boiler',
      'Ao adicionar um equipamento, novas tarefas são geradas automaticamente para ele',
      'Remover um equipamento apaga todas as tarefas pendentes vinculadas',
    ],
  },
  {
    icon: '📅',
    title: 'Calendário',
    badge: 'Principal',
    badgeColor: 'bg-blue-100 text-blue-700',
    description: 'Visualize todas as tarefas de manutenção organizadas por mês.',
    details: [
      'Tarefas agrupadas por mês, do mais próximo ao mais distante',
      'Cor de prioridade: vermelho (crítico), laranja (alto), amarelo (médio), cinza (baixo)',
      'Tarefas vencidas aparecem destacadas em vermelho',
      'Atualiza automaticamente ao concluir ou pular tarefas',
    ],
  },
  {
    icon: '✅',
    title: 'Tarefas',
    badge: 'Principal',
    badgeColor: 'bg-blue-100 text-blue-700',
    description: 'Gerencie suas tarefas de manutenção pendentes, concluídas e puladas.',
    details: [
      'Filtre por: Pendentes, Concluídas ou Puladas',
      'Concluir uma tarefa: a IA calcula automaticamente a próxima data de manutenção',
      'Adicione notas ao concluir — a IA ajusta o intervalo com base na condição relatada',
      'Pular uma tarefa: a IA reagenda sem ultrapassar o dobro do intervalo normal',
      'Tarefas vencidas aparecem com alerta visual em vermelho',
    ],
  },
  {
    icon: '🔔',
    title: 'Alertas',
    badge: 'IA',
    badgeColor: 'bg-purple-100 text-purple-700',
    description: 'A IA monitora suas tarefas e envia alertas automáticos quando necessário.',
    details: [
      'Verificação automática ao abrir a aba — máximo 1 vez por hora para evitar excesso',
      'Tipos de alerta: Proximidade (⏰), Vencida (🚨), Sazonal (🌱)',
      'Critérios: tarefa vence em ≤ 15 dias, tarefa de segurança em ≤ 30 dias, tarefa vencida',
      'AC antes do verão gera alerta sazonal mesmo com mais de 30 dias',
      'Não gera alerta duplicado se já existe um não lido para a mesma tarefa',
      'Marque como lido para arquivar o alerta',
    ],
  },
  {
    icon: '🤖',
    title: 'Chat IA',
    badge: 'IA',
    badgeColor: 'bg-purple-100 text-purple-700',
    description: 'Converse com o assistente de manutenção residencial sobre dúvidas e dicas.',
    details: [
      'Pergunte sobre prazos, procedimentos e equipamentos específicos da sua casa',
      'Peça dicas sazonais: "o que fazer neste outono?" ou "dicas para o verão"',
      'O agente conhece sua residência, equipamentos e próximas tarefas',
      'Escopo limitado a manutenção residencial — fora disso, o agente informa e não responde',
      'Protegido contra tentativas de manipulação do sistema',
    ],
  },
  {
    icon: '📊',
    title: 'Relatório de Saúde',
    badge: 'IA',
    badgeColor: 'bg-purple-100 text-purple-700',
    description: 'Obtenha uma avaliação completa do estado de manutenção da sua residência.',
    details: [
      'Score de 0 a 100 calculado pela IA com base no histórico de manutenção',
      'Status: Excelente (80–100), Bom (60–79), Atenção (40–59), Crítico (0–39)',
      'Lista de recomendações priorizadas com ações específicas a tomar',
      'Penaliza tarefas vencidas, tarefas puladas e pendências críticas',
      'Recompensa histórico de conclusões regulares',
      'Acesse em: GET /api/homes/{id}/report',
    ],
  },
  {
    icon: '🗓️',
    title: 'Cronograma Inteligente',
    badge: 'IA',
    badgeColor: 'bg-purple-100 text-purple-700',
    description: 'A IA distribui tarefas ao longo de 12 meses respeitando sazonalidade e prioridades.',
    details: [
      'Máximo de 3 tarefas por mês (jardinagem não conta)',
      'AC obrigatório em setembro ou outubro (pré-verão brasileiro)',
      'Dedetização em abril e outubro (antes e depois das chuvas)',
      'Tarefas de segurança (gás, extintor) nunca em dezembro',
      'Tarefas anuais distribuídas em meses diferentes para não sobrecarregar',
      'Reagendamento automático ao concluir ou pular — intervalo ajustado pelas notas',
    ],
  },
]

const TASKS_TABLE = [
  { code: 'water_filter_change', name: 'Troca de filtro de água', interval: '3–6 meses', priority: 'Média' },
  { code: 'ac_cleaning', name: 'Limpeza de ar-condicionado', interval: '6–12 meses', priority: 'Alta' },
  { code: 'drain_cleaning', name: 'Desentupimento de ralos', interval: '3 meses', priority: 'Baixa' },
  { code: 'water_tank_cleaning', name: 'Limpeza de caixa d\'água', interval: '6 meses', priority: 'Média' },
  { code: 'electrical_check', name: 'Verificação elétrica', interval: '12 meses', priority: 'Média' },
  { code: 'gas_check', name: 'Revisão de gás', interval: '12 meses', priority: 'Crítica' },
  { code: 'pest_control', name: 'Dedetização', interval: '6 meses', priority: 'Média' },
  { code: 'fire_extinguisher_check', name: 'Revisão de extintor', interval: '12 meses', priority: 'Crítica' },
  { code: 'roof_check', name: 'Vistoria de telhado', interval: '12 meses', priority: 'Alta' },
  { code: 'gutter_cleaning', name: 'Limpeza de calhas', interval: '6 meses', priority: 'Média' },
  { code: 'garden_maintenance', name: 'Manutenção de jardim', interval: 'Mensal', priority: 'Baixa' },
  { code: 'external_painting', name: 'Pintura externa', interval: '5 anos', priority: 'Baixa' },
]

const PRIORITY_BADGE: Record<string, string> = {
  'Crítica': 'bg-red-100 text-red-700',
  'Alta': 'bg-orange-100 text-orange-700',
  'Média': 'bg-yellow-100 text-yellow-700',
  'Baixa': 'bg-gray-100 text-gray-600',
}

export function HelpPage() {
  return (
    <div className="space-y-8 pb-6">
      <div className="bg-blue-600 rounded-2xl p-6 text-white">
        <h2 className="text-xl font-bold mb-1">CasaLog — Guia de Funcionalidades</h2>
        <p className="text-blue-100 text-sm">
          App de manutenção preventiva residencial com IA. Cadastre sua casa, seus equipamentos e deixe o agente cuidar do planejamento.
        </p>
      </div>

      <section>
        <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wide mb-3">Como começar</h3>
        <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-4 space-y-3">
          {[
            { step: '1', text: 'Crie sua conta e faça login' },
            { step: '2', text: 'Cadastre sua residência — tipo e se tem jardim' },
            { step: '3', text: 'Adicione seus equipamentos (a IA valida o tipo automaticamente)' },
            { step: '4', text: 'A IA gera um cronograma de 12 meses automaticamente' },
            { step: '5', text: 'Acompanhe tarefas, alertas e converse com o assistente' },
          ].map(({ step, text }) => (
            <div key={step} className="flex items-start gap-3">
              <span className="w-6 h-6 rounded-full bg-blue-600 text-white text-xs font-bold flex items-center justify-center shrink-0 mt-0.5">
                {step}
              </span>
              <p className="text-sm text-gray-700">{text}</p>
            </div>
          ))}
        </div>
      </section>

      <section>
        <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wide mb-3">Funcionalidades</h3>
        <div className="space-y-3">
          {FEATURES.map((f) => (
            <div key={f.title} className="bg-white rounded-xl border border-gray-100 shadow-sm p-4">
              <div className="flex items-start gap-3">
                <span className="text-2xl">{f.icon}</span>
                <div className="flex-1">
                  <div className="flex items-center gap-2 mb-1 flex-wrap">
                    <h4 className="font-semibold text-gray-900 text-sm">{f.title}</h4>
                    {f.badge && (
                      <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${f.badgeColor}`}>
                        {f.badge}
                      </span>
                    )}
                  </div>
                  <p className="text-sm text-gray-600 mb-3">{f.description}</p>
                  <ul className="space-y-1.5">
                    {f.details.map((d, i) => (
                      <li key={i} className="flex items-start gap-2 text-xs text-gray-500">
                        <span className="text-blue-400 mt-0.5 shrink-0">›</span>
                        <span>{d}</span>
                      </li>
                    ))}
                  </ul>
                </div>
              </div>
            </div>
          ))}
        </div>
      </section>

      <section>
        <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wide mb-3">Tipos de Manutenção</h3>
        <div className="bg-white rounded-xl border border-gray-100 shadow-sm overflow-hidden">
          <table className="w-full text-xs">
            <thead>
              <tr className="bg-gray-50 border-b border-gray-100">
                <th className="text-left px-4 py-2.5 text-gray-500 font-medium">Tarefa</th>
                <th className="text-left px-4 py-2.5 text-gray-500 font-medium">Intervalo</th>
                <th className="text-left px-4 py-2.5 text-gray-500 font-medium">Prioridade</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50">
              {TASKS_TABLE.map((t) => (
                <tr key={t.code} className="hover:bg-gray-50 transition-colors">
                  <td className="px-4 py-2.5 text-gray-700 font-medium">{t.name}</td>
                  <td className="px-4 py-2.5 text-gray-500">{t.interval}</td>
                  <td className="px-4 py-2.5">
                    <span className={`px-2 py-0.5 rounded-full font-medium ${PRIORITY_BADGE[t.priority]}`}>
                      {t.priority}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>

      <section>
        <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wide mb-3">Sazonalidade Brasileira</h3>
        <div className="grid grid-cols-2 gap-3">
          {[
            { season: 'Verão', months: 'Dez – Mar', icon: '☀️', tip: 'AC em uso máximo. Chuvas intensas.', color: 'bg-orange-50 border-orange-100' },
            { season: 'Outono', months: 'Mar – Mai', icon: '🍂', tip: 'Pós-chuvas: desentupimento e revisão geral.', color: 'bg-amber-50 border-amber-100' },
            { season: 'Inverno', months: 'Jun – Ago', icon: '❄️', tip: 'Clima seco: ideal para pintura externa.', color: 'bg-blue-50 border-blue-100' },
            { season: 'Primavera', months: 'Set – Nov', icon: '🌸', tip: 'AC obrigatório, telhado e dedetização.', color: 'bg-green-50 border-green-100' },
          ].map((s) => (
            <div key={s.season} className={`rounded-xl border p-3 ${s.color}`}>
              <div className="flex items-center gap-2 mb-1">
                <span>{s.icon}</span>
                <span className="font-semibold text-sm text-gray-800">{s.season}</span>
              </div>
              <p className="text-xs text-gray-500 mb-1">{s.months}</p>
              <p className="text-xs text-gray-600">{s.tip}</p>
            </div>
          ))}
        </div>
      </section>
    </div>
  )
}
