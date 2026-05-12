namespace CasaLog.Api.Features.Homes;

public static class CreateHomeHandler
{
    public record Request(string Type, bool HasGarden);
    public record TaskSummary(Guid Id, string Type, string ScheduledDate, string Priority, string Reason);
    public record SuggestionDto(string Type, string Name, string Reason);
    public record Response(Guid Id, string Type, bool HasGarden, List<TaskSummary> Tasks, List<SuggestionDto> Suggestions);

    public static async Task<IResult> Handle(
        Request req,
        HttpContext ctx,
        CasaLogContext db,
        IHomeAgent agent,
        CancellationToken ct)
    {
        var userId = ctx.GetUserId();
        if (userId == Guid.Empty) return Results.Unauthorized();

        if (req.Type is not ("house" or "apartment"))
            return Results.BadRequest(new { error = "Type must be 'house' or 'apartment'." });

        if (await db.Homes.AnyAsync(h => h.UserId == userId, ct))
            return Results.Conflict(new { error = "User already has a home." });

        var home = new Home
        {
            UserId = userId,
            Type = req.Type,
            HasGarden = req.HasGarden
        };

        db.Homes.Add(home);
        await db.SaveChangesAsync(ct);

        var scheduleTask = agent.GenerateScheduleAsync(home, ct);
        var suggestTask = agent.SuggestEquipmentAsync(home, ct);

        List<ScheduledTask> tasks = [];
        List<SuggestionDto> suggestions = [];

        try { await Task.WhenAll(scheduleTask, suggestTask); } catch { }

        if (scheduleTask.IsCompletedSuccessfully)
        {
            tasks = MapToTasks(scheduleTask.Result.Tasks, home.Id, home.Equipment);
            db.ScheduledTasks.AddRange(tasks);
            await db.SaveChangesAsync(ct);
        }

        if (suggestTask.IsCompletedSuccessfully)
            suggestions = suggestTask.Result?.Suggestions?.Select(s => new SuggestionDto(s.Type, s.Name, s.Reason)).ToList() ?? [];

        var summary = tasks.Select(t => new TaskSummary(
            t.Id, t.Type, t.ScheduledDate.ToString("yyyy-MM-dd"), t.Priority, t.Reason
        )).ToList();

        return Results.Created($"/api/homes/{home.Id}", new Response(home.Id, home.Type, home.HasGarden, summary, suggestions));
    }

    internal static List<ScheduledTask> MapToTasks(AgentTaskItem[] items, Guid homeId, IReadOnlyList<Equipment> equipment)
    {
        var tasks = new List<ScheduledTask>();
        foreach (var item in items)
        {
            if (!DateOnly.TryParse(item.ScheduledDate, out var date)) continue;
            var priority = item.Priority is "low" or "medium" or "high" or "critical" ? item.Priority : "medium";
            var eqMatch = item.EquipmentType is not null
                ? equipment.FirstOrDefault(e => e.Type == item.EquipmentType || e.Type == item.Type)
                : null;

            tasks.Add(new ScheduledTask
            {
                HomeId = homeId,
                EquipmentId = eqMatch?.Id,
                Type = item.Type,
                ScheduledDate = date,
                Priority = priority,
                Reason = item.Reason
            });
        }
        return tasks;
    }
}
