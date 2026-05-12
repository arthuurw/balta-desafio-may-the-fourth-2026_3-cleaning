using CasaLog.Api.Features.Alerts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace CasaLog.Api.Tests.Features.Alerts;

public class AlertTests : TestBase
{
    private readonly IHomeAgent _agent = Substitute.For<IHomeAgent>();

    private async Task<(Home home, ScheduledTask task)> SeedDueTaskAsync(string email, int daysFromNow = 7)
    {
        var user = await SeedUserAsync(email);
        var home = await SeedHomeAsync(user.Id);
        var task = new ScheduledTask
        {
            HomeId = home.Id,
            Type = "ac_cleaning",
            ScheduledDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(daysFromNow)),
            Priority = "high",
            Reason = "Test"
        };
        Db.ScheduledTasks.Add(task);
        await Db.SaveChangesAsync();
        return (home, task);
    }

    private EvaluateAlertResponse ShouldAlert() =>
        new("evaluate_alert", true, "upcoming", "Em breve", "Tarefa próxima", "ok");

    private EvaluateAlertResponse NoAlert() =>
        new("evaluate_alert", false, null, "Não necessário", "", "ok");

    // ── GetAlerts ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAlerts_NoHome_Returns404()
    {
        var user = await SeedUserAsync("al1@test.com");
        var ctx = MakeCtx(user.Id);

        var result = await GetAlertsHandler.Handle(ctx, Db, _agent, default);

        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(404, r.StatusCode);
    }

    [Fact]
    public async Task GetAlerts_EvaluatesTasks_CreatesAlert()
    {
        var (home, _) = await SeedDueTaskAsync("al2@test.com");
        var ctx = MakeCtx(home.UserId);
        _agent.EvaluateAlertAsync(Arg.Any<Home>(), Arg.Any<ScheduledTask>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(ShouldAlert());

        var result = await GetAlertsHandler.Handle(ctx, Db, _agent, default);

        var ok = Assert.IsType<Ok<List<GetAlertsHandler.Response>>>(result);
        Assert.Single(ok.Value!);
        Assert.Equal("upcoming", ok.Value![0].Type);
    }

    [Fact]
    public async Task GetAlerts_AgentSaysNoAlert_ReturnsEmpty()
    {
        var (home, _) = await SeedDueTaskAsync("al3@test.com");
        var ctx = MakeCtx(home.UserId);
        _agent.EvaluateAlertAsync(Arg.Any<Home>(), Arg.Any<ScheduledTask>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(NoAlert());

        var result = await GetAlertsHandler.Handle(ctx, Db, _agent, default);

        var ok = Assert.IsType<Ok<List<GetAlertsHandler.Response>>>(result);
        Assert.Empty(ok.Value!);
    }

    [Fact]
    public async Task GetAlerts_SkipsTaskWithExistingUnreadAlert()
    {
        var (home, task) = await SeedDueTaskAsync("al4@test.com");
        Db.Alerts.Add(new Alert { HomeId = home.Id, TaskId = task.Id, Type = "upcoming", Message = "Já existe", IsRead = false });
        await Db.SaveChangesAsync();
        var ctx = MakeCtx(home.UserId);

        await GetAlertsHandler.Handle(ctx, Db, _agent, default);

        await _agent.DidNotReceive().EvaluateAlertAsync(Arg.Any<Home>(), Arg.Any<ScheduledTask>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAlerts_EvaluatesTaskWithReadAlert()
    {
        var (home, task) = await SeedDueTaskAsync("al5@test.com");
        Db.Alerts.Add(new Alert { HomeId = home.Id, TaskId = task.Id, Type = "upcoming", Message = "Lido", IsRead = true });
        await Db.SaveChangesAsync();
        var ctx = MakeCtx(home.UserId);
        _agent.EvaluateAlertAsync(Arg.Any<Home>(), Arg.Any<ScheduledTask>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(ShouldAlert());

        var result = await GetAlertsHandler.Handle(ctx, Db, _agent, default);

        var ok = Assert.IsType<Ok<List<GetAlertsHandler.Response>>>(result);
        Assert.Single(ok.Value!);
    }

    [Fact]
    public async Task GetAlerts_AgentFails_ContinuesToNextTask_ReturnsOk()
    {
        var user = await SeedUserAsync("al6@test.com");
        var home = await SeedHomeAsync(user.Id);
        Db.ScheduledTasks.Add(new ScheduledTask { HomeId = home.Id, Type = "ac_cleaning", ScheduledDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), Priority = "high", Reason = "T1" });
        Db.ScheduledTasks.Add(new ScheduledTask { HomeId = home.Id, Type = "drain_cleaning", ScheduledDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), Priority = "low", Reason = "T2" });
        await Db.SaveChangesAsync();
        var ctx = MakeCtx(user.Id);
        _agent.EvaluateAlertAsync(Arg.Any<Home>(), Arg.Any<ScheduledTask>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new AgentException("LLM down"));

        var result = await GetAlertsHandler.Handle(ctx, Db, _agent, default);

        Assert.IsType<Ok<List<GetAlertsHandler.Response>>>(result);
        Assert.Equal(0, await Db.Alerts.CountAsync());
    }

    [Fact]
    public async Task GetAlerts_Throttle_SkipsEvaluationWithinOneHour()
    {
        var (home, _) = await SeedDueTaskAsync("al7@test.com");
        home.LastAlertCheck = DateTime.UtcNow.AddMinutes(-30);
        await Db.SaveChangesAsync();
        var ctx = MakeCtx(home.UserId);

        await GetAlertsHandler.Handle(ctx, Db, _agent, default);

        await _agent.DidNotReceive().EvaluateAlertAsync(Arg.Any<Home>(), Arg.Any<ScheduledTask>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAlerts_AfterThrottle_EvaluatesAgain()
    {
        var (home, _) = await SeedDueTaskAsync("al8@test.com");
        home.LastAlertCheck = DateTime.UtcNow.AddHours(-2);
        await Db.SaveChangesAsync();
        var ctx = MakeCtx(home.UserId);
        _agent.EvaluateAlertAsync(Arg.Any<Home>(), Arg.Any<ScheduledTask>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(NoAlert());

        await GetAlertsHandler.Handle(ctx, Db, _agent, default);

        await _agent.Received(1).EvaluateAlertAsync(Arg.Any<Home>(), Arg.Any<ScheduledTask>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAlerts_IgnoresTasksBeyond30Days()
    {
        var user = await SeedUserAsync("al9@test.com");
        var home = await SeedHomeAsync(user.Id);
        Db.ScheduledTasks.Add(new ScheduledTask { HomeId = home.Id, Type = "ac_cleaning", ScheduledDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60)), Priority = "high", Reason = "Far away" });
        await Db.SaveChangesAsync();
        var ctx = MakeCtx(user.Id);

        await GetAlertsHandler.Handle(ctx, Db, _agent, default);

        await _agent.DidNotReceive().EvaluateAlertAsync(Arg.Any<Home>(), Arg.Any<ScheduledTask>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAlerts_ReturnsOnlyUnread()
    {
        var (home, task) = await SeedDueTaskAsync("al10@test.com");
        Db.Alerts.Add(new Alert { HomeId = home.Id, TaskId = task.Id, Type = "upcoming", Message = "Lido", IsRead = true });
        Db.Alerts.Add(new Alert { HomeId = home.Id, TaskId = task.Id, Type = "overdue", Message = "Não lido", IsRead = false });
        await Db.SaveChangesAsync();
        home.LastAlertCheck = DateTime.UtcNow;
        await Db.SaveChangesAsync();
        var ctx = MakeCtx(home.UserId);

        var result = await GetAlertsHandler.Handle(ctx, Db, _agent, default);

        var ok = Assert.IsType<Ok<List<GetAlertsHandler.Response>>>(result);
        Assert.Single(ok.Value!);
        Assert.Equal("overdue", ok.Value![0].Type);
    }

    [Fact]
    public async Task GetAlerts_UpdatesLastAlertCheck()
    {
        var (home, _) = await SeedDueTaskAsync("al11@test.com");
        var ctx = MakeCtx(home.UserId);
        _agent.EvaluateAlertAsync(Arg.Any<Home>(), Arg.Any<ScheduledTask>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(NoAlert());

        await GetAlertsHandler.Handle(ctx, Db, _agent, default);

        var updated = await Db.Homes.FindAsync(home.Id);
        Assert.NotNull(updated!.LastAlertCheck);
        Assert.True(updated.LastAlertCheck > DateTime.UtcNow.AddSeconds(-10));
    }

    // ── MarkAlertRead ────────────────────────────────────────────────────────

    [Fact]
    public async Task MarkAlertRead_Valid_Returns204()
    {
        var (home, task) = await SeedDueTaskAsync("ma1@test.com");
        var alert = new Alert { HomeId = home.Id, TaskId = task.Id, Type = "upcoming", Message = "Test" };
        Db.Alerts.Add(alert);
        await Db.SaveChangesAsync();
        var ctx = MakeCtx(home.UserId);

        var result = await MarkAlertReadHandler.Handle(alert.Id, ctx, Db, default);

        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(204, r.StatusCode);
    }

    [Fact]
    public async Task MarkAlertRead_SetsIsReadTrue()
    {
        var (home, task) = await SeedDueTaskAsync("ma2@test.com");
        var alert = new Alert { HomeId = home.Id, TaskId = task.Id, Type = "upcoming", Message = "Test" };
        Db.Alerts.Add(alert);
        await Db.SaveChangesAsync();
        var ctx = MakeCtx(home.UserId);

        await MarkAlertReadHandler.Handle(alert.Id, ctx, Db, default);

        var updated = await Db.Alerts.FindAsync(alert.Id);
        Assert.True(updated!.IsRead);
    }

    [Fact]
    public async Task MarkAlertRead_OtherUsersAlert_Returns404()
    {
        var (home, task) = await SeedDueTaskAsync("ma3a@test.com");
        var other = await SeedUserAsync("ma3b@test.com");
        var alert = new Alert { HomeId = home.Id, TaskId = task.Id, Type = "upcoming", Message = "Test" };
        Db.Alerts.Add(alert);
        await Db.SaveChangesAsync();
        var ctx = MakeCtx(other.Id);

        var result = await MarkAlertReadHandler.Handle(alert.Id, ctx, Db, default);

        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(404, r.StatusCode);
    }

    [Fact]
    public async Task MarkAlertRead_NotFound_Returns404()
    {
        var user = await SeedUserAsync("ma4@test.com");
        var ctx = MakeCtx(user.Id);

        var result = await MarkAlertReadHandler.Handle(Guid.NewGuid(), ctx, Db, default);

        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(404, r.StatusCode);
    }
}
