namespace Sugentra.ERP.Api.Modules.MasterData.Queries;

public static class BillOfMaterialItemQuery
{
    public const string GetByBomIdSql = """
        SELECT * FROM MasterData_BillOfMaterialItems WHERE BillOfMaterialId = @BillOfMaterialId AND IsDeleted = 0
        """;

    public const string SoftDeleteByBomIdSql = """
        UPDATE MasterData_BillOfMaterialItems
        SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @DeletedBy
        WHERE BillOfMaterialId = @BillOfMaterialId AND IsDeleted = 0
        """;
}
