using SharedKernel;

namespace BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;

public sealed class ContaCorrenteNumero : ValueObject
{
    public string Value { get; }
    public const int ContaLenght = 10;
    
    public ContaCorrenteNumero(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != ContaLenght)
        {
            throw new ArgumentException($"Conta corrente numero must be provided and must have {ContaLenght} digits.", 
                value);
        }

        Value = value;
    }

    public static ContaCorrenteNumero Create()
    {
        var value = Random.Shared
            .Next(0, 1_000_000_000)
            .ToString("D10");

        return new ContaCorrenteNumero(value);
    }

    
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }
}
