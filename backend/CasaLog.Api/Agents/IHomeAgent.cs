namespace CasaLog.Api.Agents;

public interface IHomeAgent
{
    Task<GenerateScheduleResponse> GenerateScheduleAsync(Home home, CancellationToken ct = default);
    Task<EvaluateAlertResponse> EvaluateAlertAsync(Home home, ScheduledTask task, int existingAlertCount, CancellationToken ct = default);
    Task<RescheduleTaskResponse> RescheduleTaskAsync(Home home, ScheduledTask task, bool skipped, CancellationToken ct = default);
    Task<ChatResponse> ChatAsync(Home home, string userMessage, CancellationToken ct = default);
}
