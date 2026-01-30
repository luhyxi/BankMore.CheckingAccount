using SharedKernel;

namespace BankMore.CheckingAccount.Domain.MovimentoAggregate;

public sealed class MovimentoId : ValueObject
{
    public Guid Value { get; }

    public MovimentoId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Movimento id must be a non-empty GUID.", nameof(value));
        }

        Value = value;
    }


    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }
}