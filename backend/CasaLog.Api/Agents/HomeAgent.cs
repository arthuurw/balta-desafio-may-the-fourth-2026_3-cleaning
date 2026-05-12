using Microsoft.Extensions.AI;
using System.Runtime.InteropServices;
using System.Text;

namespace CasaLog.Api.Agents;

public class HomeAgent : IHomeAgent
{
    private static readonly TimeZoneInfo BrtZone =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")
            : TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    private readonly IChatClient _chatClient;
    private readonly string _systemPrompt;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ChatOptions _chatOptions;

    public HomeAgent(IChatClient chatClient, IConfiguration config, IWebHostEnvironment env)
    {
        _chatClient = chatClient;

        var promptRelPath = config["AgentPromptPath"] ?? "../../agents/agente-casa.md";
        var promptPath = Path.GetFullPath(Path.Combine(env.ContentRootPath, promptRelPath));
        _systemPrompt = File.ReadAllText(promptPath);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        _chatOptions = new ChatOptions { ResponseFormat = ChatResponseFormat.Json };
    }

    public async Task<GenerateScheduleResponse> GenerateScheduleAsync(Home home, CancellationToken ct = default)
        => await RunAsync<GenerateScheduleResponse>(BuildSchedulePrompt(home), ct);

    public async Task<EvaluateAlertResponse> EvaluateAlertAsync(Home home, ScheduledTask task, int existingAlertCount, CancellationToken ct = default)
        => await RunAsync<EvaluateAlertResponse>(BuildEvaluateAlertPrompt(home, task, existingAlertCount), ct);

    public async Task<RescheduleTaskResponse> RescheduleTaskAsync(Home home, ScheduledTask task, bool skipped, string? notes, CancellationToken ct = default)
        => await RunAsync<RescheduleTaskResponse>(BuildReschedulePrompt(home, task, skipped, notes), ct);

    public async Task<ChatResponse> ChatAsync(Home home, string userMessage, CancellationToken ct = default)
        => await RunAsync<ChatResponse>(BuildChatPrompt(home, userMessage), ct);

    public async Task<SuggestEquipmentResponse> SuggestEquipmentAsync(Home home, CancellationToken ct = default)
        => await RunAsync<SuggestEquipmentResponse>(BuildSuggestEquipmentPrompt(home), ct);

    public async Task<HomeReportResponse> GetHomeReportAsync(Home home, CancellationToken ct = default)
        => await RunAsync<HomeReportResponse>(BuildHomeReportPrompt(home), ct);

    public async Task<NormalizeEquipmentResponse> NormalizeEquipmentTypeAsync(string rawType, string rawName, string homeType, CancellationToken ct = default)
        => await RunAsync<NormalizeEquipmentResponse>(BuildNormalizeEquipmentPrompt(rawType, rawName, homeType), ct);

    private async Task<T> RunAsync<T>(string prompt, CancellationToken ct)
    {
        const int maxRetries = 2;

        for (var attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                var messages = new List<ChatMessage>
                {
                    new(ChatRole.System, _systemPrompt),
                    new(ChatRole.User, prompt)
                };

                var response = await _chatClient.GetResponseAsync(messages, _chatOptions, ct);
                var json = response.Text
                    ?? throw new InvalidOperationException("Empty response from LLM");

                return JsonSerializer.Deserialize<T>(json, _jsonOptions)
                    ?? throw new InvalidOperationException("Failed to deserialize LLM response");
            }
            catch (AgentException) { throw; }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) when (attempt < maxRetries)
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), ct);
                _ = ex;
            }
            catch (Exception ex)
            {
                throw new AgentException($"LLM call failed after {maxRetries + 1} attempts: {ex.Message}", ex);
            }
        }

        throw new AgentException("Unexpected retry loop exit.");
    }

    private static DateOnly TodayBrt() =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BrtZone));

    private static string BuildSchedulePrompt(Home home)
    {
        var today = TodayBrt();
        var sb = new StringBuilder();
        sb.AppendLine($"Data atual: {today:yyyy-MM-dd}");
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
        var today = TodayBrt();
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

    private static string BuildReschedulePrompt(Home home, ScheduledTask task, bool skipped, string? notes)
    {
        var today = TodayBrt();
        var action = skipped ? "pulada" : "concluída";

        var sb = new StringBuilder();
        sb.AppendLine($"Data atual: {today:yyyy-MM-dd}");
        sb.AppendLine();
        sb.AppendLine($"Tarefa {action}:");
        sb.AppendLine($"- Tipo: {task.Type}");
        sb.AppendLine($"- Data anterior: {task.ScheduledDate:yyyy-MM-dd}");
        sb.AppendLine($"- Prioridade: {task.Priority}");
        sb.AppendLine($"- Razão original: {task.Reason}");

        if (!string.IsNullOrWhiteSpace(notes))
        {
            sb.AppendLine();
            sb.AppendLine("Notas de conclusão (use para ajustar o intervalo):");
            sb.AppendLine($"- {notes}");
        }

        var upcoming = home.ScheduledTasks
            .Where(t => t.Status == "pending" && t.ScheduledDate >= today)
            .OrderBy(t => t.ScheduledDate)
            .Take(15)
            .ToList();

        if (upcoming.Count > 0)
        {
            sb.AppendLine();
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
        var today = TodayBrt();

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

    private static string BuildSuggestEquipmentPrompt(Home home)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Data atual: {TodayBrt():yyyy-MM-dd}");
        sb.AppendLine();
        sb.AppendLine("Perfil da residência:");
        sb.AppendLine($"- Tipo: {home.Type}");
        sb.AppendLine($"- Tem jardim: {(home.HasGarden ? "sim" : "não")}");
        sb.AppendLine();

        if (home.Equipment.Count > 0)
        {
            sb.AppendLine("Equipamentos já cadastrados (não sugira novamente):");
            foreach (var eq in home.Equipment)
                sb.AppendLine($"- {eq.Type}: {eq.Name}");
            sb.AppendLine();
        }

        sb.AppendLine("Ação solicitada: suggest_equipment");
        sb.AppendLine("Sugira os equipamentos mais importantes para cadastrar nesta residência.");
        return sb.ToString();
    }

    private static string BuildHomeReportPrompt(Home home)
    {
        var today = TodayBrt();
        var pending = home.ScheduledTasks.Where(t => t.Status == "pending").ToList();
        var overdue = pending.Where(t => t.ScheduledDate < today).ToList();
        var upcoming30 = pending.Where(t => t.ScheduledDate >= today && t.ScheduledDate <= today.AddDays(30)).ToList();
        var completed = home.ScheduledTasks.Where(t => t.Status == "completed").ToList();
        var skipped = home.ScheduledTasks.Where(t => t.Status == "skipped").ToList();
        var criticalPending = pending.Where(t => t.Priority is "critical" or "high").ToList();

        var sb = new StringBuilder();
        sb.AppendLine($"Data atual: {today:yyyy-MM-dd}");
        sb.AppendLine();
        sb.AppendLine("Residência:");
        sb.AppendLine($"- Tipo: {home.Type}");
        sb.AppendLine($"- Tem jardim: {(home.HasGarden ? "sim" : "não")}");
        sb.AppendLine($"- Equipamentos cadastrados: {home.Equipment.Count}");
        sb.AppendLine();
        sb.AppendLine("Estatísticas:");
        sb.AppendLine($"- Total tarefas: {home.ScheduledTasks.Count}");
        sb.AppendLine($"- Pendentes: {pending.Count} ({overdue.Count} vencidas)");
        sb.AppendLine($"- Vencendo em 30 dias: {upcoming30.Count}");
        sb.AppendLine($"- Críticas/Altas pendentes: {criticalPending.Count}");
        sb.AppendLine($"- Concluídas: {completed.Count}");
        sb.AppendLine($"- Puladas: {skipped.Count}");

        if (overdue.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Tarefas vencidas:");
            foreach (var t in overdue.OrderBy(t => t.ScheduledDate).Take(5))
                sb.AppendLine($"- {t.Type}: venceu em {t.ScheduledDate:yyyy-MM-dd} ({t.Priority})");
        }

        if (upcoming30.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Próximas tarefas (30 dias):");
            foreach (var t in upcoming30.OrderBy(t => t.ScheduledDate).Take(8))
                sb.AppendLine($"- {t.Type}: {t.ScheduledDate:yyyy-MM-dd} ({t.Priority})");
        }

        if (completed.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Concluídas recentemente:");
            foreach (var t in completed.OrderByDescending(t => t.CreatedAt).Take(5))
                sb.AppendLine($"- {t.Type}: {t.ScheduledDate:yyyy-MM-dd}");
        }

        sb.AppendLine();
        sb.AppendLine("Ação solicitada: home_report");
        sb.AppendLine("Gere o relatório de saúde desta residência com score de 0 a 100.");
        return sb.ToString();
    }

    private static string BuildNormalizeEquipmentPrompt(string rawType, string rawName, string homeType)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Equipamento para normalização:");
        sb.AppendLine($"- Tipo informado: {rawType}");
        sb.AppendLine($"- Nome informado: {rawName}");
        sb.AppendLine($"- Tipo de residência: {homeType}");
        sb.AppendLine();
        sb.AppendLine("Ação solicitada: normalize_equipment");
        sb.AppendLine("Normalize o tipo do equipamento para um dos tipos válidos do sistema.");
        return sb.ToString();
    }
}
