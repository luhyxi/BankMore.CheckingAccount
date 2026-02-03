using BankMore.CheckingAccount.Domain.Interfaces;

using Microsoft.Extensions.Logging;

namespace BankMore.CheckingAccount.Application.ContaCorrente.Query.GetIdByNumero;

public sealed class GetIdByNumeroHandler(
    IContaCorrenteRepository contaCorrenteRepository,
    ILogger<GetIdByNumeroHandler> logger)
    : IQueryHandler<GetIdByNumeroQuery, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(GetIdByNumeroQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        try
        {
            var conta = await contaCorrenteRepository.GetByNumeroAsync(query.Numero, cancellationToken);

            return Result<Guid>.Success(conta.Id.Value);
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "GetIdByNumero query failed due to invalid data for account number {AccountNumber}", query.Numero);
            return Result<Guid>.Failure(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogError(ex, "GetIdByNumero query failed because account {AccountNumber} was not found", query.Numero);
            return Result<Guid>.Failure(ex.Message);
        }
    }
}
