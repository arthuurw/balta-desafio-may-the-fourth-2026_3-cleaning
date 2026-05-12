namespace CasaLog.Api.Data.Entities;

public class ScheduledTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid HomeId { get; set; }
    public Guid? EquipmentId { get; set; }
    public string Type { get; set; } = default!;
    public DateOnly ScheduledDate { get; set; }
    public string Priority { get; set; } = default!;
    public string Reason { get; set; } = default!;
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Home Home { get; set; } = default!;
    public Equipment? Equipment { get; set; }
    public TaskCompletion? Completion { get; set; }
    public List<Alert> Alerts { get; set; } = [];
}
