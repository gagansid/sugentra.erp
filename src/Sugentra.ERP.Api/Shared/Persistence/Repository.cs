using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using Dapper;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Shared.Persistence;

/// <summary>
/// Generic CRUD base for entities inheriting <see cref="BaseAuditableEntity"/>.
/// Per-entity repositories extend this and add only their specialized lookup methods.
/// Table name comes from [Table("...")] on TEntity; column names are assumed to match property names.
/// </summary>
public abstract class Repository<TEntity>(IDbConnectionFactory connectionFactory)
    where TEntity : BaseAuditableEntity
{
    private static readonly string TableName = ResolveTableName();
    private static readonly string[] WritableColumns = ResolveWritableColumns();

    protected IDbConnectionFactory ConnectionFactory { get; } = connectionFactory;

    public virtual async Task<TEntity?> GetByIdAsync(long id)
    {
        using var connection = ConnectionFactory.CreateConnection();
        var sql = $"SELECT * FROM {TableName} WHERE Id = @Id AND IsDeleted = 0";
        return await connection.QuerySingleAsync<TEntity>(sql, new { Id = id });
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync()
    {
        using var connection = ConnectionFactory.CreateConnection();
        var sql = $"SELECT * FROM {TableName} WHERE IsDeleted = 0";
        return await connection.QueryListAsync<TEntity>(sql);
    }

    public virtual async Task<long> AddAsync(TEntity entity)
    {
        var columns = string.Join(", ", WritableColumns);
        var parameters = string.Join(", ", WritableColumns.Select(c => "@" + c));
        var sql = $"INSERT INTO {TableName} ({columns}) OUTPUT INSERTED.Id VALUES ({parameters})";

        using var connection = ConnectionFactory.CreateConnection();
        var newId = await connection.QueryScalarAsync<long>(sql, entity);
        entity.Id = newId;
        return newId;
    }

    public virtual async Task UpdateAsync(TEntity entity)
    {
        var assignments = string.Join(", ", WritableColumns.Select(c => $"{c} = @{c}"));
        var sql = $"UPDATE {TableName} SET {assignments} WHERE Id = @Id";

        using var connection = ConnectionFactory.CreateConnection();
        await connection.ExecuteCommandAsync(sql, entity);
    }

    public virtual async Task SoftDeleteAsync(long id, long deletedBy)
    {
        const string sql = "UPDATE {0} SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @DeletedBy WHERE Id = @Id";
        using var connection = ConnectionFactory.CreateConnection();
        await connection.ExecuteCommandAsync(string.Format(sql, TableName), new { Id = id, DeletedBy = deletedBy });
    }

    private static string ResolveTableName()
    {
        var attribute = typeof(TEntity).GetCustomAttribute<TableAttribute>();
        return attribute?.Name ?? typeof(TEntity).Name;
    }

    private static string[] ResolveWritableColumns()
    {
        // Id (identity) is auto-generated and excluded from writable columns.
        return typeof(TEntity)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.Name != nameof(BaseAuditableEntity.Id) && p.CanWrite)
            .Select(p => p.Name)
            .ToArray();
    }
}
