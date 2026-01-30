using SharedKernel;

namespace BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;

public sealed class ContaCorrenteNumero : ValueObject
{
    public string Value { get; }
    public const string Agencia = "0001";
    public const int ContaLenght = 6;
    
    public ContaCorrenteNumero(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != ContaLenght)
        {
            throw new ArgumentException("Conta corrente numero must be provided and must have 6 digits.", 
                nameof(value));
        }

        Value = value;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }
}
