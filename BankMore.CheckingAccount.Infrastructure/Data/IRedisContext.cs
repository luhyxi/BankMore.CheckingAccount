using StackExchange.Redis;

namespace BankMore.CheckingAccount.Infrastructure.Data;

public interface IRedisContext
{
    IDatabase Database { get; }
}
