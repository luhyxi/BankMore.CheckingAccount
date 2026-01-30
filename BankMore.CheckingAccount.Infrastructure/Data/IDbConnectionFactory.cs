using System.Data.Common;

namespace BankMore.CheckingAccount.Infrastructure.Data;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}