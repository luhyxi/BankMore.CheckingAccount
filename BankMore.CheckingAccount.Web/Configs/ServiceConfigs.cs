using BankMore.CheckingAccount.Infrastructure;

namespace BankMore.CheckingAccount.Web.Configs;

public static class ServiceConfigs
{
  public static IServiceCollection AddServiceConfigs(this IServiceCollection services, Microsoft.Extensions.Logging.ILogger logger, WebApplicationBuilder builder)
  {
    services.AddInfrastructure(builder.Configuration, logger)
            .AddMediatorSourceGen(logger);

    // if (builder.Environment.IsDevelopment())
    // {
    //   // Use a local test email server - configured in Aspire
    //   // See: https://ardalis.com/configuring-a-local-test-email-server/
    //   services.AddScoped<IEmailSender, MimeKitEmailSender>();
    //
    //   // Otherwise use this:
    //   //builder.Services.AddScoped<IEmailSender, FakeEmailSender>();
    // }
    // else
    // {
    //   services.AddScoped<IEmailSender, MimeKitEmailSender>();
    // }

    logger.LogInformation("services registered");

    return services;
  }

    
}