namespace CasaLog.Api.Features.Tasks;

public static class SkipTaskHandler
{
    public record NextTaskDto(Guid Id, string Type, string ScheduledDate, string Priority);
    public record Response(NextTaskDto? NextTask);

    public static async Task<IResult> Handle(
        Guid homeId,
        Guid taskId,
        HttpContext ctx,
        CasaLogContext db,
        IHomeAgent agent,
        CancellationToken ct)
    {
        var userId = ctx.GetUserId();
        if (userId == Guid.Empty) return Results.Unauthorized();

        var home = await db.Homes
            .Include(h => h.ScheduledTasks)
            .FirstOrDefaultAsync(h => h.Id == homeId && h.UserId == userId, ct);

        if (home is null) return Results.NotFound(new { error = "Home not found." });

        var task = home.ScheduledTasks.FirstOrDefault(t => t.Id == taskId);
        if (task is null) return Results.NotFound(new { error = "Task not found." });
        if (task.Status != "pending") return Results.BadRequest(new { error = "Task is not pending." });

        task.Status = "skipped";
        await db.SaveChangesAsync(ct);

        ScheduledTask? nextTask = null;
        try
        {
            var reschedule = await agent.RescheduleTaskAsync(home, task, skipped: true, ct);
            if (DateOnly.TryParse(reschedule.NewDate, out var newDate))
            {
                nextTask = new ScheduledTask
                {
                    HomeId = homeId,
                    EquipmentId = task.EquipmentId,
                    Type = task.Type,
                    ScheduledDate = newDate,
                    Priority = task.Priority,
                    Reason = reschedule.Reason
                };
                db.ScheduledTasks.Add(nextTask);
                await db.SaveChangesAsync(ct);
            }
        }
        catch (AgentException)
        {
            // AI unavailable — task skipped, reschedule pending
        }

        var nextDto = nextTask is not null
            ? new NextTaskDto(nextTask.Id, nextTask.Type, nextTask.ScheduledDate.ToString("yyyy-MM-dd"), nextTask.Priority)
            : null;

        return Results.Ok(new Response(nextDto));
    }
}
