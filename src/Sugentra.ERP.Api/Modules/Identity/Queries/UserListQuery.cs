using Dapper;
using Sugentra.ERP.Api.Modules.Identity.Dtos;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Identity.Queries;

public class UserListQuery(IDbConnectionFactory connectionFactory)
{
    public async Task<PagedResult<UserListItemDto>> GetPagedAsync(int page, int pageSize)
    {
        using var connection = connectionFactory.CreateConnection();

        const string countSql = "SELECT COUNT(1) FROM Identity_Users WHERE IsDeleted = 0";
        const string pageSql = """
            SELECT Id, Username, Email, FullName, IsActive
            FROM Identity_Users
            WHERE IsDeleted = 0
            ORDER BY Id
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);
        var items = await connection.QueryAsync<UserListItemDto>(pageSql, new { Offset = (page - 1) * pageSize, PageSize = pageSize });

        return new PagedResult<UserListItemDto>
        {
            Items = items.ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
