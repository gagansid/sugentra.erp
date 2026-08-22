-- 0014's UQ_MasterData_PriceListLines_Header_Item_Date is a plain UNIQUE constraint, so it still counts
-- soft-deleted rows (IsDeleted = 1). Editing a price list soft-deletes its old lines then re-inserts the
-- full line set (PriceListUseCase.UpdateAsync's replace pattern) — re-inserting the same
-- (PriceListHeaderId, ItemId, EffectiveDate) as a soft-deleted line then throws a duplicate-key violation.
-- Fix: drop the plain constraint, replace with a filtered unique index that only applies to live rows,
-- same pattern as 0017's OnePrimaryPerType indexes.
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_MasterData_PriceListLines_Header_Item_Date')
BEGIN
    ALTER TABLE MasterData_PriceListLines DROP CONSTRAINT UQ_MasterData_PriceListLines_Header_Item_Date;
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_MasterData_PriceListLines_Header_Item_Date')
BEGIN
    CREATE UNIQUE INDEX UQ_MasterData_PriceListLines_Header_Item_Date
        ON MasterData_PriceListLines (PriceListHeaderId, ItemId, EffectiveDate)
        WHERE IsDeleted = 0;
END
