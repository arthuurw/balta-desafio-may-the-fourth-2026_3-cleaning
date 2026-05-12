using CasaLog.Api.Features.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CasaLog.Api.Tests.Features.Auth;

public class LoginTests : TestBase
{
    private readonly IConfiguration _config = MakeCfg();

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        await SeedUserAsync("login@test.com");
        var req = new LoginHandler.Request("login@test.com", "password123");
        var result = await LoginHandler.Handle(req, Db, _config, default);
        var ok = Assert.IsType<Ok<LoginHandler.Response>>(result);
        Assert.False(string.IsNullOrEmpty(ok.Value!.Token));
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        await SeedUserAsync("user2@test.com");
        var req = new LoginHandler.Request("user2@test.com", "wrongpassword");
        var result = await LoginHandler.Handle(req, Db, _config, default);
        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(401, r.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsUnauthorized()
    {
        var req = new LoginHandler.Request("nobody@test.com", "password123");
        var result = await LoginHandler.Handle(req, Db, _config, default);
        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(401, r.StatusCode);
    }

    [Fact]
    public async Task Login_CaseInsensitiveEmail_Works()
    {
        await SeedUserAsync("cased@test.com");
        var req = new LoginHandler.Request("CASED@TEST.COM", "password123");
        var result = await LoginHandler.Handle(req, Db, _config, default);
        Assert.IsType<Ok<LoginHandler.Response>>(result);
    }

    [Fact]
    public async Task Login_EmptyEmail_Returns400()
    {
        var req = new LoginHandler.Request("", "password123");
        var result = await LoginHandler.Handle(req, Db, _config, default);
        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(400, r.StatusCode);
    }
}
