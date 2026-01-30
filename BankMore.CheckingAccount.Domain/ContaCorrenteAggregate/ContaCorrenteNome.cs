using SharedKernel;

namespace BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;

public sealed class ContaCorrenteNome : ValueObject
{
    public string Value { get; }
    public const int MaxLength = 100;

    public ContaCorrenteNome(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > MaxLength)
        {
            throw new ArgumentException("Conta corrente nome must be provided.", nameof(value));
        }

        Value = value;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }
}
