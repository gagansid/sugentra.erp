using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Shared.Logging;

public interface IAuditLogService
{
    Task LogAsync(string tableName, long recordId, string action, string? oldValues, string? newValues, long? changedBy);
}

public class AuditLogService(IDbConnectionFactory connectionFactory) : IAuditLogService
{
    public async Task LogAsync(string tableName, long recordId, string action, string? oldValues, string? newValues, long? changedBy)
    {
        const string sql = """
            INSERT INTO Setting_AuditLogs (TableName, RecordId, Action, OldValues, NewValues, ChangedBy, ChangedAt, CreatedBy)
            VALUES (@TableName, @RecordId, @Action, @OldValues, @NewValues, @ChangedBy, GETDATE(), @ChangedBy)
            """;

        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteCommandAsync(sql, new { TableName = tableName, RecordId = recordId, Action = action, OldValues = oldValues, NewValues = newValues, ChangedBy = changedBy });
    }
}
