namespace CasaLog.Api.Extensions;

internal static class HttpContextExtensions
{
    internal static Guid GetUserId(this HttpContext ctx)
    {
        var value = ctx.User.FindFirst("sub")?.Value;
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }
}
