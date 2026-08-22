-- Legacy flat price list table replaced by MasterData_PriceListHeaders + MasterData_PriceListLines (see 0013-0015).
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MasterData_PriceLists')
BEGIN
    DROP TABLE MasterData_PriceLists;
END
