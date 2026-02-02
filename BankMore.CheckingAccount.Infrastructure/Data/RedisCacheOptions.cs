namespace BankMore.CheckingAccount.Infrastructure.Data;

public sealed class RedisCacheOptions
{
    public int SaldoCacheTtlSeconds { get; set; } = 300;
}
