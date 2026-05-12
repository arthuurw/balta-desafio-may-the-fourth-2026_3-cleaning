namespace CasaLog.Api.Features.Homes;

public static class GetHomeReportHandler
{
    public record RecommendationDto(string Type, string Message, string Priority);
    public record Response(int Score, string Status, string Summary, List<RecommendationDto> Recommendations);

    public static async Task<IResult> Handle(
        Guid homeId,
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

        try
        {
            var report = await agent.GetHomeReportAsync(home, ct);
            var recs = report.Recommendations
                .Select(r => new RecommendationDto(r.Type, r.Message, r.Priority))
                .ToList();
            return Results.Ok(new Response(report.Score, report.Status, report.Summary, recs));
        }
        catch (AgentException)
        {
            return Results.Problem("AI service unavailable.", statusCode: 502);
        }
    }
}
