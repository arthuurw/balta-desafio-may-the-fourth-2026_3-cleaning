<img width="1280" height="630" alt="banner" src="https://github.com/user-attachments/assets/eb2f345f-7b28-41d0-b374-6336dc8f8f75" />

## May The Fourth 2026 - Desafio 3: Cleaning

**Arthur Webster** — desafio de manutenção residencial com IA, realizado pelo [balta.io](https://balta.io).

### Sobre o projeto

**CasaLog** é um app fullstack para manutenção preventiva residencial. O usuário cadastra sua residência e equipamentos (ar-condicionado, filtro de água, caixa d'água, etc.), e a IA distribui tarefas ao longo de 12 meses, enviando alertas apenas quando necessário.

### Stack

| Camada | Tecnologia |
|--------|-----------|
| Backend | .NET 10 Minimal API, Vertical Slice, EF Core 10, SQLite |
| Frontend | React 19, TypeScript, Vite, Tailwind CSS v4 |
| Agente IA | Microsoft Agents Framework (MAF) 1.4.0 |
| LLM dev | Ollama + llama3.2 |
| LLM prod | Groq + llama-3.3-70b-versatile |
| Auth | JWT Bearer + PasswordHasher |

### Como rodar

**Pré-requisitos:** .NET 10 SDK, Node.js 20+, Ollama (dev)

```bash
# Backend
cd backend
cp CasaLog.Api/appsettings.Development.json.example CasaLog.Api/appsettings.Development.json
dotnet run --project CasaLog.Api

# Frontend (outro terminal)
cd frontend
npm install
npm run dev
```

- Backend: `http://localhost:5059`
- Frontend: `http://localhost:5173`
- Swagger: `http://localhost:5059/swagger`

### Funcionalidades

- Cadastro e autenticação de usuários (JWT)
- Registro de residência (casa/apartamento, com/sem jardim)
- Gestão de equipamentos com normalização via IA e cascade delete
- Geração de cronograma anual via IA (MAF + LLM)
- Reagendamento inteligente ao concluir/pular tarefa, com ajuste por notas
- Calendário visual de manutenção por mês
- Conclusão e postergação de tarefas com registro de notas
- Alertas automáticos com throttle de 1h por residência
- Relatório de saúde da residência com score 0–100 via IA
- Chat com agente de manutenção residencial
- Página de ajuda integrada com guia de funcionalidades
- Rate limiting in-memory por endpoint
- Validação de tamanho de entrada (nome ≤ 100, e-mail ≤ 254, senha 6–128, mensagem ≤ 2000 chars)
- 84 testes de integração (backend) + 25 testes Vitest (frontend)

### O que aprendi

- Arquitetura Vertical Slice com Minimal API
- Microsoft Agents Framework (MAF) 1.4.0 — roteamento de 9 ações, injeção de contexto
- Integração com LLMs via OpenAI-compatible API (Ollama + Groq)
- Proteção contra prompt injection no system prompt do agente
- JWT auth em .NET 10 sem Identity
- `Microsoft.Extensions.AI` 10.x — migração de `CompleteAsync` → `GetResponseAsync`

## Badge

<img src="https://baltaio.blob.core.windows.net/static/images/v4/challenges/may-the-fourth-2026/rewards/cleaning/image.png" width="200" />

## Sobre o May The Fourth 2026

O desafio **May The Fourth 2026** consiste em implementar agentes e inteligência artificial em cenários reais, resolvendo problemas do dia-a-dia com Microsoft Agent Framework, C# e .NET.

### Recursos

- [Imersão - Microsoft Agents Framework](https://www.youtube.com/watch?v=XkgjeBurtFw)
- [Curso - Microsoft Agents Framework](https://balta.io/cursos/fundamentos-do-microsoft-agent-framework)
