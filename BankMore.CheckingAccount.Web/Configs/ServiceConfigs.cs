using BankMore.CheckingAccount.Application.Services;
using BankMore.CheckingAccount.Domain.Interfaces;
using BankMore.CheckingAccount.Domain.Services;
using BankMore.CheckingAccount.Infrastructure;
using BankMore.CheckingAccount.Infrastructure.Data;

using StackExchange.Redis;

namespace BankMore.CheckingAccount.Web.Configs;

public static class ServiceConfigs
{
    public static IServiceCollection AddServiceConfigs(this IServiceCollection services, ILogger logger,
        WebApplicationBuilder builder)
    {
        services.AddInfrastructure(builder.Configuration, logger)
            .AddMediatorSourceGen(logger);
        services.AddScoped<IPasswordHashingService, PasswordHashingService>();
        services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IIdempotenciaService, IdempotenciaService>();
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var connectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Redis connection string is not configured.");
            }

            return ConnectionMultiplexer.Connect(connectionString);
        });
        services.AddSingleton<IRedisContext, RedisContext>();
        logger.LogInformation("services registered");

        return services;
    }
}