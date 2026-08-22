namespace Sugentra.ERP.Api.Modules.MasterData.Queries;

public static class BusinessPartnerContactQuery
{
    public const string GetByPartnerIdSql = """
        SELECT * FROM MasterData_BusinessPartnerContacts WHERE BusinessPartnerId = @BusinessPartnerId AND IsDeleted = 0
        """;

    public const string SoftDeleteByPartnerIdSql = """
        UPDATE MasterData_BusinessPartnerContacts
        SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @DeletedBy
        WHERE BusinessPartnerId = @BusinessPartnerId AND IsDeleted = 0
        """;
}
