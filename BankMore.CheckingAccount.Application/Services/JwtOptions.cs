namespace BankMore.CheckingAccount.Application.Services;

public sealed class JwtOptions()
{
    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
    public string SigningKey { get; init; } = null!;
    public int ExpirationMinutes { get; init; } = 60;
}
