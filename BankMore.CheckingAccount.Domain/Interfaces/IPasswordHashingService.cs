using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;

namespace BankMore.CheckingAccount.Domain.Interfaces;

public interface IPasswordHashingService
{
    ContaCorrenteSenha Hash(string plainText, out string salt);
    bool Verify(string plainText, string salt, string expectedHash);
}
