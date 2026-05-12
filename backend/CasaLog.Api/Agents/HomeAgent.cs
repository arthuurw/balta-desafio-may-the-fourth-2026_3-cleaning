using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text;

namespace CasaLog.Api.Agents;

public class HomeAgent : IHomeAgent
{
    private readonly ChatClientAgent _agent;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ChatClientAgentRunOptions _runOptions;

    public HomeAgent(IChatClient chatClient, IConfiguration config, IWebHostEnvironment env)
    {
        var promptRelPath = config["AgentPromptPath"] ?? "../../agents/agente-casa.md";
        var promptPath = Path.GetFullPath(Path.Combine(env.ContentRootPath, promptRelPath));
        var systemPrompt = File.ReadAllText(promptPath);

        _agent = (ChatClientAgent)chatClient.AsAIAgent(systemPrompt, null!, null!, null, null, null);
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        _runOptions = new ChatClientAgentRunOptions(new ChatOptions
        {
            ResponseFormat = ChatResponseFormat.Json
        });
    }

    public async Task<GenerateScheduleResponse> GenerateScheduleAsync(Home home, CancellationToken ct = default)
        => await RunAsync<GenerateScheduleResponse>(BuildSchedulePrompt(home), ct);

    public async Task<EvaluateAlertResponse> EvaluateAlertAsync(Home home, ScheduledTask task, int existingAlertCount, CancellationToken ct = default)
        => await RunAsync<EvaluateAlertResponse>(BuildEvaluateAlertPrompt(home, task, existingAlertCount), ct);

    public async Task<RescheduleTaskResponse> RescheduleTaskAsync(Home home, ScheduledTask task, bool skipped, CancellationToken ct = default)
        => await RunAsync<RescheduleTaskResponse>(BuildReschedulePrompt(home, task, skipped), ct);

    public async Task<ChatResponse> ChatAsync(Home home, string userMessage, CancellationToken ct = default)
        => await RunAsync<ChatResponse>(BuildChatPrompt(home, userMessage), ct);

    private async Task<T> RunAsync<T>(string prompt, CancellationToken ct)
    {
        try
        {
            var session = await _agent.CreateSessionAsync(ct);
            var response = await _agent.RunAsync<T>(prompt, session, _jsonOptions, _runOptions, ct);
            return response.Result;
        }
        catch (Exception ex) when (ex is not AgentException)
        {
            throw new AgentException($"Agent call failed: {ex.Message}", ex);
        }
    }

    private static string BuildSchedulePrompt(Home home)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Data atual: {DateTime.UtcNow:yyyy-MM-dd}");
        sb.AppendLine();
        sb.AppendLine("Contexto da residência:");
        sb.AppendLine($"- Tipo: {home.Type}");
        sb.AppendLine($"- Tem jardim: {(home.HasGarden ? "sim" : "não")}");
        sb.AppendLine();
        sb.AppendLine("Equipamentos cadastrados:");
        if (home.Equipment.Count == 0)
            sb.AppendLine("- Nenhum equipamento específico cadastrado.");
        else
            foreach (var eq in home.Equipment)
                sb.AppendLine($"- {eq.Type}: {eq.Name}" +
                    (eq.InstalledAt.HasValue ? $" (instalado em {eq.InstalledAt:yyyy-MM-dd})" : ""));

        var pending = home.ScheduledTasks.Where(t => t.Status == "pending").OrderBy(t => t.ScheduledDate).Take(20).ToList();
        if (pending.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Tarefas já agendadas (não duplique estes tipos nos mesmos meses):");
            foreach (var t in pending)
                sb.AppendLine($"- {t.Type}: {t.ScheduledDate:yyyy-MM-dd} ({t.Priority})");
        }

        sb.AppendLine();
        sb.AppendLine("Ação solicitada: generate_schedule");
        sb.AppendLine("Gere o cronograma completo de manutenção preventiva para os próximos 12 meses.");
        return sb.ToString();
    }

    private static string BuildEvaluateAlertPrompt(Home home, ScheduledTask task, int existingAlertCount)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var daysUntil = task.ScheduledDate.DayNumber - today.DayNumber;
        var status = daysUntil < 0 ? "vencida" : $"vence em {daysUntil} dias ({task.ScheduledDate:yyyy-MM-dd})";

        var sb = new StringBuilder();
        sb.AppendLine($"Data atual: {today:yyyy-MM-dd}");
        sb.AppendLine();
        sb.AppendLine("Tarefa para avaliação:");
        sb.AppendLine($"- Tipo: {task.Type}");
        sb.AppendLine($"- Equipamento: {task.Equipment?.Name ?? "N/A"}");
        sb.AppendLine($"- Data agendada: {task.ScheduledDate:yyyy-MM-dd} ({status})");
        sb.AppendLine($"- Prioridade: {task.Priority}");
        sb.AppendLine($"- Status: {task.Status}");
        sb.AppendLine($"- Alertas já enviados para esta tarefa: {existingAlertCount}");
        sb.AppendLine();
        sb.AppendLine($"Contexto: residência tipo '{home.Type}', jardim: {(home.HasGarden ? "sim" : "não")}");
        sb.AppendLine();
        sb.AppendLine("Ação solicitada: evaluate_alert");
        sb.AppendLine("Avalie se deve ser enviado um alerta para esta tarefa.");
        return sb.ToString();
    }

    private static string BuildReschedulePrompt(Home home, ScheduledTask task, bool skipped)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var action = skipped ? "pulada" : "concluída";

        var sb = new StringBuilder();
        sb.AppendLine($"Data atual: {today:yyyy-MM-dd}");
        sb.AppendLine();
        sb.AppendLine($"Tarefa {action}:");
        sb.AppendLine($"- Tipo: {task.Type}");
        sb.AppendLine($"- Data anterior: {task.ScheduledDate:yyyy-MM-dd}");
        sb.AppendLine($"- Prioridade: {task.Priority}");
        sb.AppendLine($"- Razão original: {task.Reason}");
        sb.AppendLine();

        var upcoming = home.ScheduledTasks
            .Where(t => t.Status == "pending" && t.ScheduledDate >= today)
            .OrderBy(t => t.ScheduledDate)
            .Take(15)
            .ToList();

        if (upcoming.Count > 0)
        {
            sb.AppendLine("Tarefas já agendadas (considere para não sobrecarregar meses):");
            foreach (var t in upcoming)
                sb.AppendLine($"- {t.Type}: {t.ScheduledDate:yyyy-MM-dd}");
        }

        sb.AppendLine();
        sb.AppendLine("Ação solicitada: reschedule_task");
        sb.AppendLine("Calcule a próxima data de manutenção para esta tarefa.");
        return sb.ToString();
    }

    private static string BuildChatPrompt(Home home, string userMessage)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var sb = new StringBuilder();
        sb.AppendLine($"Data atual: {today:yyyy-MM-dd}");
        sb.AppendLine();
        sb.AppendLine("Contexto da residência:");
        sb.AppendLine($"- Tipo: {home.Type}");
        sb.AppendLine($"- Tem jardim: {(home.HasGarden ? "sim" : "não")}");
        sb.AppendLine();

        var upcoming = home.ScheduledTasks
            .Where(t => t.Status == "pending")
            .OrderBy(t => t.ScheduledDate)
            .Take(10)
            .ToList();

        if (upcoming.Count > 0)
        {
            sb.AppendLine("Próximas tarefas agendadas:");
            foreach (var t in upcoming)
                sb.AppendLine($"- {t.Type}: {t.ScheduledDate:yyyy-MM-dd} ({t.Priority}) — {t.Reason}");
        }

        sb.AppendLine();
        sb.AppendLine($"Mensagem do usuário: {userMessage}");
        return sb.ToString();
    }
}
