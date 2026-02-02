using System.Text.RegularExpressions;

using SharedKernel;

namespace BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;

public class ContaCorrenteCpf : ValueObject
{
    public string Value { get; }

    public ContaCorrenteCpf(string value)
    {
        if (!ValidCpf(value))
        {
            throw new ArgumentException("Invalid CPF", value);
        }

        Value = Regex.Replace(value, @"\D", "");
    }
    
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }
    
    public bool ValidCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        // remove non-digits
        cpf = Regex.Replace(cpf, @"\D", "");

        // must be 11 digits and not all equal
        if (!Regex.IsMatch(cpf, @"^(?!([0-9])\1{10})\d{11}$"))
            return false;

        int sum = 0;
        for (int i = 0; i < 9; i++)
            sum += (cpf[i] - '0') * (10 - i);

        int d1 = (sum * 10) % 11;
        if (d1 == 10) d1 = 0;
        if (d1 != cpf[9] - '0') return false;

        sum = 0;
        for (int i = 0; i < 10; i++)
            sum += (cpf[i] - '0') * (11 - i);

        int d2 = (sum * 10) % 11;
        if (d2 == 10) d2 = 0;

        return d2 == cpf[10] - '0';
    }
}