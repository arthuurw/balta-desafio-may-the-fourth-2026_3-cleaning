# CasaLog — Backend

.NET 10 Minimal API com arquitetura Vertical Slice para o app de manutenção residencial CasaLog.

## Stack

- .NET 10 Minimal API
- EF Core 10 + SQLite (`EnsureCreated`, sem migrations)
- JWT Bearer auth + `PasswordHasher<User>`
- Microsoft Agents Framework (MAF) 1.4.0
- OpenAI-compatible LLM (Ollama dev / Groq prod)
- Scalar/Swagger (dev)

## Estrutura

```
CasaLog.Api/
├── Data/
│   ├── Entities/          # User, Home, Equipment, ScheduledTask, TaskCompletion, Alert
│   └── CasaLogContext.cs
├── Features/              # Vertical Slice
│   ├── Auth/              # Register, Login
│   ├── Homes/             # CreateHome, GetHome
│   ├── Equipment/         # AddEquipment, DeleteEquipment
│   ├── Tasks/             # GetTasks, CompleteTask, SkipTask
│   ├── Alerts/            # GetAlerts, MarkAlertRead
│   └── Chat/              # ChatHandler
├── Agents/
│   ├── HomeAgent.cs       # MAF agent implementation
│   ├── IHomeAgent.cs
│   └── AgentDtos.cs
├── Infrastructure/
│   ├── Security/          # JwtTokenGenerator, PasswordHasher
│   └── Llm/               # LlmClientFactory (Ollama/Groq)
└── Extensions/
    ├── ServiceExtensions.cs
    └── EndpointExtensions.cs

agents/
└── agente-casa.md         # System prompt do agente

CasaLog.Api.Tests/
├── Features/Auth/
├── Features/Homes/
├── Features/Equipment/
├── Features/Tasks/
├── Features/Alerts/
└── Agents/
```

## Endpoints

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| POST | `/api/auth/register` | — | Cadastro de usuário |
| POST | `/api/auth/login` | — | Login, retorna JWT |
| POST | `/api/homes` | ✓ | Cria residência + gera cronograma IA |
| GET | `/api/homes/mine` | ✓ | Retorna residência do usuário |
| GET | `/api/homes/{id}/report` | ✓ | Relatório de saúde com score 0–100 via IA |
| POST | `/api/homes/{id}/equipment` | ✓ | Adiciona equipamento (normaliza tipo via IA) |
| DELETE | `/api/homes/{id}/equipment/{eqId}` | ✓ | Remove equipamento (cascade tasks pending) |
| GET | `/api/homes/{id}/tasks` | ✓ | Lista tarefas (filtro: status, month) |
| POST | `/api/homes/{id}/tasks/{taskId}/complete` | ✓ | Conclui tarefa + reagenda via IA |
| POST | `/api/homes/{id}/tasks/{taskId}/skip` | ✓ | Posterga tarefa + reagenda via IA |
| GET | `/api/alerts` | ✓ | Lista alertas (dispara check-alerts se 1h passou) |
| PUT | `/api/alerts/{alertId}/read` | ✓ | Marca alerta como lido |
| POST | `/api/chat` | ✓ | Mensagem para o agente IA |

## Configuração

Criar `CasaLog.Api/appsettings.Development.json`:

```json
{
  "Llm": {
    "Provider": "ollama",
    "ApiKey": "",
    "BaseUrl": "http://localhost:11434/v1",
    "Model": "llama3.2"
  },
  "ConnectionStrings": {
    "Default": "Data Source=casalog.db"
  },
  "Jwt": {
    "Secret": "sua-chave-secreta-minimo-32-chars-aqui!!",
    "ExpirationDays": 7
  },
  "RateLimit": {
    "ChatPerMinute": 20,
    "ScheduleGeneratePerHour": 3,
    "CheckAlertsPerHour": 10
  }
}
```

Para Groq: `"Provider": "groq"` + `"ApiKey": "gsk_..."`.

## Comandos

```bash
dotnet build
dotnet run --project CasaLog.Api
dotnet test -c Release   # 84 testes de integração
```

## Ações do agente (9 total)

| Ação | Trigger | Descrição |
|------|---------|-----------|
| `generate_schedule` | POST /homes, POST equipment | Cronograma 12 meses, máx 3 tarefas/mês |
| `evaluate_alert` | GET /alerts | Decide se gera alerta para cada tarefa |
| `reschedule_task` | complete, skip | Nova data com ajuste por notas |
| `answer_maintenance` | chat | Resposta a perguntas técnicas |
| `seasonal_tips` | chat | Dicas por estação |
| `unknown` | chat (fora de escopo) | Rejeição com explicação |
| `suggest_equipment` | HomeSetup frontend | Sugestões de equipamentos por perfil |
| `home_report` | GET /homes/{id}/report | Score 0–100 + recomendações |
| `normalize_equipment` | POST equipment | Normaliza tipo informado pelo usuário |

## Decisões arquiteturais

- **Falha do LLM no POST /homes**: home é criada, schedule não. Usuário gera manualmente via chat.
- **Falha do LLM em normalize_equipment**: retorna 502 — equipamento não é adicionado.
- **Falha do LLM em generate_schedule (addEquipment)**: equipamento é salvo, tasks não são geradas.
- **check-alerts**: dispara automaticamente no GET /api/alerts, throttle de 1h por residência no banco.
- **Múltiplos equipamentos do mesmo tipo**: permitido.
- **Remoção de equipment**: cascade — tasks `pending` vinculadas são apagadas; tasks `completed`/`skipped` preservadas.
- **LLM config validation**: lazy — falha na primeira chamada LLM com 502.
- **Agente stateless**: nova sessão MAF por chamada, sem `conversationId`.
- **Prompt injection**: mensagens com `system:`, `assistant:`, `[INST]`, `<s>` → `action:unknown` sem chamar LLM.
- **Alerta duplicado**: verifica alerta não lido existente antes de chamar IA.
- **Stack trace**: nunca exposto em erros 502.
- **Informação interna**: erros de AgentException retornam `"AI service unavailable."` — provider, endpoint e credenciais nunca vazam.
- **Limites de entrada**: nome ≤ 100, e-mail ≤ 254 (RFC 5321), senha 6–128, mensagem de chat ≤ 2000 chars.
- **IChatClient API**: usa `GetResponseAsync` + `ChatResponse.Text` (Microsoft.Extensions.AI 10.x).
