namespace CasaLog.Api.Agents;

public interface IHomeAgent
{
    Task<GenerateScheduleResponse> GenerateScheduleAsync(Home home, CancellationToken ct = default);
    Task<EvaluateAlertResponse> EvaluateAlertAsync(Home home, ScheduledTask task, int existingAlertCount, CancellationToken ct = default);
    Task<RescheduleTaskResponse> RescheduleTaskAsync(Home home, ScheduledTask task, bool skipped, string? notes, CancellationToken ct = default);
    Task<ChatResponse> ChatAsync(Home home, string userMessage, CancellationToken ct = default);
    Task<SuggestEquipmentResponse> SuggestEquipmentAsync(Home home, CancellationToken ct = default);
    Task<HomeReportResponse> GetHomeReportAsync(Home home, CancellationToken ct = default);
    Task<NormalizeEquipmentResponse> NormalizeEquipmentTypeAsync(string rawType, string rawName, string homeType, CancellationToken ct = default);
}
