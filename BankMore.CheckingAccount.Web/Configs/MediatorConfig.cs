using BankMore.CheckingAccount.Application.ContaCorrente.Command.Create;
using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;
using BankMore.CheckingAccount.Infrastructure;


namespace BankMore.CheckingAccount.Web.Configs;

public static class MediatorConfig
{
  public static IServiceCollection AddMediatorSourceGen(this IServiceCollection services,
    ILogger logger)
  {
    logger.LogInformation("Registering Mediator SourceGen and Behaviors");
    services.AddMediator(options =>
    {
      options.ServiceLifetime = ServiceLifetime.Scoped;

      options.Assemblies =
      [
        typeof(CreateContaCorrenteCommand),
      ];
    });

    return services;
  }
}
