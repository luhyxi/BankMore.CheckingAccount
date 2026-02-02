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
    public async ValueTask<IResult<string>> Handle(CreateContaCorrenteCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var cpf = command.Cpf;
            var id = Guid.NewGuid();
            var numero = ContaCorrenteNumero.Create(); // TODO: Tem que checar a existencia desse numero na DB
            var nome = command.Nome;
            var senha = passwordHashingService.Hash(command.Senha, out var salt);

            var conta = new ContaCorrenteModel(new ContaCorrenteId(id), numero, nome, cpf, senha, salt);

            await repository.CreateAsync(conta, cancellationToken);

            logger.LogInformation("Account {AccountId} created successfully", conta.Numero);
            return Result<string>.Success(numero.Value);
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "Account of user named {Name} failed to be created", command.Nome);
            return Result<string>.Failure(ex.Message);
        }
    }
}
