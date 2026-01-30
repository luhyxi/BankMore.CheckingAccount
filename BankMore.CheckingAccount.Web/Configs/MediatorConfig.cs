using BankMore.CheckingAccount.Application.ContaCorrente.Command.CreateContaCorrente;
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
        typeof(CreateContaCorrenteCommand),     // UseCases
      ];

      // Register pipeline behaviors here (order matters)
      // options.PipelineBehaviors =
      // [
      //   typeof(LoggingBehavior<,>)
      // ];

      // If you have stream behaviors:
      // options.StreamPipelineBehaviors = [ typeof(YourStreamBehavior<,>) ];
    });

    return services;
  }
}
