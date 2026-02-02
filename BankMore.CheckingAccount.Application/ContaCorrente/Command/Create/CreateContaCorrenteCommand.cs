using SharedKernel;

namespace BankMore.CheckingAccount.Application.ContaCorrente.Command.Create;

public sealed record CreateContaCorrenteCommand(
    ContaCorrenteCpf Cpf,
    ContaCorrenteNome Nome,
    string Senha)
    : ICommand<IResult<string>>;
