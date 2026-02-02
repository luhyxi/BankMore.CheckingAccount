using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;

namespace BankMore.CheckingAccount.Domain.Interfaces;

public interface IMovimentoRepository
{
    ValueTask<decimal> GetSaldoAsync(ContaCorrenteId id, CancellationToken cancellationToken = default);
}