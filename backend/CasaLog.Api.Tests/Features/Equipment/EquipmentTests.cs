using CasaLog.Api.Features.Equipments;
using CasaLog.Api.Features.Homes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace CasaLog.Api.Tests.Features.Equipment;

public class EquipmentTests : TestBase
{
    private readonly IHomeAgent _agent = Substitute.For<IHomeAgent>();

    private NormalizeEquipmentResponse ValidNorm(string type = "ac") =>
        new("normalize_equipment", true, type, "ok", "ok");

    private GenerateScheduleResponse EmptySchedule() =>
        new("generate_schedule", "ok", []);

    private async Task<Home> SeedHomeWithEquipmentAsync(Guid userId)
    {
        var home = await SeedHomeAsync(userId);
        var eq = new Data.Entities.Equipment { HomeId = home.Id, Type = "ac", Name = "Ar Condicionado" };
        Db.Equipment.Add(eq);
        await Db.SaveChangesAsync();
        home = await Db.Homes.Include(h => h.Equipment).FirstAsync(h => h.Id == home.Id);
        return home;
    }

    // ── AddEquipment ────────────────────────────────────────────────────────

    [Fact]
    public async Task AddEquipment_Valid_Returns201()
    {
        var user = await SeedUserAsync("eq1@test.com");
        var home = await SeedHomeAsync(user.Id);
        var ctx = MakeCtx(user.Id);
        _agent.NormalizeEquipmentTypeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(ValidNorm("ac"));
        _agent.GenerateScheduleAsync(Arg.Any<Home>(), Arg.Any<CancellationToken>())
            .Returns(EmptySchedule());

        var result = await AddEquipmentHandler.Handle(home.Id, new AddEquipmentHandler.Request("AC", "Split 12k", null), ctx, Db, _agent, default);

        Assert.IsType<Created<AddEquipmentHandler.Response>>(result);
    }

    [Fact]
    public async Task AddEquipment_SavesNormalizedType()
    {
        var user = await SeedUserAsync("eq2@test.com");
        var home = await SeedHomeAsync(user.Id);
        var ctx = MakeCtx(user.Id);
        _agent.NormalizeEquipmentTypeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(ValidNorm("water_heater"));
        _agent.GenerateScheduleAsync(Arg.Any<Home>(), Arg.Any<CancellationToken>())
            .Returns(EmptySchedule());

        await AddEquipmentHandler.Handle(home.Id, new AddEquipmentHandler.Request("Boiler", "Aquecedor", null), ctx, Db, _agent, default);

        var eq = await Db.Equipment.FirstAsync();
        Assert.Equal("water_heater", eq.Type);
    }

    [Fact]
    public async Task AddEquipment_EmptyType_Returns400()
    {
        var user = await SeedUserAsync("eq3@test.com");
        var home = await SeedHomeAsync(user.Id);
        var ctx = MakeCtx(user.Id);

        var result = await AddEquipmentHandler.Handle(home.Id, new AddEquipmentHandler.Request("", "Nome", null), ctx, Db, _agent, default);

        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(400, r.StatusCode);
    }

    [Fact]
    public async Task AddEquipment_EmptyName_Returns400()
    {
        var user = await SeedUserAsync("eq4@test.com");
        var home = await SeedHomeAsync(user.Id);
        var ctx = MakeCtx(user.Id);

        var result = await AddEquipmentHandler.Handle(home.Id, new AddEquipmentHandler.Request("ac", "", null), ctx, Db, _agent, default);

        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(400, r.StatusCode);
    }

    [Fact]
    public async Task AddEquipment_HomeNotFound_Returns404()
    {
        var user = await SeedUserAsync("eq5@test.com");
        var ctx = MakeCtx(user.Id);

        var result = await AddEquipmentHandler.Handle(Guid.NewGuid(), new AddEquipmentHandler.Request("ac", "Split", null), ctx, Db, _agent, default);

        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(404, r.StatusCode);
    }

    [Fact]
    public async Task AddEquipment_OtherUsersHome_Returns404()
    {
        var owner = await SeedUserAsync("eq6a@test.com");
        var other = await SeedUserAsync("eq6b@test.com");
        var home = await SeedHomeAsync(owner.Id);
        var ctx = MakeCtx(other.Id);

        var result = await AddEquipmentHandler.Handle(home.Id, new AddEquipmentHandler.Request("ac", "Split", null), ctx, Db, _agent, default);

        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(404, r.StatusCode);
    }

    [Fact]
    public async Task AddEquipment_AgentRejectsType_Returns400()
    {
        var user = await SeedUserAsync("eq7@test.com");
        var home = await SeedHomeAsync(user.Id);
        var ctx = MakeCtx(user.Id);
        _agent.NormalizeEquipmentTypeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new NormalizeEquipmentResponse("normalize_equipment", false, null, "Tipo inválido", "ok"));

        var result = await AddEquipmentHandler.Handle(home.Id, new AddEquipmentHandler.Request("xyz", "Coisa", null), ctx, Db, _agent, default);

        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(400, r.StatusCode);
    }

    [Fact]
    public async Task AddEquipment_NormalizeAgentFails_Returns502()
    {
        var user = await SeedUserAsync("eq8@test.com");
        var home = await SeedHomeAsync(user.Id);
        var ctx = MakeCtx(user.Id);
        _agent.NormalizeEquipmentTypeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new AgentException("LLM down"));

        var result = await AddEquipmentHandler.Handle(home.Id, new AddEquipmentHandler.Request("ac", "Split", null), ctx, Db, _agent, default);

        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(502, r.StatusCode);
    }

    [Fact]
    public async Task AddEquipment_ScheduleAgentFails_StillCreatesEquipment()
    {
        var user = await SeedUserAsync("eq9@test.com");
        var home = await SeedHomeAsync(user.Id);
        var ctx = MakeCtx(user.Id);
        _agent.NormalizeEquipmentTypeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(ValidNorm("ac"));
        _agent.GenerateScheduleAsync(Arg.Any<Home>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new AgentException("LLM down"));

        var result = await AddEquipmentHandler.Handle(home.Id, new AddEquipmentHandler.Request("ac", "Split", null), ctx, Db, _agent, default);

        Assert.IsType<Created<AddEquipmentHandler.Response>>(result);
        Assert.Equal(1, await Db.Equipment.CountAsync());
        Assert.Equal(0, await Db.ScheduledTasks.CountAsync());
    }

    [Fact]
    public async Task AddEquipment_WithValidInstalledAt_SavesDate()
    {
        var user = await SeedUserAsync("eq10@test.com");
        var home = await SeedHomeAsync(user.Id);
        var ctx = MakeCtx(user.Id);
        _agent.NormalizeEquipmentTypeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(ValidNorm("ac"));
        _agent.GenerateScheduleAsync(Arg.Any<Home>(), Arg.Any<CancellationToken>())
            .Returns(EmptySchedule());

        await AddEquipmentHandler.Handle(home.Id, new AddEquipmentHandler.Request("ac", "Split", "2023-06-01"), ctx, Db, _agent, default);

        var eq = await Db.Equipment.FirstAsync();
        Assert.Equal(DateOnly.Parse("2023-06-01"), eq.InstalledAt);
    }

    [Fact]
    public async Task AddEquipment_WithInvalidInstalledAt_SavesNull()
    {
        var user = await SeedUserAsync("eq11@test.com");
        var home = await SeedHomeAsync(user.Id);
        var ctx = MakeCtx(user.Id);
        _agent.NormalizeEquipmentTypeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(ValidNorm("ac"));
        _agent.GenerateScheduleAsync(Arg.Any<Home>(), Arg.Any<CancellationToken>())
            .Returns(EmptySchedule());

        await AddEquipmentHandler.Handle(home.Id, new AddEquipmentHandler.Request("ac", "Split", "not-a-date"), ctx, Db, _agent, default);

        var eq = await Db.Equipment.FirstAsync();
        Assert.Null(eq.InstalledAt);
    }

    [Fact]
    public async Task AddEquipment_WithSchedule_CreatesTasks()
    {
        var user = await SeedUserAsync("eq12@test.com");
        var home = await SeedHomeAsync(user.Id);
        var ctx = MakeCtx(user.Id);
        _agent.NormalizeEquipmentTypeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(ValidNorm("ac"));
        _agent.GenerateScheduleAsync(Arg.Any<Home>(), Arg.Any<CancellationToken>())
            .Returns(new GenerateScheduleResponse("generate_schedule", "ok", [
                new AgentTaskItem("ac_cleaning", "2026-09-01", "high", "Limpeza AC", "ac")
            ]));

        var result = await AddEquipmentHandler.Handle(home.Id, new AddEquipmentHandler.Request("ac", "Split", null), ctx, Db, _agent, default);

        var created = Assert.IsType<Created<AddEquipmentHandler.Response>>(result);
        Assert.Single(created.Value!.Tasks);
        Assert.Equal(1, await Db.ScheduledTasks.CountAsync());
    }

    // ── DeleteEquipment ─────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteEquipment_Valid_Returns204()
    {
        var user = await SeedUserAsync("del1@test.com");
        var home = await SeedHomeAsync(user.Id);
        var eq = new Data.Entities.Equipment { HomeId = home.Id, Type = "ac", Name = "Split" };
        Db.Equipment.Add(eq);
        await Db.SaveChangesAsync();
        var ctx = MakeCtx(user.Id);

        var result = await DeleteEquipmentHandler.Handle(home.Id, eq.Id, ctx, Db, default);

        Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(204, ((IStatusCodeHttpResult)result).StatusCode);
    }

    [Fact]
    public async Task DeleteEquipment_CascadesPendingTasks()
    {
        var user = await SeedUserAsync("del2@test.com");
        var home = await SeedHomeAsync(user.Id);
        var eq = new Data.Entities.Equipment { HomeId = home.Id, Type = "ac", Name = "Split" };
        Db.Equipment.Add(eq);
        await Db.SaveChangesAsync();
        Db.ScheduledTasks.Add(new ScheduledTask { HomeId = home.Id, EquipmentId = eq.Id, Type = "ac_cleaning", ScheduledDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), Priority = "high", Reason = "Test", Status = "pending" });
        await Db.SaveChangesAsync();
        var ctx = MakeCtx(user.Id);

        await DeleteEquipmentHandler.Handle(home.Id, eq.Id, ctx, Db, default);

        Assert.Equal(0, await Db.ScheduledTasks.CountAsync());
        Assert.Equal(0, await Db.Equipment.CountAsync());
    }

    [Fact]
    public async Task DeleteEquipment_DoesNotDeleteCompletedTasks()
    {
        var user = await SeedUserAsync("del3@test.com");
        var home = await SeedHomeAsync(user.Id);
        var eq = new Data.Entities.Equipment { HomeId = home.Id, Type = "ac", Name = "Split" };
        Db.Equipment.Add(eq);
        await Db.SaveChangesAsync();
        Db.ScheduledTasks.Add(new ScheduledTask { HomeId = home.Id, EquipmentId = eq.Id, Type = "ac_cleaning", ScheduledDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)), Priority = "high", Reason = "Test", Status = "completed" });
        Db.ScheduledTasks.Add(new ScheduledTask { HomeId = home.Id, EquipmentId = eq.Id, Type = "ac_cleaning", ScheduledDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), Priority = "high", Reason = "Test", Status = "pending" });
        await Db.SaveChangesAsync();
        var ctx = MakeCtx(user.Id);

        await DeleteEquipmentHandler.Handle(home.Id, eq.Id, ctx, Db, default);

        Assert.Equal(1, await Db.ScheduledTasks.CountAsync());
        Assert.Equal("completed", (await Db.ScheduledTasks.FirstAsync()).Status);
    }

    [Fact]
    public async Task DeleteEquipment_HomeNotFound_Returns404()
    {
        var user = await SeedUserAsync("del4@test.com");
        var ctx = MakeCtx(user.Id);

        var result = await DeleteEquipmentHandler.Handle(Guid.NewGuid(), Guid.NewGuid(), ctx, Db, default);

        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(404, r.StatusCode);
    }

    [Fact]
    public async Task DeleteEquipment_EquipmentNotFound_Returns404()
    {
        var user = await SeedUserAsync("del5@test.com");
        var home = await SeedHomeAsync(user.Id);
        var ctx = MakeCtx(user.Id);

        var result = await DeleteEquipmentHandler.Handle(home.Id, Guid.NewGuid(), ctx, Db, default);

        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(404, r.StatusCode);
    }

    [Fact]
    public async Task DeleteEquipment_OtherUsersHome_Returns404()
    {
        var owner = await SeedUserAsync("del6a@test.com");
        var other = await SeedUserAsync("del6b@test.com");
        var home = await SeedHomeAsync(owner.Id);
        var eq = new Data.Entities.Equipment { HomeId = home.Id, Type = "ac", Name = "Split" };
        Db.Equipment.Add(eq);
        await Db.SaveChangesAsync();
        var ctx = MakeCtx(other.Id);

        var result = await DeleteEquipmentHandler.Handle(home.Id, eq.Id, ctx, Db, default);

        var r = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(404, r.StatusCode);
    }
}
