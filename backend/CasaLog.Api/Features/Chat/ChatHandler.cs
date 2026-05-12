namespace CasaLog.Api.Features.Chat;

public static class ChatHandler
{
    public record Request(string Message);
    public record TipDto(string Type, string Tip);
    public record Response(string Action, string Reply, List<TipDto>? Tips);

    private static readonly string[] InjectionPatterns = ["system:", "assistant:", "[INST]", "<s>"];

    public static async Task<IResult> Handle(
        Request req,
        HttpContext ctx,
        CasaLogContext db,
        IHomeAgent agent,
        CancellationToken ct)
    {
        var userId = ctx.GetUserId();
        if (userId == Guid.Empty) return Results.Unauthorized();

        if (string.IsNullOrWhiteSpace(req.Message))
            return Results.BadRequest(new { error = "Message is required." });

        if (InjectionPatterns.Any(p => req.Message.Contains(p, StringComparison.OrdinalIgnoreCase)))
            return Results.Ok(new Response("unknown", "Posso ajudar apenas com manutenção residencial.", null));

        var home = await db.Homes
            .Include(h => h.Equipment)
            .Include(h => h.ScheduledTasks)
            .FirstOrDefaultAsync(h => h.UserId == userId, ct);

        if (home is null) return Results.NotFound(new { error = "Home not found. Register your home first." });

        try
        {
            var result = await agent.ChatAsync(home, req.Message, ct);
            var tips = result.Tips?.Select(t => new TipDto(t.Type, t.Tip)).ToList();
            return Results.Ok(new Response(result.Action, result.Reply, tips));
        }
        catch (AgentException ex)
        {
            return Results.Problem(ex.Message, statusCode: 502);
        }
    }
}
