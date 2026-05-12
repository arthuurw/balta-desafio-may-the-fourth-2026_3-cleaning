using CasaLog.Api.Features.Homes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;

namespace CasaLog.Api.Tests.Features.Homes;

public class HomeTests : TestBase
{
    private readonly IHomeAgent _agent = Substitute.For<IHomeAgent>();

    [Fact]
    public async Task CreateHome_ValidInput_Returns201()
    {
        var user = await SeedUserAsync();
        var ctx = MakeCtx(user.Id);
        _agent.GenerateScheduleAsync(Arg.Any<Home>(), Arg.Any<CancellationToken>())
            .Returns(new GenerateScheduleResponse("generate_schedule", "ok", []));

        var result = await CreateHomeHandler.Handle(new CreateHomeHandler.Request("apartment", false), ctx, Db, _agent, default);

        Assert.IsType<Created<CreateHomeHandler.Response>>(result);
    }

    [Fact]
    public async Task CreateHome_InvalidType_Returns400()
    {
        var user = await SeedUserAsync("b@test.com");
        var ctx = MakeCtx(user.Id);

        var result = await CreateHomeHandler.Handle(new CreateHomeHandler.Request("condo", false), ctx, Db, _agent, default);

        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(400, r.StatusCode);
    }

    [Fact]
    public async Task CreateHome_DuplicateHome_Returns409()
    {
        var user = await SeedUserAsync("c@test.com");
        await SeedHomeAsync(user.Id);
        var ctx = MakeCtx(user.Id);

        var result = await CreateHomeHandler.Handle(new CreateHomeHandler.Request("house", true), ctx, Db, _agent, default);

        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(409, r.StatusCode);
    }

    [Fact]
    public async Task CreateHome_AgentFailure_StillCreatesHome()
    {
        var user = await SeedUserAsync("d@test.com");
        var ctx = MakeCtx(user.Id);
        _agent.GenerateScheduleAsync(Arg.Any<Home>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<GenerateScheduleResponse>(new AgentException("LLM unavailable")));

        var result = await CreateHomeHandler.Handle(new CreateHomeHandler.Request("house", true), ctx, Db, _agent, default);

        Assert.IsType<Created<CreateHomeHandler.Response>>(result);
    }

    [Fact]
    public async Task GetHome_NoHome_Returns404()
    {
        var user = await SeedUserAsync("e@test.com");
        var ctx = MakeCtx(user.Id);

        var result = await GetHomeHandler.Handle(ctx, Db, default);

        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(404, r.StatusCode);
    }

    [Fact]
    public async Task GetHome_WithHome_ReturnsOk()
    {
        var user = await SeedUserAsync("f@test.com");
        await SeedHomeAsync(user.Id, "house", true);
        var ctx = MakeCtx(user.Id);

        var result = await GetHomeHandler.Handle(ctx, Db, default);

        var ok = Assert.IsType<Ok<GetHomeHandler.Response>>(result);
        Assert.Equal("house", ok.Value!.Type);
        Assert.True(ok.Value.HasGarden);
    }

    [Fact]
    public async Task CreateHome_SavesToDatabase()
    {
        var user = await SeedUserAsync("g@test.com");
        var ctx = MakeCtx(user.Id);
        _agent.GenerateScheduleAsync(Arg.Any<Home>(), Arg.Any<CancellationToken>())
            .Returns(new GenerateScheduleResponse("generate_schedule", "ok", []));

        await CreateHomeHandler.Handle(new CreateHomeHandler.Request("house", true), ctx, Db, _agent, default);

        var home = await Db.Homes.FirstOrDefaultAsync(h => h.UserId == user.Id);
        Assert.NotNull(home);
        Assert.Equal("house", home.Type);
        Assert.True(home.HasGarden);
    }

    [Fact]
    public async Task CreateHome_WithSchedule_SavesTasks()
    {
        var user = await SeedUserAsync("h@test.com");
        var ctx = MakeCtx(user.Id);
        var tasks = new[]
        {
            new AgentTaskItem("water_filter_change", "2026-08-01", "medium", "Troca semestral", null),
            new AgentTaskItem("ac_cleaning", "2026-09-15", "high", "Limpeza pré-verão", "ac_cleaning")
        };
        _agent.GenerateScheduleAsync(Arg.Any<Home>(), Arg.Any<CancellationToken>())
            .Returns(new GenerateScheduleResponse("generate_schedule", "ok", tasks));

        await CreateHomeHandler.Handle(new CreateHomeHandler.Request("apartment", false), ctx, Db, _agent, default);

        var count = await Db.ScheduledTasks.CountAsync();
        Assert.Equal(2, count);
    }
}
