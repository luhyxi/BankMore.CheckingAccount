using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;

using ContaCorrenteModel = BankMore.CheckingAccount.Domain.ContaCorrenteAggregate.ContaCorrente;

using Mediator;



using SharedKernel;

namespace BankMore.CheckingAccount.Application.ContaCorrente.Command.CreateContaCorrente;

public sealed class CreateContaCorrenteHandler(IRepository<ContaCorrenteModel> repository)
    : ICommandHandler<CreateContaCorrenteCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateContaCorrenteCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var id = Guid.NewGuid();
        var numero = new ContaCorrenteNumero(command.Numero.Value);
        var nome = new ContaCorrenteNome(command.Nome.Value);
        var senha = new ContaCorrenteSenha(command.Senha.Value);

        var conta = new ContaCorrenteModel(new ContaCorrenteId(id), numero, nome, senha);

        await repository.CreateAsync(conta, cancellationToken);

        return id;
    }
}
