using CasaLog.Api.Features.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CasaLog.Api.Tests.Features.Auth;

public class RegisterTests : TestBase
{
    private readonly IConfiguration _config = MakeCfg();

    [Fact]
    public async Task Register_ValidInput_Returns201()
    {
        var req = new RegisterHandler.Request("Alice", "alice@test.com", "password123");
        var result = await RegisterHandler.Handle(req, Db, _config, default);
        var created = Assert.IsType<Created<RegisterHandler.Response>>(result);
        Assert.Equal("alice@test.com", created.Value!.Email);
        Assert.False(string.IsNullOrEmpty(created.Value.Token));
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        await SeedUserAsync("dup@test.com");
        var req = new RegisterHandler.Request("Bob", "dup@test.com", "password123");
        var result = await RegisterHandler.Handle(req, Db, _config, default);
        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(409, r.StatusCode);
    }

    [Fact]
    public async Task Register_ShortPassword_Returns400()
    {
        var req = new RegisterHandler.Request("Carol", "carol@test.com", "abc");
        var result = await RegisterHandler.Handle(req, Db, _config, default);
        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(400, r.StatusCode);
    }

    [Fact]
    public async Task Register_EmptyName_Returns400()
    {
        var req = new RegisterHandler.Request("", "carol@test.com", "password123");
        var result = await RegisterHandler.Handle(req, Db, _config, default);
        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(400, r.StatusCode);
    }

    [Fact]
    public async Task Register_EmailNormalized_StoredLowercase()
    {
        var req = new RegisterHandler.Request("Dave", "Dave@Test.COM", "password123");
        await RegisterHandler.Handle(req, Db, _config, default);
        var user = await Db.Users.FirstOrDefaultAsync(u => u.Email == "dave@test.com");
        Assert.NotNull(user);
    }

    [Fact]
    public async Task Register_PasswordNotStoredPlaintext()
    {
        var req = new RegisterHandler.Request("Eve", "eve@test.com", "supersecret");
        await RegisterHandler.Handle(req, Db, _config, default);
        var user = await Db.Users.FirstAsync(u => u.Email == "eve@test.com");
        Assert.NotEqual("supersecret", user.PasswordHash);
        Assert.Contains(".", user.PasswordHash);
    }

    [Fact]
    public async Task Register_TokenIsJwt()
    {
        var req = new RegisterHandler.Request("Frank", "frank@test.com", "password123");
        var result = await RegisterHandler.Handle(req, Db, _config, default);
        var created = Assert.IsType<Created<RegisterHandler.Response>>(result);
        Assert.Equal(3, created.Value!.Token.Split('.').Length);
    }

    [Fact]
    public async Task Register_EmptyEmail_Returns400()
    {
        var req = new RegisterHandler.Request("Grace", "", "password123");
        var result = await RegisterHandler.Handle(req, Db, _config, default);
        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(400, r.StatusCode);
    }
}
