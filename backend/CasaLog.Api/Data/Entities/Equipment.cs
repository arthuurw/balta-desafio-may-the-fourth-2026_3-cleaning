namespace CasaLog.Api.Data.Entities;

public class Equipment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid HomeId { get; set; }
    public string Type { get; set; } = default!;
    public string Name { get; set; } = default!;
    public DateOnly? InstalledAt { get; set; }
    public Home Home { get; set; } = default!;
    public List<ScheduledTask> ScheduledTasks { get; set; } = [];
}
