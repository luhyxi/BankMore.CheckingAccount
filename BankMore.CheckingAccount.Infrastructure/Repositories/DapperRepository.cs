using System.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

using BankMore.CheckingAccount.Infrastructure.Data;

using SharedKernel;
using Dapper;

namespace BankMore.CheckingAccount.Infrastructure.Repositories;

public class DapperRepository<T> : IRepository<T> where T : class, IAggregateRoot
{
    private readonly IDbConnectionFactory _connectionFactory;
    private static readonly string TableName = ResolveTableName();
    private static readonly PropertyInfo[] Properties = ResolveProperties();
    private static readonly PropertyInfo KeyProperty = ResolveKeyProperty(Properties);
    private static readonly string KeyColumnName = GetColumnName(KeyProperty);
    private static readonly string SelectAllSql = BuildSelectAllSql();
    private static readonly string SelectByIdSql = BuildSelectByIdSql();
    private static readonly string InsertSql = BuildInsertSql();
    private static readonly string UpdateSql = BuildUpdateSql();
    private static readonly string DeleteSql = BuildDeleteSql();

    public DapperRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private async ValueTask<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await _connectionFactory.OpenConnectionAsync(cancellationToken);
    }

    public async ValueTask CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var parameters = BuildParameters(entity, includeKey: true);
        var command = new CommandDefinition(InsertSql, parameters, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
    }

    public async ValueTask<Guid> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var parameters = new DynamicParameters();
        parameters.Add(KeyColumnName, id);
        var command = new CommandDefinition(DeleteSql, parameters, cancellationToken: cancellationToken);
        var affected = await connection.ExecuteAsync(command);
        return affected == 0 ? Guid.Empty : id;
    }

    public async ValueTask<IReadOnlyCollection<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(SelectAllSql, cancellationToken: cancellationToken);
        var results = await connection.QueryAsync<T>(command);
        return results.ToArray();
    }

    public async ValueTask<T> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var parameters = new DynamicParameters();
        parameters.Add(KeyColumnName, id);
        var command = new CommandDefinition(SelectByIdSql, parameters, cancellationToken: cancellationToken);
        var result = await connection.QuerySingleOrDefaultAsync<T>(command);
        if (result is null)
        {
            throw new KeyNotFoundException($"{typeof(T).Name} with id '{id}' was not found.");
        }

        return result;
    }

    public async ValueTask<T> UpdateAsync(Guid id, T entity, CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var parameters = BuildParameters(entity, includeKey: false);
        parameters.Add(KeyColumnName, id);
        var command = new CommandDefinition(UpdateSql, parameters, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
        return entity;
    }

    private static string ResolveTableName()
    {
        var tableAttribute = typeof(T).GetCustomAttribute<TableAttribute>();
        return tableAttribute?.Name ?? typeof(T).Name;
    }

    private static PropertyInfo[] ResolveProperties()
    {
        var properties = typeof(T)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.GetIndexParameters().Length == 0)
            .Where(property => !Attribute.IsDefined(property, typeof(NotMappedAttribute)))
            .ToArray();

        if (properties.Length == 0)
        {
            throw new InvalidOperationException($"{typeof(T).Name} does not define any mapped properties.");
        }

        return properties;
    }

    private static PropertyInfo ResolveKeyProperty(PropertyInfo[] properties)
    {
        var keyProperty = properties.FirstOrDefault(property => property.Name == "Id")
            ?? properties.FirstOrDefault(property => property.Name == $"{typeof(T).Name}Id");

        if (keyProperty is null)
        {
            throw new InvalidOperationException($"{typeof(T).Name} does not define a key property.");
        }

        return keyProperty;
    }

    private static string GetColumnName(PropertyInfo property)
    {
        var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
        return columnAttribute?.Name ?? property.Name;
    }

    private static string BuildSelectAllSql()
    {
        var columns = string.Join(", ", Properties.Select(property =>
            $"{QuoteIdentifier(GetColumnName(property))} AS {QuoteIdentifier(property.Name)}"));
        return $"SELECT {columns} FROM {QuoteIdentifier(TableName)}";
    }

    private static string BuildSelectByIdSql()
    {
        return $"{SelectAllSql} WHERE {QuoteIdentifier(KeyColumnName)} = @{KeyColumnName}";
    }

    private static string BuildInsertSql()
    {
        var columnList = string.Join(", ", Properties.Select(property => QuoteIdentifier(GetColumnName(property))));
        var parameterList = string.Join(", ", Properties.Select(property => $"@{GetColumnName(property)}"));
        return $"INSERT INTO {QuoteIdentifier(TableName)} ({columnList}) VALUES ({parameterList})";
    }

    private static string BuildUpdateSql()
    {
        var setClause = string.Join(", ", Properties
            .Where(property => property != KeyProperty)
            .Select(property => $"{QuoteIdentifier(GetColumnName(property))} = @{GetColumnName(property)}"));
        return $"UPDATE {QuoteIdentifier(TableName)} SET {setClause} WHERE {QuoteIdentifier(KeyColumnName)} = @{KeyColumnName}";
    }

    private static string BuildDeleteSql()
    {
        return $"DELETE FROM {QuoteIdentifier(TableName)} WHERE {QuoteIdentifier(KeyColumnName)} = @{KeyColumnName}";
    }

    private static DynamicParameters BuildParameters(T entity, bool includeKey)
    {
        var parameters = new DynamicParameters();
        foreach (var property in Properties)
        {
            if (!includeKey && property == KeyProperty)
            {
                continue;
            }

            var value = GetDbValue(property.GetValue(entity));
            parameters.Add(GetColumnName(property), value);
        }

        return parameters;
    }

    private static object? GetDbValue(object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value is ValueObject)
        {
            var valueProperty = value.GetType().GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);
            if (valueProperty is not null)
            {
                return valueProperty.GetValue(value);
            }
        }

        return value;
    }

    private static string QuoteIdentifier(string identifier) => $"\"{identifier}\"";
}
