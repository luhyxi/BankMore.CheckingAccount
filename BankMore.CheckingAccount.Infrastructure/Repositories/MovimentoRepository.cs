using System.Data;
using System.Globalization;

using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;
using BankMore.CheckingAccount.Domain.Interfaces;
using BankMore.CheckingAccount.Infrastructure.Data;

using Dapper;

using Microsoft.Extensions.Options;

using StackExchange.Redis;

namespace BankMore.CheckingAccount.Infrastructure.Repositories;

public sealed class MovimentoRepository(
    IDbConnectionFactory connectionFactory,
    IRedisContext redisContext,
    IOptions<RedisCacheOptions> cacheOptions)
    : IMovimentoRepository
{
    private readonly IDatabase _redis = redisContext.Database;
    private readonly TimeSpan _saldoCacheTtl = GetSaldoCacheTtl(cacheOptions.Value);

    public async ValueTask<decimal> GetSaldoAsync(
        ContaCorrenteId id,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GetSaldoCacheKey(id);

        var cachedValue = await _redis.StringGetAsync(cacheKey);
        if (cachedValue.HasValue)
        {
            return decimal.Parse(
                (string)cachedValue!,
                CultureInfo.InvariantCulture);
        }

        const string sql = """
                           SELECT
                               COALESCE(SUM(
                                   CASE tipomovimento
                                       WHEN 'C' THEN valor
                                       WHEN 'D' THEN -valor
                                       ELSE 0
                                   END
                               ), 0)
                           FROM movimento
                           WHERE idcontacorrente = @Id
                           """;

        using var connection = await OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            sql,
            new { Id = id.Value },
            cancellationToken: cancellationToken);

        var saldo = await connection.ExecuteScalarAsync<decimal>(command);

        await _redis.StringSetAsync(
            cacheKey,
            saldo.ToString(CultureInfo.InvariantCulture),
            _saldoCacheTtl);

        return saldo;
    }

    private static string GetSaldoCacheKey(ContaCorrenteId id)
        => $"saldo:{id.Value}";

    private static TimeSpan GetSaldoCacheTtl(RedisCacheOptions options) 
        => options.SaldoCacheTtlSeconds <= 0 ?
            TimeSpan.FromMinutes(5) :
            TimeSpan.FromSeconds(options.SaldoCacheTtlSeconds);

    private async ValueTask<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default) 
        => await connectionFactory.OpenConnectionAsync(cancellationToken);
}
