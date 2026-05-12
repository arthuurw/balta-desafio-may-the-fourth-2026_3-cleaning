namespace CasaLog.Api.Features.Equipments;

public static class DeleteEquipmentHandler
{
    public static async Task<IResult> Handle(
        Guid homeId,
        Guid equipmentId,
        HttpContext ctx,
        CasaLogContext db,
        CancellationToken ct)
    {
        var userId = ctx.GetUserId();
        if (userId == Guid.Empty) return Results.Unauthorized();

        var home = await db.Homes.FirstOrDefaultAsync(h => h.Id == homeId && h.UserId == userId, ct);
        if (home is null) return Results.NotFound(new { error = "Home not found." });

        var equipment = await db.Equipment.FirstOrDefaultAsync(e => e.Id == equipmentId && e.HomeId == homeId, ct);
        if (equipment is null) return Results.NotFound(new { error = "Equipment not found." });

        // Nullify equipment reference on pending tasks (cascade via SetNull FK)
        var pendingTasks = await db.ScheduledTasks
            .Where(t => t.EquipmentId == equipmentId && t.Status == "pending")
            .ToListAsync(ct);

        // Delete pending tasks linked to this equipment
        db.ScheduledTasks.RemoveRange(pendingTasks);
        db.Equipment.Remove(equipment);
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}
