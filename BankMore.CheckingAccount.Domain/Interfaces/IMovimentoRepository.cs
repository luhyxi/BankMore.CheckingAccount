using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;
using BankMore.CheckingAccount.Domain.MovimentoAggregate;

namespace BankMore.CheckingAccount.Domain.Interfaces;

public interface IMovimentoRepository
{
    ValueTask<decimal> GetSaldoAsync(ContaCorrenteId id, CancellationToken cancellationToken = default);
    ValueTask CreateAsync(Movimento movimento, CancellationToken cancellationToken = default);
}