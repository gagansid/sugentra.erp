using Sugentra.ERP.Api.Modules.Identity.Dtos;
using Sugentra.ERP.Api.Modules.Identity.Entities;
using Sugentra.ERP.Api.Modules.Identity.Queries;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Identity.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(long id);
    Task<IReadOnlyList<Role>> GetByIdsAsync(IReadOnlyList<long> ids);
    Task<bool> ExistsByNameAsync(string name);
    Task<PagedResult<RoleListItemDto>> GetPagedAsync(RoleListRequest request);
    Task<long> AddAsync(Role entity);
    Task UpdateAsync(Role entity);
    Task SoftDeleteAsync(long id, long deletedBy);
}

public class RoleRepository(IDbConnectionFactory connectionFactory)
    : Repository<Role>(connectionFactory), IRoleRepository
{
    public async Task<IReadOnlyList<Role>> GetByIdsAsync(IReadOnlyList<long> ids)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QueryListAsync<Role>(RoleListQuery.GetByIdsSql, new { Ids = ids });
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        using var connection = ConnectionFactory.CreateConnection();
        return await connection.QueryScalarAsync<int>(RoleListQuery.ExistsByNameSql, new { Name = name }) > 0;
    }

    public async Task<PagedResult<RoleListItemDto>> GetPagedAsync(RoleListRequest request)
    {
        using var connection = ConnectionFactory.CreateConnection();
        var rows = await connection.QueryListAsync<RolePagedRow>(RoleListQuery.GetPagedSql, request);

        return new PagedResult<RoleListItemDto>
        {
            Items = rows.Select(r => new RoleListItemDto(r.Id, r.Name, r.Description, r.IsActive)).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = rows.Count > 0 ? rows[0].TotalCount : 0
        };
    }

    private record RolePagedRow(long Id, string Name, string? Description, bool IsActive, int TotalCount);
}
