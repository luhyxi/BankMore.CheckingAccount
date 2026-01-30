using SharedKernel;

namespace BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;

public sealed class ContaCorrenteId : ValueObject
{
    public Guid Value { get; }

    public ContaCorrenteId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Conta corrente id must be a non-empty GUID.", nameof(value));
        }

        Value = value;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }
}
