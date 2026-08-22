namespace Sugentra.ERP.Api.Modules.MasterData.Queries;

public static class PriceListLineQuery
{
    public const string GetByHeaderIdSql = """
        SELECT * FROM MasterData_PriceListLines WHERE PriceListHeaderId = @PriceListHeaderId AND IsDeleted = 0
        """;

    public const string SoftDeleteByHeaderIdSql = """
        UPDATE MasterData_PriceListLines
        SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @DeletedBy
        WHERE PriceListHeaderId = @PriceListHeaderId AND IsDeleted = 0
        """;
}
