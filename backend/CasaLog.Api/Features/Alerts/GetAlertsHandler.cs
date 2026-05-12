namespace CasaLog.Api.Features.Alerts;

public static class GetAlertsHandler
{
    public record TaskDto(Guid Id, string Type, string ScheduledDate);
    public record Response(Guid Id, string Type, string Message, TaskDto Task, DateTime CreatedAt);

    private static readonly TimeSpan CheckThrottle = TimeSpan.FromHours(1);

    public static async Task<IResult> Handle(
        HttpContext ctx,
        CasaLogContext db,
        IHomeAgent agent,
        CancellationToken ct)
    {
        var userId = ctx.GetUserId();
        if (userId == Guid.Empty) return Results.Unauthorized();

        var home = await db.Homes
            .Include(h => h.Equipment)
            .Include(h => h.ScheduledTasks).ThenInclude(t => t.Alerts)
            .Include(h => h.ScheduledTasks).ThenInclude(t => t.Equipment)
            .FirstOrDefaultAsync(h => h.UserId == userId, ct);

        if (home is null) return Results.NotFound(new { error = "Home not found." });

        var now = DateTime.UtcNow;
        var shouldCheck = home.LastAlertCheck is null || now - home.LastAlertCheck.Value >= CheckThrottle;

        if (shouldCheck)
        {
            var today = DateOnly.FromDateTime(now);
            var pendingTasks = home.ScheduledTasks
                .Where(t => t.Status == "pending" && t.ScheduledDate <= today.AddDays(30))
                .ToList();

            foreach (var task in pendingTasks)
            {
                // Skip if unread alert already exists for this task
                if (task.Alerts.Any(a => !a.IsRead)) continue;

                try
                {
                    var alertCount = task.Alerts.Count;
                    var evaluation = await agent.EvaluateAlertAsync(home, task, alertCount, ct);
                    if (evaluation.ShouldAlert && !string.IsNullOrWhiteSpace(evaluation.AlertType))
                    {
                        db.Alerts.Add(new Alert
                        {
                            HomeId = home.Id,
                            TaskId = task.Id,
                            Type = evaluation.AlertType,
                            Message = evaluation.Message
                        });
                    }
                }
                catch (AgentException)
                {
                    // Continue to next task on agent failure
                }
            }

            home.LastAlertCheck = now;
            await db.SaveChangesAsync(ct);
        }

        var alerts = await db.Alerts
            .Include(a => a.Task)
            .Where(a => a.HomeId == home.Id && !a.IsRead)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new Response(
                a.Id, a.Type, a.Message,
                new TaskDto(a.Task.Id, a.Task.Type, a.Task.ScheduledDate.ToString("yyyy-MM-dd")),
                a.CreatedAt
            ))
            .ToListAsync(ct);

        return Results.Ok(alerts);
    }
}
