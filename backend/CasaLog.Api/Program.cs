using CasaLog.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCasaLogServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CasaLogContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapCasaLogEndpoints();

app.Run();

public partial class Program;
