using CasaLog.Api.Features.Alerts;
using CasaLog.Api.Features.Auth;
using CasaLog.Api.Features.Chat;
using CasaLog.Api.Features.Equipments;
using CasaLog.Api.Features.Homes;
using CasaLog.Api.Features.Tasks;

namespace CasaLog.Api.Extensions;

internal static class EndpointExtensions
{
    internal static WebApplication MapCasaLogEndpoints(this WebApplication app)
    {
        var auth = app.MapGroup("/api/auth");
        auth.MapPost("/register", RegisterHandler.Handle);
        auth.MapPost("/login", LoginHandler.Handle);

        var homes = app.MapGroup("/api/homes").RequireAuthorization();
        homes.MapPost("/", CreateHomeHandler.Handle);
        homes.MapGet("/mine", GetHomeHandler.Handle);
        homes.MapGet("/{homeId:guid}/report", GetHomeReportHandler.Handle);

        var equipment = app.MapGroup("/api/homes/{homeId:guid}/equipment").RequireAuthorization();
        equipment.MapPost("/", AddEquipmentHandler.Handle);
        equipment.MapDelete("/{equipmentId:guid}", DeleteEquipmentHandler.Handle);

        var tasks = app.MapGroup("/api/homes/{homeId:guid}/tasks").RequireAuthorization();
        tasks.MapGet("/", (Guid homeId, string? status, string? month, HttpContext ctx, CasaLogContext db, CancellationToken ct)
            => GetTasksHandler.Handle(homeId, status, month, ctx, db, ct));
        tasks.MapPost("/{taskId:guid}/complete", CompleteTaskHandler.Handle);
        tasks.MapPost("/{taskId:guid}/skip", SkipTaskHandler.Handle);

        var alerts = app.MapGroup("/api/alerts").RequireAuthorization();
        alerts.MapGet("/", GetAlertsHandler.Handle);
        alerts.MapPut("/{alertId:guid}/read", MarkAlertReadHandler.Handle);

        var chat = app.MapGroup("/api/chat").RequireAuthorization();
        chat.MapPost("/", ChatHandler.Handle);

        return app;
    }
}
