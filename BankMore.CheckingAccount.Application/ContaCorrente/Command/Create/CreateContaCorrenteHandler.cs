using BankMore.CheckingAccount.Domain.Interfaces;

using Microsoft.Extensions.Logging;

using ContaCorrenteModel = BankMore.CheckingAccount.Domain.ContaCorrenteAggregate.ContaCorrente;

using SharedKernel;

namespace BankMore.CheckingAccount.Application.ContaCorrente.Command.Create;

public sealed class CreateContaCorrenteHandler(
    IContaCorrenteRepository repository,
    IPasswordHashingService passwordHashingService,
    ILogger<CreateContaCorrenteHandler> logger)
    : ICommandHandler<CreateContaCorrenteCommand, IResult<string>>
{
    private const int MaxNumeroGenerationAttempts = 5;

    public async ValueTask<IResult<string>> Handle(CreateContaCorrenteCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var cpf = command.Cpf;
            var id = Guid.NewGuid();
            var nome = command.Nome;
            var senha = passwordHashingService.Hash(command.Senha, out var salt);

            for (var attempt = 1; attempt <= MaxNumeroGenerationAttempts; attempt++)
            {
                var numero = ContaCorrenteNumero.Create();
                if (await repository.ExistsByNumeroAsync(numero, cancellationToken))
                {
                    logger.LogWarning("Duplicate account number generated; retrying (attempt {Attempt})", attempt);
                    continue;
                }

                var conta = new ContaCorrenteModel(new ContaCorrenteId(id), numero, nome, cpf, senha, salt);
                await repository.CreateAsync(conta, cancellationToken);

                logger.LogInformation("Account {AccountId} created successfully", conta.Numero);
                return Result<string>.Success(numero.Value);
            }

            return Result<string>.Failure("Unable to generate a unique account number. Please try again.");
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "Account of user named {Name} failed to be created", command.Nome);
            return Result<string>.Failure(ex.Message);
        }
    }
}
