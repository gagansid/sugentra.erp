using Sugentra.ERP.Api.Shared.Persistence;
using Module = Sugentra.ERP.Api.Modules.Settings.Entities.Module;

namespace Sugentra.ERP.Api.Modules.Settings.Queries;

/// <summary>Read path: raw Dapper, no business rules — powers the post-login module hub grid.
/// Matches the caller's JWT "permission" claims against Identity_Permissions to find which modules the user
/// may see, preferring the ModuleId FK (Setting_Modules.Id) when populated and falling back to the legacy
/// free-text Module column for any permission row that predates the backfill (see docs/plan.md's
/// ModuleId FK note) - keeps existing/unmigrated data working with zero behavior change.</summary>
public class ModuleQuery(IDbConnectionFactory connectionFactory)
{
    private const string Sql = """
        SELECT DISTINCT m.*
        FROM Setting_Modules m
        WHERE m.IsDeleted = 0
          AND m.IsActive = 1
          AND EXISTS (
              SELECT 1 FROM Identity_Permissions p
              WHERE p.IsDeleted = 0
                AND (p.ModuleId = m.Id OR (p.ModuleId IS NULL AND p.Module = m.Code))
                AND p.Code IN @Codes
          )
        ORDER BY m.SortOrder
        """;

    public async Task<IReadOnlyList<Module>> GetActiveForPermissionCodesAsync(IEnumerable<string> permissionCodes)
    {
        var codes = permissionCodes.ToArray();
        if (codes.Length == 0)
        {
            return [];
        }

        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryListAsync<Module>(Sql, new { Codes = codes });
    }
}
