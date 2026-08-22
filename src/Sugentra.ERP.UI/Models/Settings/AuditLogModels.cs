namespace Sugentra.ERP.UI.Models.Settings;

public record AuditLogListItemDto(long Id, string TableName, string Module, string ModuleName, string Menu, string MenuName, long RecordId, string Action, string? OldValues, string? NewValues, long? ChangedBy, string? ChangedByName, DateTime ChangedAt);

public record AuditLogListRequest(string? TableName = null, long? RecordId = null, long? ChangedByUserId = null, string? Action = null, string? Module = null, string? Menu = null, DateTime? FromDate = null, DateTime? ToDate = null, int Page = 1, int PageSize = 10);

public record AuditLogModuleOptionDto(string Code, string Name);

public record AuditLogMenuOptionDto(string Code, string Name);

public record AuditLogFilterOptionsDto(IReadOnlyList<AuditLogModuleOptionDto> Modules, IReadOnlyList<AuditLogMenuOptionDto> Menus);

public record AuditLogAdjacentDto(long? PreviousId, long? NextId, long? FirstId, long? LastId);

