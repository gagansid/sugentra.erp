namespace Sugentra.ERP.Api.Shared.Contracts;

/// <summary>Cross-module read-only lookup of the holiday calendar (e.g. for the Approvals module's business-day
/// "Aging" calculation) — other modules depend on this interface only, never on Modules/Settings/** directly,
/// per the module isolation rule in AGENTS.md.</summary>
public interface IHolidayLookupService
{
    /// <summary>Returns the set of holiday dates (date-only, no time component) between <paramref name="from"/>
    /// and <paramref name="to"/> inclusive.</summary>
    Task<IReadOnlySet<DateTime>> GetHolidayDatesAsync(DateTime from, DateTime to);
}
