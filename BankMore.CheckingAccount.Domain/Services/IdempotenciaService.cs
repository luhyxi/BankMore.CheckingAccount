using BankMore.CheckingAccount.Domain.IdempotenciaAggregate;
using BankMore.CheckingAccount.Domain.Interfaces;

using Mediator;

using Microsoft.Extensions.Logging;

namespace BankMore.CheckingAccount.Domain.Services;

public sealed class IdempotenciaService(
    IIdempotenciaRepository repository,
    IMediator mediator,
    ILogger<IdempotenciaService> logger)
    : IIdempotenciaService
{
    public async ValueTask<IResult<bool>> SaveIdempotencia(Idempotencia idempotencia)
    {
        ArgumentNullException.ThrowIfNull(idempotencia);

        try
        {
            await repository.CreateAsync(idempotencia);

            logger.LogInformation(
                "Idempotencia {IdempotenciaId} saved successfully",
                idempotencia.IdempotenciaId);

            return Result<bool>.Success(true);
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Failed to save idempotencia {IdempotenciaId}", idempotencia.IdempotenciaId);
            return Result<bool>.Failure(ex.Message);
        }
    }

    public async ValueTask<IResult<bool>> RequestHashExists(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            return Result<bool>.Failure("Hash must be provided.");
        }

        try
        {
            _ = await repository.GetByRequisicaoAsync(hash);
            return Result<bool>.Success(true);
        }
        catch (KeyNotFoundException)
        {
            return Result<bool>.Success(false);
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Failed to check idempotencia request hash");
            return Result<bool>.Failure(ex.Message);
        }
    }

    public async ValueTask<IResult<bool>> ChangeIdempotenciaStatus(
        string id,
        IdempotenciaResult idempotenciaResult)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Result<bool>.Failure("Idempotencia id must be provided.");
        }

        if (!Guid.TryParse(id, out _))
        {
            return Result<bool>.Failure("Idempotencia id must be a valid GUID.");
        }

        try
        {
            var idempotencia = await repository.GetByIdAsync(Guid.Parse(id));
            idempotencia.SetResultado(idempotenciaResult);
            await repository.UpdateAsync(idempotencia);

            logger.LogInformation(
                "Idempotencia {IdempotenciaId} updated to {IdempotenciaResult}",
                idempotencia.IdempotenciaId,
                idempotenciaResult);

            return Result<bool>.Success(true);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogError(ex, "Idempotencia {IdempotenciaId} was not found", id);
            return Result<bool>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Failed to update idempotencia {IdempotenciaId}", id);
            return Result<bool>.Failure(ex.Message);
        }
    }
}
