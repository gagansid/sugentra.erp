using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Approvals.Entities;

// One active flow config per DocumentType, optionally narrowed by amount range/currency/warehouse
// (see docs/plan.md ApprovalMatrix precedent). Higher Priority is matched first when multiple flows qualify.
[Table("Approval_FlowDefinitions")]
public class ApprovalFlowDefinition : BaseAuditableEntity
{
    public string DocumentType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public long? CurrencyId { get; set; }
    public long? WarehouseId { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
}
