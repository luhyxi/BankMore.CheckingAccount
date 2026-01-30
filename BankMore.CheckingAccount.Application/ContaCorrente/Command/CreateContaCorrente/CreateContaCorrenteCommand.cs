using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;

using Mediator;

namespace BankMore.CheckingAccount.Application.ContaCorrente.Command.CreateContaCorrente;

public sealed record CreateContaCorrenteCommand(
    ContaCorrenteNumero Numero, 
    ContaCorrenteNome Nome, 
    ContaCorrenteSenha Senha) 
    : ICommand<Guid>;
