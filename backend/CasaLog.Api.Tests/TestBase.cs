using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using CasaLog.Api.Data;
using CasaLog.Api.Data.Entities;
using CasaLog.Api.Infrastructure.Security;

namespace CasaLog.Api.Tests;

public abstract class TestBase : IAsyncDisposable
{
    private readonly SqliteConnection _conn;
    protected readonly CasaLogContext Db;

    protected TestBase()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var opts = new DbContextOptionsBuilder<CasaLogContext>()
            .UseSqlite(_conn)
            .Options;
        Db = new CasaLogContext(opts);
        Db.Database.EnsureCreated();
    }

    protected static HttpContext MakeCtx(Guid userId)
    {
        var claims = new[] { new Claim("sub", userId.ToString()) };
        return new DefaultHttpContext { User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(claims, "Test")) };
    }

    protected static IConfiguration MakeCfg(Dictionary<string, string?>? extra = null)
    {
        var dict = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "casalog-test-secret-key-32chars!!",
            ["Jwt:Issuer"] = "CasaLog",
            ["Jwt:Audience"] = "CasaLog",
            ["Jwt:ExpiryDays"] = "7"
        };
        if (extra is not null) foreach (var kv in extra) dict[kv.Key] = kv.Value;
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    protected async Task<User> SeedUserAsync(string email = "user@test.com", string name = "Test User")
    {
        var user = new User { Name = name, Email = email, PasswordHash = PasswordHasher.Hash("password123") };
        Db.Users.Add(user);
        await Db.SaveChangesAsync();
        return user;
    }

    protected async Task<Home> SeedHomeAsync(Guid userId, string type = "apartment", bool hasGarden = false)
    {
        var home = new Home { UserId = userId, Type = type, HasGarden = hasGarden };
        Db.Homes.Add(home);
        await Db.SaveChangesAsync();
        return home;
    }

    public async ValueTask DisposeAsync()
    {
        await Db.DisposeAsync();
        await _conn.DisposeAsync();
    }
}
