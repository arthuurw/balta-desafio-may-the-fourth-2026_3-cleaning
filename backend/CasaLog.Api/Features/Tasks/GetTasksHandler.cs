namespace CasaLog.Api.Features.Tasks;

public static class GetTasksHandler
{
    public record EquipmentDto(Guid Id, string Type, string Name);
    public record Response(Guid Id, string Type, string ScheduledDate, string Priority, string Reason, string Status, EquipmentDto? Equipment);

    public static async Task<IResult> Handle(
        Guid homeId,
        string? status,
        string? month,
        HttpContext ctx,
        CasaLogContext db,
        CancellationToken ct)
    {
        var userId = ctx.GetUserId();
        if (userId == Guid.Empty) return Results.Unauthorized();

        var exists = await db.Homes.AnyAsync(h => h.Id == homeId && h.UserId == userId, ct);
        if (!exists) return Results.NotFound(new { error = "Home not found." });

        var query = db.ScheduledTasks
            .Include(t => t.Equipment)
            .Where(t => t.HomeId == homeId);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(t => t.Status == status);

        if (!string.IsNullOrWhiteSpace(month) && int.TryParse(month, out var m))
            query = query.Where(t => t.ScheduledDate.Month == m);

        var tasks = await query
            .OrderBy(t => t.ScheduledDate)
            .Select(t => new Response(
                t.Id, t.Type, t.ScheduledDate.ToString("yyyy-MM-dd"),
                t.Priority, t.Reason, t.Status,
                t.Equipment != null ? new EquipmentDto(t.Equipment.Id, t.Equipment.Type, t.Equipment.Name) : null
            ))
            .ToListAsync(ct);

        return Results.Ok(tasks);
    }
}
