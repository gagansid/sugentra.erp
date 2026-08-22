namespace Sugentra.ERP.Api.Shared.Common;

/// <summary>Standard audit/soft-delete columns present on every table (see plan.md).</summary>
public abstract class BaseAuditableEntity
{
    public long Id { get; set; }
    // Defaults to now — without this, an unset CreatedAt is DateTime.MinValue (year 1), which SQL Server's
    // DATETIME parameter binding rejects (min year 1753) before the INSERT even reaches the DATETIME2 column.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public long? DeletedBy { get; set; }
}
