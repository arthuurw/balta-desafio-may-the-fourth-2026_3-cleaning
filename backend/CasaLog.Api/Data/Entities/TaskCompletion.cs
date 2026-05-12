namespace CasaLog.Api.Data.Entities;

public class TaskCompletion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TaskId { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public Guid? NextTaskId { get; set; }
    public ScheduledTask Task { get; set; } = default!;
}
