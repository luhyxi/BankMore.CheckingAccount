using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;

namespace BankMore.CheckingAccount.Application.ContaCorrente.Query.GetSaldo;

public sealed record GetSaldoQuery(ContaCorrenteId ContaCorrenteId)
    : IQuery<Result<SaldoResult>>;

public sealed record SaldoResult(
    string NumeroConta,
    string NomeTitular,
    DateTime DataConsulta,
    string SaldoAtual);
