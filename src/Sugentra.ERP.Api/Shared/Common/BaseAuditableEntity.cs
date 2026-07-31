namespace Sugentra.ERP.Api.Shared.Common;

/// <summary>Standard audit/soft-delete columns present on every table (see plan.md).</summary>
public abstract class BaseAuditableEntity
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public long CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public long? DeletedBy { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
