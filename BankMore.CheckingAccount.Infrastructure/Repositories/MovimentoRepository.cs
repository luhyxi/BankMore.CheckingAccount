using System.Data;
using System.Globalization;

using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;
using BankMore.CheckingAccount.Domain.Interfaces;
using BankMore.CheckingAccount.Domain.MovimentoAggregate;
using BankMore.CheckingAccount.Infrastructure.Data;

using Dapper;

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

    private const string InsertSql = """
        INSERT INTO movimento(
            idmovimento,
            idcontacorrente,
            datamovimento,
            tipomovimento,
            valor
        )
        VALUES (
            @IdMovimento,
            @IdContaCorrente,
            @DataMovimento,
            @TipoMovimento,
            @Valor
        )
        """;
    
    private const string CalculateAccountBalanceSql= """
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
    public async ValueTask CreateAsync(
        Movimento movimento, 
        CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var parameters = BuildParameters(movimento);
        var command = new CommandDefinition(InsertSql, parameters, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
    }
    
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

        using var connection = await OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            CalculateAccountBalanceSql,
            new { Id = id.Value },
            cancellationToken: cancellationToken);

        var saldo = await connection.ExecuteScalarAsync<decimal>(command);

        await _redis.StringSetAsync(
            cacheKey,
            saldo.ToString(CultureInfo.InvariantCulture),
            _saldoCacheTtl);

        return saldo;
    }

    private static object BuildParameters(Movimento movimento)
    {
        return new
        {
            IdMovimento = movimento.MovimentoId.Value,
            IdContaCorrente = movimento.ContaCorrenteId.Value,
            DataMovimento = movimento.DataMovimento.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            TipoMovimento = movimento.TipoMovimento switch
            {
                TipoMovimento.Credito => "C",
                TipoMovimento.Debito => "D",
                _ => throw new ArgumentOutOfRangeException(nameof(movimento), "Invalid movimento type.")
            },
            Valor = movimento.Valor
        };
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
