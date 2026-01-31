using System.Security.Cryptography;
using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;
using BankMore.CheckingAccount.Domain.Interfaces;

namespace BankMore.CheckingAccount.Application.Services;

public sealed class PasswordHashingService : IPasswordHashingService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int HashIterations = 100_000;
    private const int SenhaLenght = 10;
    
    public ContaCorrenteSenha Hash(string plainText, out string salt)
        => FromPlainText(plainText, out salt);

    private static ContaCorrenteSenha FromPlainText(string plainText, out string salt)
    {
        ValidatePlainText(plainText);
        salt = GenerateSalt();
        var hash = HashPlainText(plainText, salt);
        return new ContaCorrenteSenha(hash);
    }

    public bool Verify(string plainText, string salt, string expectedHash)
    {
        if (string.IsNullOrWhiteSpace(expectedHash))
        {
            return false;
        }

        var computed = HashPlainText(plainText, salt);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(computed),
            Convert.FromBase64String(expectedHash));
    }

    private static void ValidatePlainText(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > SenhaLenght)
        {
            throw new ArgumentException(
                "Conta corrente senha must be provided and must have less than 10 characters.",
                nameof(value));
        }
    }

    private static string GenerateSalt()
    {
        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        return Convert.ToBase64String(saltBytes);
    }

    private static string HashPlainText(string plainText, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            plainText,
            saltBytes,
            HashIterations,
            HashAlgorithmName.SHA256,
            HashSize);
        return Convert.ToBase64String(hashBytes);
    }
}
