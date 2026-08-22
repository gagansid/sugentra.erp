using Sugentra.ERP.Api.Modules.Settings.Repositories;
using Sugentra.ERP.Api.Shared.Contracts;

namespace Sugentra.ERP.Api.Modules.Settings.Services;

public class SystemParameterService(ISystemParameterRepository repository) : ISystemParameterService
{
    public async Task<int> GetIntAsync(string? category, string key, int defaultValue)
    {
        var raw = await repository.GetValueAsync(category, key);
        return int.TryParse(raw, out var value) ? value : defaultValue;
    }

    public async Task<string?> GetStringAsync(string? category, string key, string? defaultValue = null)
    {
        var raw = await repository.GetValueAsync(category, key);
        return string.IsNullOrWhiteSpace(raw) ? defaultValue : raw;
    }
}
