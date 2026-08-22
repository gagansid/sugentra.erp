namespace Sugentra.ERP.Api.Modules.MasterData.Queries;

public static class BusinessPartnerAddressQuery
{
    public const string GetByPartnerIdSql = """
        SELECT * FROM MasterData_BusinessPartnerAddresses WHERE BusinessPartnerId = @BusinessPartnerId AND IsDeleted = 0
        """;

    public const string SoftDeleteByPartnerIdSql = """
        UPDATE MasterData_BusinessPartnerAddresses
        SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @DeletedBy
        WHERE BusinessPartnerId = @BusinessPartnerId AND IsDeleted = 0
        """;
}
