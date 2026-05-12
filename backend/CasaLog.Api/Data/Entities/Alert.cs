namespace CasaLog.Api.Data.Entities;

public class Alert
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid HomeId { get; set; }
    public Guid TaskId { get; set; }
    public string Type { get; set; } = default!;
    public string Message { get; set; } = default!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Home Home { get; set; } = default!;
    public ScheduledTask Task { get; set; } = default!;
}
