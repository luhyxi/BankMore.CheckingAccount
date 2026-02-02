using System.Data;

using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;
using BankMore.CheckingAccount.Domain.Interfaces;
using BankMore.CheckingAccount.Infrastructure.Data;

using Dapper;

namespace BankMore.CheckingAccount.Infrastructure.Repositories;

public sealed class MovimentoRepository : IMovimentoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MovimentoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async ValueTask<decimal> GetSaldoAsync(ContaCorrenteId id, CancellationToken cancellationToken = default)
    {
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
        var command = new CommandDefinition(sql, new { Id = id.Value }, cancellationToken: cancellationToken);
        return await connection.ExecuteScalarAsync<decimal>(command);
    }

    private async ValueTask<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await _connectionFactory.OpenConnectionAsync(cancellationToken);
    }
}
