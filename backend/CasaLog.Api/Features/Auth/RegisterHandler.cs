namespace CasaLog.Api.Features.Auth;

public static class RegisterHandler
{
    public record Request(string Name, string Email, string Password);
    public record Response(Guid Id, string Name, string Email, string Token);

    public static async Task<IResult> Handle(Request req, CasaLogContext db, IConfiguration config, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return Results.BadRequest(new { error = "Name, email and password are required." });

        if (req.Password.Length < 6)
            return Results.BadRequest(new { error = "Password must be at least 6 characters." });

        if (await db.Users.AnyAsync(u => u.Email == req.Email.ToLower(), ct))
            return Results.Conflict(new { error = "Email already in use." });

        var user = new User
        {
            Name = req.Name.Trim(),
            Email = req.Email.ToLower().Trim(),
            PasswordHash = PasswordHasher.Hash(req.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        var token = JwtTokenGenerator.Generate(user, config);
        return Results.Created($"/api/users/{user.Id}", new Response(user.Id, user.Name, user.Email, token));
    }
}
