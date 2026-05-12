namespace CasaLog.Api.Features.Alerts;

public static class MarkAlertReadHandler
{
    public static async Task<IResult> Handle(
        Guid alertId,
        HttpContext ctx,
        CasaLogContext db,
        CancellationToken ct)
    {
        var userId = ctx.GetUserId();
        if (userId == Guid.Empty) return Results.Unauthorized();

        var alert = await db.Alerts
            .Include(a => a.Home)
            .FirstOrDefaultAsync(a => a.Id == alertId, ct);

        if (alert is null || alert.Home.UserId != userId)
            return Results.NotFound(new { error = "Alert not found." });

        alert.IsRead = true;
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}
