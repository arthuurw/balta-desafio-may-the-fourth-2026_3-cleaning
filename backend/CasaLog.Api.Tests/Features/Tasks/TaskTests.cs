using CasaLog.Api.Data.Entities;
using CasaLog.Api.Features.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;

namespace CasaLog.Api.Tests.Features.Tasks;

public class TaskTests : TestBase
{
    private readonly IHomeAgent _agent = Substitute.For<IHomeAgent>();

    private async Task<(Home home, ScheduledTask task)> SeedTaskAsync(string email)
    {
        var user = await SeedUserAsync(email);
        var home = await SeedHomeAsync(user.Id);
        var task = new ScheduledTask
        {
            HomeId = home.Id,
            Type = "water_filter_change",
            ScheduledDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            Priority = "medium",
            Reason = "Troca semestral"
        };
        Db.ScheduledTasks.Add(task);
        await Db.SaveChangesAsync();
        return (home, task);
    }

    [Fact]
    public async Task GetTasks_NoHome_Returns404()
    {
        var user = await SeedUserAsync();
        var ctx = MakeCtx(user.Id);

        var result = await GetTasksHandler.Handle(Guid.NewGuid(), null, null, ctx, Db, default);

        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(404, r.StatusCode);
    }

    [Fact]
    public async Task GetTasks_WithTasks_ReturnsList()
    {
        var user = await SeedUserAsync("t2@test.com");
        var home = await SeedHomeAsync(user.Id);
        Db.ScheduledTasks.Add(new ScheduledTask { HomeId = home.Id, Type = "ac_cleaning", ScheduledDate = DateOnly.Parse("2026-09-15"), Priority = "high", Reason = "Test" });
        await Db.SaveChangesAsync();
        var ctx = MakeCtx(user.Id);

        var result = await GetTasksHandler.Handle(home.Id, null, null, ctx, Db, default);
        var ok = Assert.IsType<Ok<List<GetTasksHandler.Response>>>(result);
        Assert.Single(ok.Value!);
    }

    [Fact]
    public async Task GetTasks_FilterByStatus_ReturnsFiltered()
    {
        var user = await SeedUserAsync("t3@test.com");
        var home = await SeedHomeAsync(user.Id);
        Db.ScheduledTasks.Add(new ScheduledTask { HomeId = home.Id, Type = "ac_cleaning", ScheduledDate = DateOnly.Parse("2026-09-15"), Priority = "high", Reason = "Test", Status = "pending" });
        Db.ScheduledTasks.Add(new ScheduledTask { HomeId = home.Id, Type = "drain_cleaning", ScheduledDate = DateOnly.Parse("2026-06-01"), Priority = "low", Reason = "Test", Status = "completed" });
        await Db.SaveChangesAsync();
        var ctx = MakeCtx(user.Id);

        var result = await GetTasksHandler.Handle(home.Id, "pending", null, ctx, Db, default);
        var ok = Assert.IsType<Ok<List<GetTasksHandler.Response>>>(result);
        Assert.Single(ok.Value!);
        Assert.Equal("pending", ok.Value![0].Status);
    }

    [Fact]
    public async Task CompleteTask_ValidTask_ReturnsOk()
    {
        var (home, task) = await SeedTaskAsync("t4@test.com");
        var user = await Db.Users.FindAsync(home.UserId);
        var ctx = MakeCtx(user!.Id);
        _agent.RescheduleTaskAsync(Arg.Any<Home>(), Arg.Any<ScheduledTask>(), false, Arg.Any<CancellationToken>())
            .Returns(new RescheduleTaskResponse("reschedule_task", "2026-11-10", "Próxima manutenção", "ok"));

        var result = await CompleteTaskHandler.Handle(home.Id, task.Id, new CompleteTaskHandler.Request(null), ctx, Db, _agent, default);
        Assert.IsType<Ok<CompleteTaskHandler.Response>>(result);
    }

    [Fact]
    public async Task CompleteTask_AlreadyCompleted_Returns400()
    {
        var (home, task) = await SeedTaskAsync("t5@test.com");
        task.Status = "completed";
        await Db.SaveChangesAsync();
        var user = await Db.Users.FindAsync(home.UserId);
        var ctx = MakeCtx(user!.Id);

        var result = await CompleteTaskHandler.Handle(home.Id, task.Id, new CompleteTaskHandler.Request(null), ctx, Db, _agent, default);
        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(400, r.StatusCode);
    }

    [Fact]
    public async Task SkipTask_ValidTask_ReturnsOk()
    {
        var (home, task) = await SeedTaskAsync("t6@test.com");
        var user = await Db.Users.FindAsync(home.UserId);
        var ctx = MakeCtx(user!.Id);
        _agent.RescheduleTaskAsync(Arg.Any<Home>(), Arg.Any<ScheduledTask>(), true, Arg.Any<CancellationToken>())
            .Returns(new RescheduleTaskResponse("reschedule_task", "2026-11-10", "Reagendado", "ok"));

        var result = await SkipTaskHandler.Handle(home.Id, task.Id, ctx, Db, _agent, default);
        Assert.IsType<Ok<SkipTaskHandler.Response>>(result);
    }

    [Fact]
    public async Task SkipTask_SetsStatusToSkipped()
    {
        var (home, task) = await SeedTaskAsync("t7@test.com");
        var user = await Db.Users.FindAsync(home.UserId);
        var ctx = MakeCtx(user!.Id);
        _agent.RescheduleTaskAsync(Arg.Any<Home>(), Arg.Any<ScheduledTask>(), true, Arg.Any<CancellationToken>())
            .Returns(new RescheduleTaskResponse("reschedule_task", "2026-11-10", "Reagendado", "ok"));

        await SkipTaskHandler.Handle(home.Id, task.Id, ctx, Db, _agent, default);

        var updated = await Db.ScheduledTasks.FindAsync(task.Id);
        Assert.Equal("skipped", updated!.Status);
    }

    [Fact]
    public async Task CompleteTask_CreatesRescheduledTask()
    {
        var (home, task) = await SeedTaskAsync("t8@test.com");
        var user = await Db.Users.FindAsync(home.UserId);
        var ctx = MakeCtx(user!.Id);
        _agent.RescheduleTaskAsync(Arg.Any<Home>(), Arg.Any<ScheduledTask>(), false, Arg.Any<CancellationToken>())
            .Returns(new RescheduleTaskResponse("reschedule_task", "2026-11-10", "Próxima manutenção", "ok"));

        await CompleteTaskHandler.Handle(home.Id, task.Id, new CompleteTaskHandler.Request("Done!"), ctx, Db, _agent, default);

        var all = await Db.ScheduledTasks.Where(t => t.HomeId == home.Id).ToListAsync();
        Assert.Equal(2, all.Count);
        Assert.Single(all, t => t.Status == "completed");
        Assert.Single(all, t => t.Status == "pending");
    }

    [Fact]
    public async Task CompleteTask_WrongHomeId_Returns404()
    {
        var (home, task) = await SeedTaskAsync("t9@test.com");
        var user = await Db.Users.FindAsync(home.UserId);
        var ctx = MakeCtx(user!.Id);

        var result = await CompleteTaskHandler.Handle(Guid.NewGuid(), task.Id, new CompleteTaskHandler.Request(null), ctx, Db, _agent, default);
        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(404, r.StatusCode);
    }

    [Fact]
    public async Task GetTasks_OtherUsersTask_Returns404()
    {
        var (home, _) = await SeedTaskAsync("t10@test.com");
        var otherUser = await SeedUserAsync("t10b@test.com");
        var ctx = MakeCtx(otherUser.Id);

        var result = await GetTasksHandler.Handle(home.Id, null, null, ctx, Db, default);
        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(404, r.StatusCode);
    }
}
