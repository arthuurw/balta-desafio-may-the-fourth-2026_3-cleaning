using CasaLog.Api.Features.Homes;

namespace CasaLog.Api.Features.Equipments;

public static class AddEquipmentHandler
{
    public record Request(string Type, string Name, string? InstalledAt);
    public record Response(Guid Id, string Type, string Name, string? InstalledAt, List<CreateHomeHandler.TaskSummary> Tasks);

    public static async Task<IResult> Handle(
        Guid homeId,
        Request req,
        HttpContext ctx,
        CasaLogContext db,
        IHomeAgent agent,
        CancellationToken ct)
    {
        var userId = ctx.GetUserId();
        if (userId == Guid.Empty) return Results.Unauthorized();

        var home = await db.Homes
            .Include(h => h.Equipment)
            .Include(h => h.ScheduledTasks)
            .FirstOrDefaultAsync(h => h.Id == homeId && h.UserId == userId, ct);

        if (home is null) return Results.NotFound(new { error = "Home not found." });

        if (string.IsNullOrWhiteSpace(req.Type) || string.IsNullOrWhiteSpace(req.Name))
            return Results.BadRequest(new { error = "Type and name are required." });

        string normalizedType;
        try
        {
            var norm = await agent.NormalizeEquipmentTypeAsync(req.Type, req.Name, home.Type, ct);
            if (!norm.Valid)
                return Results.BadRequest(new { error = norm.Reason });
            normalizedType = norm.NormalizedType ?? req.Type.Trim();
        }
        catch (AgentException)
        {
            return Results.Problem("AI service unavailable.", statusCode: 502);
        }

        DateOnly? installedAt = null;
        if (req.InstalledAt is not null && DateOnly.TryParse(req.InstalledAt, out var parsed))
            installedAt = parsed;

        var equipment = new Data.Entities.Equipment
        {
            HomeId = homeId,
            Type = normalizedType,
            Name = req.Name.Trim(),
            InstalledAt = installedAt
        };

        db.Equipment.Add(equipment);
        await db.SaveChangesAsync(ct);

        List<ScheduledTask> tasks = [];
        try
        {
            var miniHome = new Home
            {
                Id = home.Id,
                Type = home.Type,
                HasGarden = home.HasGarden,
                Equipment = [equipment],
                ScheduledTasks = home.ScheduledTasks
            };
            var schedule = await agent.GenerateScheduleAsync(miniHome, ct);
            tasks = CreateHomeHandler.MapToTasks(schedule.Tasks, homeId, [equipment]);
            db.ScheduledTasks.AddRange(tasks);
            await db.SaveChangesAsync(ct);
        }
        catch (AgentException)
        {
            // AI unavailable — equipment added, tasks pending
        }

        var summary = tasks.Select(t => new CreateHomeHandler.TaskSummary(
            t.Id, t.Type, t.ScheduledDate.ToString("yyyy-MM-dd"), t.Priority, t.Reason
        )).ToList();

        return Results.Created($"/api/homes/{homeId}/equipment/{equipment.Id}",
            new Response(equipment.Id, equipment.Type, equipment.Name, equipment.InstalledAt?.ToString("yyyy-MM-dd"), summary));
    }
}
