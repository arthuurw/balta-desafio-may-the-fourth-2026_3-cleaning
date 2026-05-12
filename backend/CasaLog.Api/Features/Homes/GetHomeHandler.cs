namespace CasaLog.Api.Features.Homes;

public static class GetHomeHandler
{
    public record EquipmentDto(Guid Id, string Type, string Name, string? InstalledAt);
    public record TaskDto(Guid Id, string Type, string ScheduledDate, string Priority, string Reason, string Status, EquipmentDto? Equipment);
    public record Response(Guid Id, string Type, bool HasGarden, List<EquipmentDto> Equipment, List<TaskDto> UpcomingTasks);

    public static async Task<IResult> Handle(HttpContext ctx, CasaLogContext db, CancellationToken ct)
    {
        var userId = ctx.GetUserId();
        if (userId == Guid.Empty) return Results.Unauthorized();

        var home = await db.Homes
            .Include(h => h.Equipment)
            .Include(h => h.ScheduledTasks).ThenInclude(t => t.Equipment)
            .FirstOrDefaultAsync(h => h.UserId == userId, ct);

        if (home is null) return Results.NotFound(new { error = "Home not found." });

        var equipment = home.Equipment
            .Select(e => new EquipmentDto(e.Id, e.Type, e.Name, e.InstalledAt?.ToString("yyyy-MM-dd")))
            .ToList();

        var upcoming = home.ScheduledTasks
            .Where(t => t.Status == "pending")
            .OrderBy(t => t.ScheduledDate)
            .Take(20)
            .Select(t => new TaskDto(
                t.Id, t.Type, t.ScheduledDate.ToString("yyyy-MM-dd"),
                t.Priority, t.Reason, t.Status,
                t.Equipment is not null ? new EquipmentDto(t.Equipment.Id, t.Equipment.Type, t.Equipment.Name, t.Equipment.InstalledAt?.ToString("yyyy-MM-dd")) : null
            ))
            .ToList();

        return Results.Ok(new Response(home.Id, home.Type, home.HasGarden, equipment, upcoming));
    }
}
