using System.Data;
using System.Linq;

using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;
using BankMore.CheckingAccount.Domain.Interfaces;
using BankMore.CheckingAccount.Infrastructure.Data;

using Dapper;

namespace BankMore.CheckingAccount.Infrastructure.Repositories;

public sealed class ContaCorrenteRepository : IContaCorrenteRepository
{
    private const string TableName = "contacorrente";

    private const string SelectSql = """
        SELECT
            idcontacorrente AS Id,
            numero AS Numero,
            nome AS Nome,
            cpf AS Cpf,
            senha AS Senha,
            salt AS Salt,
            ativo AS Ativo
        FROM contacorrente
        """;

    private const string InsertSql = """
        INSERT INTO contacorrente (
            idcontacorrente,
            numero,
            nome,
            cpf,
            senha,
            salt,
            ativo
        )
        VALUES (
            @Id,
            @Numero,
            @Nome,
            @Cpf,
            @Senha,
            @Salt,
            @Ativo
        )
        """;

    private const string UpdateSql = """
        UPDATE contacorrente
        SET
            numero = @Numero,
            nome = @Nome,
            cpf = @Cpf,
            senha = @Senha,
            salt = @Salt,
            ativo = @Ativo
        WHERE idcontacorrente = @Id
        """;

    private const string DeleteSql = $"DELETE FROM {TableName} WHERE idcontacorrente = @Id";

    private readonly IDbConnectionFactory _connectionFactory;

    public ContaCorrenteRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async ValueTask CreateAsync(ContaCorrente contaCorrente, CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var parameters = BuildParameters(contaCorrente);
        var command = new CommandDefinition(InsertSql, parameters, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
    }

    public async ValueTask<IReadOnlyCollection<ContaCorrente>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(SelectSql, cancellationToken: cancellationToken);
        var results = await connection.QueryAsync<ContaCorrenteRow>(command);
        return results.Select(Map).ToArray();
    }

    public async ValueTask<ContaCorrente> GetByIdAsync(ContaCorrenteId id, CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var sql = $"{SelectSql} WHERE idcontacorrente = @Id";
        var command = new CommandDefinition(sql, new { Id = id.Value }, cancellationToken: cancellationToken);
        var result = await connection.QuerySingleOrDefaultAsync<ContaCorrenteRow>(command);
        return result is null
            ? throw new KeyNotFoundException($"ContaCorrente with id '{id.Value}' was not found.")
            : Map(result);
    }

    public async ValueTask<ContaCorrente> GetByCpfAsync(ContaCorrenteCpf cpf, CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var sql = $"{SelectSql} WHERE cpf = @Cpf";
        var command = new CommandDefinition(sql, new { Cpf = cpf.Value }, cancellationToken: cancellationToken);
        var result = await connection.QuerySingleOrDefaultAsync<ContaCorrenteRow>(command);
        return result is null
            ? throw new KeyNotFoundException($"ContaCorrente with cpf '{cpf.Value}' was not found.")
            : Map(result);
    }

    public async ValueTask<ContaCorrente> GetByNumeroAsync(ContaCorrenteNumero numero, CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var sql = $"{SelectSql} WHERE numero = @Numero";
        var command = new CommandDefinition(sql, new { Numero = numero.Value }, cancellationToken: cancellationToken);
        var result = await connection.QuerySingleOrDefaultAsync<ContaCorrenteRow>(command);
        return result is null
            ? throw new KeyNotFoundException($"ContaCorrente with numero '{numero.Value}' was not found.")
            : Map(result);
    }

    public async ValueTask<bool> ExistsByNumeroAsync(ContaCorrenteNumero numero, CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        const string sql = "SELECT 1 FROM contacorrente WHERE numero = @Numero LIMIT 1";
        var command = new CommandDefinition(sql, new { Numero = numero.Value }, cancellationToken: cancellationToken);
        var result = await connection.QuerySingleOrDefaultAsync<int?>(command);
        return result.HasValue;
    }

    public async ValueTask UpdateAsync(ContaCorrente contaCorrente, CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var parameters = BuildParameters(contaCorrente);
        var command = new CommandDefinition(UpdateSql, parameters, cancellationToken: cancellationToken);
        var affected = await connection.ExecuteAsync(command);
        if (affected == 0)
        {
            throw new KeyNotFoundException($"ContaCorrente with id '{contaCorrente.Id.Value}' was not found.");
        }
    }

    public async ValueTask DeleteAsync(ContaCorrenteId id, CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(DeleteSql, new { Id = id.Value }, cancellationToken: cancellationToken);
        var affected = await connection.ExecuteAsync(command);
        if (affected == 0)
        {
            throw new KeyNotFoundException($"ContaCorrente with id '{id.Value}' was not found.");
        }
    }

    private async ValueTask<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await _connectionFactory.OpenConnectionAsync(cancellationToken);
    }

    private static object BuildParameters(ContaCorrente contaCorrente)
    {
        return new
        {
            Id = contaCorrente.Id.Value,
            Numero = contaCorrente.Numero.Value,
            Nome = contaCorrente.Nome.Value,
            Cpf = contaCorrente.Cpf.Value,
            Senha = contaCorrente.Senha.Value,
            Salt = contaCorrente.Salt,
            Ativo = contaCorrente.Ativo
        };
    }

    private static ContaCorrente Map(ContaCorrenteRow row)
    {
        return new ContaCorrente(
            new ContaCorrenteId(Guid.Parse(row.Id)),
            new ContaCorrenteNumero(row.Numero),
            new ContaCorrenteNome(row.Nome),
            new ContaCorrenteCpf(row.Cpf),
            new ContaCorrenteSenha(row.Senha),
            row.Salt,
            row.Ativo == 1);
    }

    private sealed class ContaCorrenteRow
    {
        public string Id { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public long Ativo { get; set; }
    }
}
