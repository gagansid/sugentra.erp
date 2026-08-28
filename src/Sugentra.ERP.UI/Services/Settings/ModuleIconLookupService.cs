using Microsoft.Extensions.Caching.Memory;

namespace Sugentra.ERP.UI.Services.Settings;

/// <summary>Resolves a module's sidebar icon by Code for breadcrumbs, so views never hardcode a literal
/// icon string - changing Setting_Modules.Icon in the DB (via Modules admin page) updates every breadcrumb
/// that references the module automatically. Whole module list is cached briefly since it barely changes.</summary>
public class ModuleIconLookupService(ModuleApiService moduleApiService, IMemoryCache cache)
{
    private const string CacheKey = "module-icons-by-code";

    public async Task<string> GetIconAsync(string moduleCode, string fallbackIcon = "ri-apps-2-line")
    {
        if (!cache.TryGetValue(CacheKey, out Dictionary<string, string?>? iconsByCode) || iconsByCode is null)
        {
            var result = await moduleApiService.GetAllAsync();
            iconsByCode = result.Success && result.Data is not null
                ? result.Data.ToDictionary(m => m.Code, m => m.Icon)
                : new Dictionary<string, string?>();
            cache.Set(CacheKey, iconsByCode, TimeSpan.FromMinutes(5));
        }

        return iconsByCode.TryGetValue(moduleCode, out var icon) && !string.IsNullOrWhiteSpace(icon)
            ? icon
            : fallbackIcon;
    }
}
