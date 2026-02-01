using BankMore.CheckingAccount.Domain.Interfaces;
using BankMore.CheckingAccount.Domain.IdempotenciaAggregate;
using BankMore.CheckingAccount.Domain.MovimentoAggregate;

using Microsoft.Extensions.Logging;

namespace BankMore.CheckingAccount.Application.Movimentacao.Command.Transaction;

public sealed class TransactionHandler(
    IContaCorrenteRepository contaCorrenteRepository,
    IIdempotenciaService idempotenciaService,
    ILogger<TransactionHandler> logger)
    : ICommandHandler<TransactionCommand, IResult<string>>
{
    public async ValueTask<IResult<string>> Handle(TransactionCommand command, CancellationToken cancellationToken)
    {
        if (!CommandParamsIsValid(command, out string? result))
            return Result<string>.Failure(result!);

        try
        {
            var conta = await contaCorrenteRepository.GetByNumeroAsync(command.Numero, cancellationToken);

            if (!conta.Ativo)
                return Result<string>.Failure("Conta corrente is inactive.");

            var hashExistsResult = await idempotenciaService.RequestHashExists(command.Idempotencia.Requisicao!);
            if (!hashExistsResult.IsSuccess)
                return Result<string>.Failure(hashExistsResult.Error ?? "Failed to check idempotency hash.");

            if (hashExistsResult.Value)
                return Result<string>.Failure("Duplicate transaction request.");

            var saveResult = await idempotenciaService.SaveIdempotencia(command.Idempotencia);
            if (!saveResult.IsSuccess)
                return Result<string>.Failure(saveResult.Error ?? "Failed to save idempotency request.");

            var statusResult = await idempotenciaService.ChangeIdempotenciaStatus(
                command.Idempotencia.IdempotenciaId.ToString(),
                IdempotenciaResult.Done);

            if (!statusResult.IsSuccess)
            {
                return Result<string>.Failure(statusResult.Error ?? "Failed to update idempotency status.");
            }

            logger.LogInformation(
                "Transaction for account {Numero} completed with idempotencia {IdempotenciaId}",
                command.Numero.Value,
                command.Idempotencia.IdempotenciaId);

            return Result<string>.Success(command.Idempotencia.IdempotenciaId.ToString());
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Transaction failed due to invalid data for account {Numero}", command.Numero.Value);
            return Result<string>.Failure(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogError(ex, "Transaction failed because account {Numero} was not found", command.Numero.Value);
            return Result<string>.Failure(ex.Message);
        }
    }

    private static bool CommandParamsIsValid(TransactionCommand command, out string? result)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Valor <= 0)
        {
            result = "Transaction value must be greater than zero.";
            return false;
        }

        if (command.Tipo == TipoMovimento.None)
        {
            result = "Transaction type must be provided.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(command.Idempotencia.Requisicao))
        {
            result = "Idempotency hash must be provided.";
            return false;
        }

        result = null;
        return true;
    }
}
