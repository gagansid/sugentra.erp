using Sugentra.ERP.Api.Modules.Identity.Dtos;
using Sugentra.ERP.Api.Modules.Identity.Entities;
using Sugentra.ERP.Api.Modules.Identity.Queries;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Identity.Repositories;

public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(long id);
    Task<IReadOnlyList<Permission>> GetAllAsync();
    Task<bool> ExistsByCodeAsync(string code);
    Task<PagedResult<PermissionListItemDto>> GetPagedAsync(PermissionListRequest request);
    Task<long> AddAsync(Permission entity);
    Task UpdateAsync(Permission entity);
    Task SoftDeleteAsync(long id, long deletedBy);
}

public class PermissionRepository(IDbConnectionFactory connectionFactory)
    : Repository<Permission>(connectionFactory), IPermissionRepository
{
    // Overridden so Module reflects Setting_Modules.Name (display) instead of the raw legacy code column.
    public override async Task<IReadOnlyList<Permission>> GetAllAsync()
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QueryListAsync<Permission>(PermissionListQuery.GetAllWithModuleNameSql);
    }

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QueryScalarAsync<int>(PermissionListQuery.ExistsByCodeSql, new { Code = code }) > 0;
    }

    public async Task<PagedResult<PermissionListItemDto>> GetPagedAsync(PermissionListRequest request)
    {
        using var connection = ConnectionFactory.CreateConnection();
        var rows = await connection.QueryListAsync<PermissionPagedRow>(PermissionListQuery.GetPagedSql, request);

        return new PagedResult<PermissionListItemDto>
        {
            Items = rows.Select(r => new PermissionListItemDto(r.Id, r.Code, r.Module, r.Description)).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = rows.Count > 0 ? rows[0].TotalCount : 0
        };
    }

    private record PermissionPagedRow(long Id, string Code, string? Module, string? Description, int TotalCount);
}
