using SharedKernel;

namespace BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;

public sealed class ContaCorrenteSenha : ValueObject
{
    private const int StoredHashMaxLength = 100;

    public string Value { get; }

    public ContaCorrenteSenha(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > StoredHashMaxLength)
        {
            throw new ArgumentException("Conta corrente senha must be provided and must have less than 100 characters.", nameof(value));
        }

        Value = value;
    }

    
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

}
