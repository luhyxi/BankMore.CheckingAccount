using System.Globalization;

using BankMore.CheckingAccount.Domain.Interfaces;

using Microsoft.Extensions.Logging;

namespace BankMore.CheckingAccount.Application.ContaCorrente.Query.GetSaldo;

public sealed class GetSaldoHandler(
    IContaCorrenteRepository contaCorrenteRepository,
    IMovimentoRepository movimentoRepository,
    ILogger<GetSaldoHandler> logger)
    : IQueryHandler<GetSaldoQuery, Result<SaldoResult>>
{
    public async ValueTask<Result<SaldoResult>> Handle(GetSaldoQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        try
        {
            var conta = await contaCorrenteRepository.GetByIdAsync(query.ContaCorrenteId, cancellationToken);

            if (!conta.Ativo) return Result<SaldoResult>.Failure("Conta corrente is inactive.");

            var saldo = await movimentoRepository.GetSaldoAsync(conta.Id, cancellationToken);
            var response = new SaldoResult(
                conta.Numero.Value,
                conta.Nome.Value,
                DateTime.UtcNow,
                saldo.ToString("F2", CultureInfo.InvariantCulture));

            return Result<SaldoResult>.Success(response);
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Saldo query failed due to invalid data for account {AccountId}", query.ContaCorrenteId);
            return Result<SaldoResult>.Failure(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogError(ex, "Saldo query failed because account {AccountId} was not found", query.ContaCorrenteId);
            return Result<SaldoResult>.Failure(ex.Message);
        }
    }
}
