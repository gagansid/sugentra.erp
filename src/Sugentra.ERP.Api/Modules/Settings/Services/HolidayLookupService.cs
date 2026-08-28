using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Shared.Contracts;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Settings.Services;

public class HolidayLookupService(GenericRepository<Holiday> repository) : IHolidayLookupService
{
    public async Task<IReadOnlySet<DateTime>> GetHolidayDatesAsync(DateTime from, DateTime to)
    {
        var all = await repository.GetAllAsync();
        return all
            .Where(h => h.Date.Date >= from.Date && h.Date.Date <= to.Date)
            .Select(h => h.Date.Date)
            .ToHashSet();
    }
}
