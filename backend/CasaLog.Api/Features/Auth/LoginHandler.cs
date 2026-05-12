namespace CasaLog.Api.Features.Auth;

public static class LoginHandler
{
    public record Request(string Email, string Password);
    public record Response(Guid Id, string Name, string Email, string Token);

    public static async Task<IResult> Handle(Request req, CasaLogContext db, IConfiguration config, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return Results.BadRequest(new { error = "Email and password are required." });

        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLower().Trim(), ct);

        if (user is null || !PasswordHasher.Verify(req.Password, user.PasswordHash))
            return Results.Unauthorized();

        var token = JwtTokenGenerator.Generate(user, config);
        return Results.Ok(new Response(user.Id, user.Name, user.Email, token));
    }
}
