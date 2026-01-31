using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;

namespace BankMore.CheckingAccount.Domain.Interfaces;

public interface IJwtService
{
    string GenerateToken(ContaCorrente conta);
}
