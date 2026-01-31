using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;
using SharedKernel;

namespace BankMore.CheckingAccount.Application.ContaCorrente.Command.Inactivate;

public sealed record InactivateContaCorrenteCommand(
    ContaCorrenteId ContaCorrenteId,
    ContaCorrenteSenha Senha)
    : ICommand<IResult<bool>>;
