using BankMore.CheckingAccount.Infrastructure.Data;
using BankMore.CheckingAccount.Infrastructure.Repositories;
using SharedKernel;

namespace BankMore.CheckingAccount.Infrastructure;

public static class InfrastructureServiceInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger logger)
    {
        var databaseSection = configuration.GetSection("Database");
        var connectionString = databaseSection.GetValue<string>("ConnectionString");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger.LogWarning("Database connection string is not configured.");
        }

        services.Configure<DatabaseOptions>(databaseSection);
        services.AddScoped<IDbConnectionFactory, SqliteConnectionFactory>();
        
        services.AddScoped(typeof(IRepository<>), typeof(DapperRepository<>));
        
        return services;
    }
}
