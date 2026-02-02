using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;
using BankMore.CheckingAccount.Domain.Interfaces;

using Microsoft.Extensions.Logging;

using ContaCorrenteModel = BankMore.CheckingAccount.Domain.ContaCorrenteAggregate.ContaCorrente;

using SharedKernel;

namespace BankMore.CheckingAccount.Application.ContaCorrente.Command.Inactivate;

public class InactivateContaCorrenteHandler(
    IContaCorrenteRepository repository,
    IPasswordHashingService passwordHashingService,
    ILogger<InactivateContaCorrenteHandler> logger)
    : ICommandHandler<InactivateContaCorrenteCommand, IResult<bool>>
{
    public async ValueTask<IResult<bool>> Handle(InactivateContaCorrenteCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var conta = await repository.GetByIdAsync(command.ContaCorrenteId, cancellationToken);

            if (!passwordHashingService.Verify(command.Senha.Value, conta.Salt, conta.Senha.Value))
            {
                return Result<bool>.Failure("Invalid credentials.");
            }

            var updated = new ContaCorrenteModel (
                conta.Id,
                conta.Numero,
                conta.Nome,
                conta.Cpf,
                conta.Senha,
                conta.Salt,
                false);

            await repository.UpdateAsync(updated, cancellationToken);
            
            logger.LogInformation("Account {AccountId} inactivated successfully", command.ContaCorrenteId);
            
            return Result<bool>.Success(true);
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Account {AccountId} failed to be inactivated", command.ContaCorrenteId);
            return Result<bool>.Failure(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogError(ex, "Account {AccountId} wasn't found", command.ContaCorrenteId);
            return Result<bool>.Failure(ex.Message);
        }
    }
}