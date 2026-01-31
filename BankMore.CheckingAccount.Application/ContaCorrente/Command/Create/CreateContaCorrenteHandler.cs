using BankMore.CheckingAccount.Domain.Interfaces;

using ContaCorrenteModel = BankMore.CheckingAccount.Domain.ContaCorrenteAggregate.ContaCorrente;

using SharedKernel;

namespace BankMore.CheckingAccount.Application.ContaCorrente.Command.Create;

public sealed class CreateContaCorrenteHandler(
    IContaCorrenteRepository repository,
    IPasswordHashingService passwordHashingService)
    : ICommandHandler<CreateContaCorrenteCommand, IResult<Guid>>
{
    public async ValueTask<IResult<Guid>> Handle(CreateContaCorrenteCommand command, CancellationToken cancellationToken)
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

            return Result<Guid>.Success(id);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}
