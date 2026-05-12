---
name: CasaLog
colors:
  background: '#f9fafb'
  surface: '#ffffff'
  surface-secondary: '#f3f4f6'
  border: '#e5e7eb'
  border-strong: '#d1d5db'
  primary: '#2563eb'
  primary-hover: '#1d4ed8'
  primary-text: '#ffffff'
  on-surface: '#111827'
  on-surface-secondary: '#6b7280'
  on-surface-tertiary: '#9ca3af'
  success: '#16a34a'
  success-bg: '#f0fdf4'
  warning: '#d97706'
  warning-bg: '#fffbeb'
  error: '#dc2626'
  error-bg: '#fef2f2'
  info: '#2563eb'
  info-bg: '#eff6ff'
  task-pending: '#f59e0b'
  task-done: '#16a34a'
  task-skipped: '#9ca3af'
  priority-low: '#9ca3af'
  priority-medium: '#d97706'
  priority-high: '#ea580c'
  priority-critical: '#dc2626'
  alert-approaching: '#ea580c'
  alert-overdue: '#dc2626'
  alert-seasonal: '#9333ea'
typography:
  h1:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '700'
    lineHeight: '1.25'
  h2:
    fontFamily: Inter
    fontSize: 18px
    fontWeight: '600'
    lineHeight: '1.35'
  h3:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '600'
    lineHeight: '1.4'
  body:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: '1.5'
  caption:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '400'
    lineHeight: '1.4'
  label:
    fontFamily: Inter
    fontSize: 13px
    fontWeight: '500'
    lineHeight: '1.4'
spacing:
  xs: 4px
  sm: 8px
  md: 16px
  lg: 24px
  xl: 32px
  2xl: 48px
---

## Brand & Estilo

CasaLog tem personalidade utilitária e confiável — como um caderno de anotações bem organizado para a casa. O visual é limpo, moderno e sem distrações, priorizando legibilidade dos dados de manutenção sobre estética elaborada.

O design é **mobile-first** (max-width 672px centralizado), otimizado para uso rápido: verificar tarefas, marcar conclusões, ler alertas.

## Cores

Base em cinza neutro claro (`#f9fafb`) com superfícies brancas. Azul (`#2563eb`) como cor **exclusiva de ação e brand** — botões primários, tabs ativas, mês atual. Nunca usar azul para estados de conteúdo (prioridade, alerta) para evitar conflito semântico.

### Status de tarefas
- **Pendente** — âmbar (`#f59e0b`)
- **Concluída** — verde (`#16a34a`)
- **Pulada** — cinza (`#9ca3af`)

### Prioridade (badges de tarefa)
Escala contínua de urgência — cinza → âmbar → laranja → vermelho:
- **low** — cinza (`bg-gray-100 text-gray-700`)
- **medium** — âmbar (`bg-amber-100 text-amber-700`) — não usar azul (reservado ao brand)
- **high** — laranja (`bg-orange-100 text-orange-700`)
- **critical** — vermelho (`bg-red-100 text-red-700`)

### Alertas (cards)
- **approaching** — laranja (`border-orange-200 bg-orange-50`) — vence em breve
- **overdue** — vermelho (`border-red-200 bg-red-50`) — vencida
- **seasonal** — roxo (`border-purple-200 bg-purple-50`) — sazonal, exige ação mas não é emergência; roxo diferencia de sucesso (verde) e urgência (laranja/vermelho)

## Tipografia

Inter em todos os textos. Tamanhos reduzidos para densidade de informação em mobile: body 14px, caption 12px. Hierarquia por peso (400 / 500 / 600 / 700), não por tamanho excessivo.

## Layout

Single-column, max-width 672px, centralizado. Header fixo com nome da residência + tabs de navegação. Main com padding 16px lateral e 24px topo.

Tabs de navegação: 5 abas (`Calendário`, `Tarefas`, `Alertas`, `Chat IA`, `?`) com indicador de borda inferior azul na ativa.

## Elevação

Sem sombras profundas. Separação por `border: 1px solid #e5e7eb`. Cards com `background: #ffffff` sobre fundo `#f9fafb`. Sticky header com `border-bottom`.

## Shapes

Border-radius padrão `rounded` (6px) para cards e inputs. `rounded-full` para badges de status e avatares. Botões com `rounded-md` (8px).

## Componentes

### Header
Sticky top-0, branco, border-bottom cinza. Linha superior: nome "CasaLog" + tipo da residência + botão "Sair". Linha inferior: tabs de navegação.

### AuthForm
Centralizado na tela, card branco com sombra suave. Formulário simples: email + senha + botão primário. Toggle entre login e cadastro.

### HomeSetup
Formulário de cadastro de residência: tipo (casa/apartamento), tamanho (m²), jardim (checkbox). Seção de equipamentos: lista com botão "Adicionar" e delete por item.

### MaintenanceCalendar
Grid `3-col → 4-col sm` com janela rolante de 12 meses a partir do mês atual. Cabeçalho "Próximos 12 meses — tarefas pendentes". Mês atual: card com `border-blue-300 bg-blue-50`. Demais: `border-gray-200 bg-white`. Meses além do ano atual exibem o ano no label (ex: "Abr 2027"). Cada tarefa: badge colorido por prioridade com nome da tarefa. Células sem tarefas: `—` em cinza.

### TaskList
Filter tabs por status (Pendentes / Concluídas / Puladas) com pill ativo em `bg-blue-600`. Cada card: nome da tarefa + badge de prioridade colorido + nome do equipamento + motivo + data relativa ("Vencida há N dias" em vermelho / "em N dias" em cinza ou laranja se ≤ 7 dias). Ações visíveis apenas em Pendentes: botão "Pular" (border ghost) + botão "✓ Feito" (verde). Cards vencidos com `border-red-200`.

### AlertsPanel
Mostra apenas alertas não lidos (marcar como lido remove da lista via `✕`). Cada card: borda colorida por tipo (`approaching` laranja, `overdue` vermelho, `seasonal` roxo), ícone emoji, nome da tarefa em negrito, mensagem da IA, data agendada em `pt-BR`. Loading: "Verificando alertas...". Empty state: ✅ + "Nenhum alerta pendente". Contador no topo: "N alerta(s) não lido(s)".

### ChatAgent
Interface de chat clássica. Mensagens do usuário alinhadas à direita (azul), respostas do agente à esquerda (cinza). Input fixo na base do painel com botão de enviar. Indicador de loading durante resposta.

### HelpPage
Página de documentação integrada acessada pela aba `?`. Seções:
- **Hero card** azul com título e descrição do app
- **Como começar**: passos numerados em card branco com bullets azuis
- **Funcionalidades**: cards individuais com ícone, badge de categoria (Configuração / Principal / IA), descrição e lista de detalhes
- **Tipos de manutenção**: tabela com tarefa, intervalo e badge de prioridade colorido
- **Sazonalidade brasileira**: grid 2×2 com card por estação, ícone e dica principal

### Inputs e Botões

**Input:** border cinza, focus ring azul, border-radius 6px, padding `8px 12px`.

**Botão primário:** `bg-blue-600` + texto branco + hover `bg-blue-700`.

**Botão secundário/ghost:** border cinza + texto cinza escuro + hover border mais escuro.

**Botão destrutivo:** texto vermelho ou `bg-red-600`.
