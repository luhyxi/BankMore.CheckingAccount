using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;

namespace BankMore.CheckingAccount.Application.ContaCorrente.Query.GetIdByNumero;

public sealed record GetIdByNumeroQuery(ContaCorrenteNumero Numero)
    : IQuery<Result<Guid>>;
