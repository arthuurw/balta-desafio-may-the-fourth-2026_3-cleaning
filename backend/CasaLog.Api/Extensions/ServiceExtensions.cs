using System.IdentityModel.Tokens.Jwt;
using System.Text;
using CasaLog.Api.Agents;
using CasaLog.Api.Infrastructure.Llm;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.AI;
using Microsoft.IdentityModel.Tokens;

namespace CasaLog.Api.Extensions;

internal static class ServiceExtensions
{
    internal static IServiceCollection AddCasaLogServices(this IServiceCollection services, IConfiguration config)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services.AddDbContext<CasaLogContext>(opt =>
            opt.UseSqlite(config.GetConnectionString("Default") ?? "Data Source=casalog.db"));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
                };
            });

        services.AddAuthorization();

        services.AddSingleton<IChatClient>(_ => LlmClientFactory.Create(config));
        services.AddScoped<IHomeAgent, HomeAgent>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddCors(opt => opt.AddDefaultPolicy(p =>
            p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

        return services;
    }
}
