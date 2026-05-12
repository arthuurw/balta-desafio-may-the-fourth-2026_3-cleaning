namespace CasaLog.Api.Data.Entities;

public class Home
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Type { get; set; } = default!;
    public bool HasGarden { get; set; }
    public DateTime? LastAlertCheck { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = default!;
    public List<Equipment> Equipment { get; set; } = [];
    public List<ScheduledTask> ScheduledTasks { get; set; } = [];
    public List<Alert> Alerts { get; set; } = [];
}
