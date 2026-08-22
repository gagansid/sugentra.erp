using System.Data;
using Dapper;

namespace Sugentra.ERP.Api.Shared.Persistence;

// Centralizes every Dapper call so Repositories never call connection.* directly.
// All params are passed as objects bound to @Name placeholders (Dapper), never string-concatenated — that's what prevents SQL injection.
public static class DbConnectionExtensions
{
    // Plain SQL — one row (SELECT ... WHERE Id = @Id).
    public static Task<T?> QuerySingleAsync<T>(this IDbConnection connection, string sql, object? param = null) =>
        connection.QuerySingleOrDefaultAsync<T>(sql, param);

    // Plain SQL — many rows (SELECT list/paged).
    public static async Task<List<T>> QueryListAsync<T>(this IDbConnection connection, string sql, object? param = null) =>
        (await connection.QueryAsync<T>(sql, param)).ToList();

    // Plain SQL — scalar value (COUNT/EXISTS checks, INSERT ... OUTPUT INSERTED.Id).
    public static async Task<T> QueryScalarAsync<T>(this IDbConnection connection, string sql, object? param = null) =>
        (await connection.ExecuteScalarAsync<T>(sql, param))!;

    // Plain SQL — INSERT/UPDATE/DELETE, returns affected row count.
    public static Task<int> ExecuteCommandAsync(this IDbConnection connection, string sql, object? param = null) =>
        connection.ExecuteAsync(sql, param);

    // Stored procedure — one row.
    public static Task<T?> QuerySingleStoredProcedureAsync<T>(this IDbConnection connection, string procedureName, object? param = null) =>
        connection.QuerySingleOrDefaultAsync<T>(procedureName, param, commandType: CommandType.StoredProcedure);

    // Stored procedure — many rows.
    public static async Task<List<T>> QueryStoredProcedureAsync<T>(this IDbConnection connection, string procedureName, object? param = null) =>
        (await connection.QueryAsync<T>(procedureName, param, commandType: CommandType.StoredProcedure)).ToList();

    // Stored procedure — INSERT/UPDATE/DELETE via proc, returns affected row count.
    public static Task<int> ExecuteStoredProcedureAsync(this IDbConnection connection, string procedureName, object? param = null) =>
        connection.ExecuteAsync(procedureName, param, commandType: CommandType.StoredProcedure);
}
