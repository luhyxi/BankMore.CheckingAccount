using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;

using SharedKernel;

namespace BankMore.CheckingAccount.Application.ContaCorrente.Command.Login;

public sealed record LoginContaCorrenteCommand(
    bool IsCpf,
    ContaCorrenteSenha senha,
    ContaCorrenteNumero? numero = null,
    ContaCorrenteCpf? cpf = null)
    : ICommand<IResult<string>>;
