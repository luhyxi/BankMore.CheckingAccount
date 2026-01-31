using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;

using SharedKernel;

namespace BankMore.CheckingAccount.Application.ContaCorrente.Command.Login;

public sealed record LoginContaCorrenteCommand(
    bool IsCpf,
    ContaCorrenteCpf cpf, 
    ContaCorrenteNumero numero,
    ContaCorrenteSenha senha)
    : ICommand<IResult<string>>;
