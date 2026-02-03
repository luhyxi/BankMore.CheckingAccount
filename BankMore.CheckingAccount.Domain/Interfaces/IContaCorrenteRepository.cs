using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;

namespace BankMore.CheckingAccount.Domain.Interfaces;

public interface IContaCorrenteRepository
{
    ValueTask CreateAsync(ContaCorrente contaCorrente, CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyCollection<ContaCorrente>> GetAllAsync(CancellationToken cancellationToken = default);

    ValueTask<ContaCorrente> GetByIdAsync(ContaCorrenteId id, CancellationToken cancellationToken = default);

    ValueTask<ContaCorrente> GetByCpfAsync(ContaCorrenteCpf cpf, CancellationToken cancellationToken = default);

    ValueTask<ContaCorrente> GetByNumeroAsync(ContaCorrenteNumero numero, CancellationToken cancellationToken = default);

    ValueTask<bool> ExistsByNumeroAsync(ContaCorrenteNumero numero, CancellationToken cancellationToken = default);

    ValueTask UpdateAsync(ContaCorrente contaCorrente, CancellationToken cancellationToken = default);

    ValueTask DeleteAsync(ContaCorrenteId id, CancellationToken cancellationToken = default);
}
