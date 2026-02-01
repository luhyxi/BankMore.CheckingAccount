using BankMore.CheckingAccount.Domain.IdempotenciaAggregate;
using BankMore.CheckingAccount.Domain.MovimentoAggregate;

namespace BankMore.CheckingAccount.Application.Movimentacao.Command.Transaction;

public sealed record TransactionCommand(
    Idempotencia Idempotencia,
    ContaCorrenteNumero Numero,
    decimal Valor,
    TipoMovimento Tipo
    )
    : ICommand<IResult<string>>;
