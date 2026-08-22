-- Migrate legacy flat MasterData_PriceLists rows into the new header+lines model.
-- One "General Price List (Sales)" header is created per distinct CurrencyId found in the legacy table
-- (legacy data had no Type/BusinessPartner concept, so it defaults to a general Sales list).
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MasterData_PriceLists')
BEGIN
    INSERT INTO MasterData_PriceListHeaders (Name, Type, CurrencyId, BusinessPartnerId, IsActive)
    SELECT DISTINCT 'General Price List (Sales)', 'Sales', pl.CurrencyId, NULL, 1
    FROM MasterData_PriceLists pl
    WHERE NOT EXISTS (
        SELECT 1 FROM MasterData_PriceListHeaders h
        WHERE h.CurrencyId = pl.CurrencyId AND h.Type = 'Sales' AND h.BusinessPartnerId IS NULL
    );

    INSERT INTO MasterData_PriceListLines (PriceListHeaderId, ItemId, Price, EffectiveDate)
    SELECT h.Id, pl.ItemId, pl.Price, pl.EffectiveDate
    FROM MasterData_PriceLists pl
    INNER JOIN MasterData_PriceListHeaders h
        ON h.CurrencyId = pl.CurrencyId AND h.Type = 'Sales' AND h.BusinessPartnerId IS NULL
    WHERE pl.IsDeleted = 0
      AND NOT EXISTS (
          SELECT 1 FROM MasterData_PriceListLines l
          WHERE l.PriceListHeaderId = h.Id AND l.ItemId = pl.ItemId AND l.EffectiveDate = pl.EffectiveDate
      );
END
