namespace Sugentra.ERP.Api.Modules.Identity.Queries;

// SQL text only — no execution here. Repository executes these and returns results (Queries convention).
public static class RoleListQuery
{
  public const string ExistsByNameSql = "SELECT COUNT(1) FROM Identity_Roles WHERE Name = @Name AND IsDeleted = 0";

  public const string GetByIdsSql = "SELECT * FROM Identity_Roles WHERE Id IN @Ids AND IsDeleted = 0";

  // 1 optional filter stays inline (below the SP threshold of "many optional filters").
  public const string GetPagedSql = """
        SELECT Id, Name, Description, IsActive, COUNT(*) OVER() AS TotalCount
        FROM Identity_Roles
        WHERE IsDeleted = 0
          AND (@Name IS NULL OR Name LIKE '%' + @Name + '%')
        ORDER BY Id
        OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
        """;
}
