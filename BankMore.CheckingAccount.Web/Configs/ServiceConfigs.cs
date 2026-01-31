using BankMore.CheckingAccount.Application.Services;
using BankMore.CheckingAccount.Domain.Interfaces;
using BankMore.CheckingAccount.Infrastructure;

namespace BankMore.CheckingAccount.Web.Configs;

public static class ServiceConfigs
{
  public static IServiceCollection AddServiceConfigs(this IServiceCollection services, ILogger logger, WebApplicationBuilder builder)
  {
    services.AddInfrastructure(builder.Configuration, logger)
            .AddMediatorSourceGen(logger);
    services.AddScoped<IPasswordHashingService, PasswordHashingService>();
    services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
    services.AddScoped<IJwtService, JwtService>();
    
    logger.LogInformation("services registered");

    return services;
  }

    
}
