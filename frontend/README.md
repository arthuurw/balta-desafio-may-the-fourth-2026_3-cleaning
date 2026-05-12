# CasaLog — Frontend

React 19 + TypeScript + Vite + Tailwind CSS v4. Interface do app de manutenção residencial CasaLog.

## Stack

- React 19
- TypeScript
- Vite
- Tailwind CSS v4
- Vitest + Testing Library

## Estrutura

```
src/
├── components/
│   ├── AuthForm.tsx             # Login e cadastro
│   ├── HomeSetup.tsx            # Cadastro de residência + equipamentos
│   ├── MaintenanceCalendar.tsx  # Calendário mensal de tarefas
│   ├── TaskList.tsx             # Lista de tarefas com ações concluir/pular
│   ├── AlertsPanel.tsx          # Painel de alertas da IA
│   ├── ChatAgent.tsx            # Chat com o agente de manutenção
│   └── HelpPage.tsx             # Guia de funcionalidades do app
├── contexts/
│   └── AuthContext.tsx          # JWT auth context
├── services/
│   └── api.ts                   # Cliente HTTP tipado
├── types/                       # Tipos compartilhados
└── App.tsx                      # Roteamento condicional (sem React Router)
```

## Configuração

Criar `frontend/.env.local`:

```
VITE_API_URL=http://localhost:5059
```

## Comandos

```bash
npm install
npm run dev      # http://localhost:5173
npm test         # Vitest
npm run build
npm run lint
```

## Fluxo de navegação

O roteamento é feito por conditional rendering em `App.tsx` — sem React Router:

1. Não autenticado → `<AuthForm />`
2. Autenticado, sem residência → `<HomeSetup />`
3. Autenticado, com residência → layout com 5 abas: **Calendário | Tarefas | Alertas | Chat IA | ?**

## Abas

| Aba | Componente | Descrição |
|-----|-----------|-----------|
| Calendário | `MaintenanceCalendar` | Tarefas agrupadas por mês, cores por prioridade |
| Tarefas | `TaskList` | Lista com filtro por status, ações concluir/pular, notas |
| Alertas | `AlertsPanel` | Alertas gerados pela IA, marcar como lido |
| Chat IA | `ChatAgent` | Chat com o agente de manutenção residencial |
| ? | `HelpPage` | Guia completo: funcionalidades, tipos de manutenção, sazonalidade |

## Testes

```bash
npm test    # 25 testes (Vitest + Testing Library)
```

Cobertura: `api.ts`, `MaintenanceCalendar`, `AuthForm`, `ChatAgent`, `AlertsPanel`.
