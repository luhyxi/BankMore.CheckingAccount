using StackExchange.Redis;

namespace BankMore.CheckingAccount.Infrastructure.Data;

public sealed class RedisContext : IRedisContext
{
    public IDatabase Database { get; }

    public RedisContext(IConnectionMultiplexer multiplexer)
    {
        Database = multiplexer.GetDatabase();
    }
}
